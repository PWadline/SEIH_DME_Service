using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features;
using Core.Domain.Entity.SEIH;
using System.Net;
using System.Text;
using Core.Application.Interface.Repository.SEIH.Hospital;
using System.Security.Claims;
using System.Security.Cryptography;
using Core.Application.Model.Features.Hospital;
using Core.Application.Interface.Services.SEIH.Record;
using Core.Application.Model.Features.Record;


namespace Infrastructure.Services.SEIH
{
    public class HospitalBranchService : IHospitalBranchService
    {
        private readonly IHospitalBranchRepository _repository;
        private readonly IUsersRepository _usersRepository;
        private readonly ISeihFileService _fileService;

        public HospitalBranchService(
            IHospitalBranchRepository repository,
            IUsersRepository usersRepository,
            ISeihFileService fileService)
        {
            _repository = repository;
            _usersRepository = usersRepository;
            _fileService = fileService;
        }

        public async Task<ServiceResult<List<HospitalBranchDto>>> GetAllByHospitalAsync(
     ClaimsPrincipal claim)
        {
            var email = claim.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _usersRepository.GetUserByEmailAsync(email!);

            if (user == null)
                return new ServiceResult<List<HospitalBranchDto>>(HttpStatusCode.Unauthorized);

            var branches = await _repository.GetByHospitalIdAsync(user.HospitalId);

            var result = new List<HospitalBranchDto>();

            foreach (var b in branches.Where(b => b.IsDeleted != true))
            {
                string? logoPath = null;

                if (b.LogoFileId.HasValue)
                {
                    var fileResult = await _fileService.GetByIdAsync(b.LogoFileId.Value);

                    if (!fileResult.IsError && fileResult.Result != null)
                    {
                        logoPath = fileResult.Result.Path;
                    }
                }

                result.Add(new HospitalBranchDto
                {
                    Id = b.Id,
                    HospitalId = b.HospitalId,
                    DisplayName = b.DisplayName,
                    Info = b.Info,
                    LogoPath = logoPath
                });
            }

            return new ServiceResult<List<HospitalBranchDto>>(result);
        }


        public async Task<ServiceResult<HospitalBranchDto?>> GetByIdAsync(
     ClaimsPrincipal claim,
     Guid id)
        {
            var email = claim.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _usersRepository.GetUserByEmailAsync(email!);

            if (user == null)
                return new ServiceResult<HospitalBranchDto?>(HttpStatusCode.Unauthorized);

            var entity = await _repository.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted == true)
                return new ServiceResult<HospitalBranchDto?>(HttpStatusCode.NotFound);

            if (entity.HospitalId != user.HospitalId)
                return new ServiceResult<HospitalBranchDto?>(HttpStatusCode.Forbidden);

            string? logoPath = null;

            if (entity.LogoFileId.HasValue)
            {
                var fileResult = await _fileService.GetByIdAsync(entity.LogoFileId.Value);

                if (!fileResult.IsError && fileResult.Result != null)
                {
                    logoPath = fileResult.Result.Path;
                }
            }

            return new ServiceResult<HospitalBranchDto?>(
                new HospitalBranchDto
                {
                    Id = entity.Id,
                    HospitalId = entity.HospitalId,
                    DisplayName = entity.DisplayName,
                    Info = entity.Info,
                    LogoPath = logoPath
                });
        }



        public async Task<ServiceResult<HospitalBranchDto>> CreateAsync(
            ClaimsPrincipal claim,
            CreateHospitalBranchDto dto)
        {
            var email = claim.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _usersRepository.GetUserByEmailAsync(email!);

            if (user == null)
                return new ServiceResult<HospitalBranchDto>(HttpStatusCode.Unauthorized);

            Guid? logoFileId = null;

            if (dto.LogoFile != null)
            {
                var uploadResult = await _fileService.UploadAsync(
                    claim,
                    new UploadFileRequestDto
                    {
                        File = dto.LogoFile,
                        Category = "hospital-logo"
                    });

                if (uploadResult.IsError)
                    return new ServiceResult<HospitalBranchDto>(HttpStatusCode.BadRequest);

                logoFileId = uploadResult.Result!.FileId;
            }

            var entity = new HospitalBranchEntity
            {
                Id = Guid.NewGuid(),
                HospitalId = user.HospitalId,
                DisplayName = dto.DisplayName,
                Info = dto.Info,
                LogoFileId = logoFileId,
                Created = DateTime.UtcNow,
                CreatedBy = user.Id.ToString(),
                IsDeleted = false
            };

            await _repository.AddAsync(entity);

            return new ServiceResult<HospitalBranchDto>(
                new HospitalBranchDto
                {
                    Id = entity.Id,
                    HospitalId = entity.HospitalId,
                    DisplayName = entity.DisplayName,
                    Info = entity.Info,
                    LogoPath = null
                });
        }

        public async Task<ServiceResult<bool>> UpdateAsync(
    ClaimsPrincipal claim,
    UpdateHospitalBranchDto dto)
        {
            var email = claim.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _usersRepository.GetUserByEmailAsync(email!);

            if (user == null)
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

            var entity = await _repository.GetByIdAsync(dto.Id);

            if (entity == null || entity.IsDeleted == true)
                return new ServiceResult<bool>(HttpStatusCode.NotFound);

            if (entity.HospitalId != user.HospitalId)
                return new ServiceResult<bool>(HttpStatusCode.Forbidden);

            entity.DisplayName = dto.DisplayName;
            entity.Info = dto.Info;

            // 🔥 Si nouveau logo
            if (dto.LogoFile != null)
            {
                // Supprimer ancien logo si existant
                if (entity.LogoFileId.HasValue)
                {
                    await _fileService.DeleteAsync(claim, entity.LogoFileId.Value);
                }

                var uploadResult = await _fileService.UploadAsync(
                    claim,
                    new UploadFileRequestDto
                    {
                        File = dto.LogoFile,
                        Category = "hospital-logo"
                    });

                if (uploadResult.IsError)
                    return new ServiceResult<bool>(HttpStatusCode.BadRequest);

                entity.LogoFileId = uploadResult.Result!.FileId;
            }

            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = user.Id.ToString();

            await _repository.UpdateAsync(entity);

            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal claim, Guid id)
        {
            var email = claim.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _usersRepository.GetUserByEmailAsync(email!);

            if (user == null)
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

            var entity = await _repository.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted == true)
                return new ServiceResult<bool>(HttpStatusCode.NotFound);

            if (entity.HospitalId != user.HospitalId)
                return new ServiceResult<bool>(HttpStatusCode.Forbidden);

            // 🔥 1️⃣ Supprimer logo lié si existant
            if (entity.LogoFileId.HasValue)
            {
                var deleteFileResult = await _fileService.DeleteAsync(
                    claim,
                    entity.LogoFileId.Value);

                // Optionnel : si suppression fichier échoue on stoppe
                if (deleteFileResult.IsError)
                    return new ServiceResult<bool>(HttpStatusCode.InternalServerError);

                entity.LogoFileId = null;
            }

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = user.Id.ToString();

            await _repository.UpdateAsync(entity);

            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> RemoveLogoAsync(
    ClaimsPrincipal claim,
    Guid branchId)
        {
            // 🔹 1️⃣ Vérifier utilisateur
            var email = claim.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _usersRepository.GetUserByEmailAsync(email!);

            if (user == null)
                return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

            // 🔹 2️⃣ Récupérer la branche
            var entity = await _repository.GetByIdAsync(branchId);

            if (entity == null || entity.IsDeleted == true)
                return new ServiceResult<bool>(HttpStatusCode.NotFound);

            // 🔹 3️⃣ Vérifier que la branche appartient au même hôpital
            if (entity.HospitalId != user.HospitalId)
                return new ServiceResult<bool>(HttpStatusCode.Forbidden);

            // 🔹 4️⃣ Vérifier qu’il y a bien un logo
            if (!entity.LogoFileId.HasValue)
                return new ServiceResult<bool>(HttpStatusCode.BadRequest);

            // 🔹 5️⃣ Supprimer le fichier physiquement + DB
            var deleteResult = await _fileService.DeleteAsync(
                claim,
                entity.LogoFileId.Value);

            if (deleteResult.IsError)
                return new ServiceResult<bool>(HttpStatusCode.InternalServerError);

            // 🔹 6️⃣ Mettre à jour la branche
            entity.LogoFileId = null;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = user.Id.ToString();

            await _repository.UpdateAsync(entity);

            return new ServiceResult<bool>(true);
        }

    }

}
