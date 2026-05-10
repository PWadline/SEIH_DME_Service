
using AutoMapper;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH.Record;
using Core.Application.Model.Features.Record;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using System.Net;
using System.Security.Claims;
using Core.Application.Interface;
using Core.Application.Interface.Services.SEIH;
using Microsoft.Extensions.Logging;
using Infrastructure.Services.SEIH.User;

namespace Infrastructure.Services.SEIH.Record;

public class FormService : IFormService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IFormRepository _formRepo;
    private readonly IRepository<FormFieldEntity> _fieldRepo;
    private readonly IHospitalBranchRepository _branchRepository;
    private readonly ILogger<UserService> _logger;
    private readonly ISeihFileRepository _fileRepository;


    public FormService(
    IUsersRepository usersRepository,
    IFormRepository formRepo,
    IRepository<FormFieldEntity> fieldRepo,
    IHospitalBranchRepository branchRepository, ISeihFileRepository fileRepository,
    ILogger<UserService> logger)
    {
        _usersRepository = usersRepository;
        _formRepo = formRepo;
        _fieldRepo = fieldRepo;
        _branchRepository = branchRepository;
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<ServiceResult<FormDto>> CreateAsync(ClaimsPrincipal claim, CreateFormDto dto)
    {
        _logger.LogInformation("HospitalBranchId reçu = {id}", dto.HospitalBranchId);
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<FormDto>(HttpStatusCode.Unauthorized);

        if (dto.HospitalBranchId == Guid.Empty)
            return new ServiceResult<FormDto>(HttpStatusCode.BadRequest);

        var branch = await _branchRepository.GetByIdAsync(dto.HospitalBranchId);

        if (branch == null || branch.IsDeleted == true)
            return new ServiceResult<FormDto>(HttpStatusCode.NotFound);

        if (branch.HospitalId != user.HospitalId)
            return new ServiceResult<FormDto>(HttpStatusCode.Forbidden);

        var form = new FormEntity
        {
            Id = Guid.NewGuid(),
            HospitalBranchId = dto.HospitalBranchId,
            FormName = dto.FormName,
            TemplateName = dto.TemplateName,
            LogoSize = dto.LogoSize,
            Created = DateTime.UtcNow,
            CreatedBy = user.Id.ToString(),
            IsDeleted = false
        };

        try
        {
            await _formRepo.AddAsync(form);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR CREATING FORM");
            throw;
        }

        foreach (var field in dto.Fields)
        {
            var entity = new FormFieldEntity
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                FieldLabel = field.FieldLabel,
                FieldType = field.FieldType,
                Section = field.Section,
                Position = field.Position,
                IsRequired = field.IsRequired,
                Created = DateTime.UtcNow,
                CreatedBy = user.Id.ToString(),
                IsDeleted = false
            };

            await _fieldRepo.CreateAsync(entity);
        }

        return new ServiceResult<FormDto>(new FormDto
        {
            Id = form.Id,
            HospitalBranchId = form.HospitalBranchId,
            FormName = form.FormName,
            TemplateName = form.TemplateName,
            LogoSize = form.LogoSize
        });
    }


    public async Task<ServiceResult<FormDto>> GetByIdAsync(ClaimsPrincipal claim, Guid id)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<FormDto>(HttpStatusCode.Unauthorized);

        var form = await _formRepo.GetByIdAsync(id);

        if (form == null || form.IsDeleted == true)
            return new ServiceResult<FormDto>(HttpStatusCode.NotFound);

        var branch = await _branchRepository.GetByIdAsync(form.HospitalBranchId);

        if (branch == null || branch.HospitalId != user.HospitalId)
            return new ServiceResult<FormDto>(HttpStatusCode.Forbidden);

        var fields = await _fieldRepo.FindListAsync(f =>
            f.FormId == form.Id && f.IsDeleted == false);

        return new ServiceResult<FormDto>(new FormDto
        {
            Id = form.Id,
            HospitalBranchId = form.HospitalBranchId,
            FormName = form.FormName,
            TemplateName = form.TemplateName,
            LogoSize = form.LogoSize,
            FieldValues = fields
                .OrderBy(f => f.Position)
                .Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    FieldLabel = f.FieldLabel,
                    FieldType = f.FieldType,
                    Section = f.Section,
                    Position = f.Position,
                    IsRequired = f.IsRequired
                }).ToList()
        });
    }


    public async Task<ServiceResult<FormDto>> UpdateAsync(ClaimsPrincipal claim, UpdateFormDto dto)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<FormDto>(HttpStatusCode.Unauthorized);

        var form = await _formRepo.GetByIdAsync(dto.Id);

        if (form == null || form.IsDeleted == true)
            return new ServiceResult<FormDto>(HttpStatusCode.NotFound);

        var branch = await _branchRepository.GetByIdAsync(form.HospitalBranchId);

        if (branch == null || branch.IsDeleted == true)
            return new ServiceResult<FormDto>(HttpStatusCode.NotFound);

        if (branch.HospitalId != user.HospitalId)
            return new ServiceResult<FormDto>(HttpStatusCode.Forbidden);

        form.FormName = dto.FormName;
        form.TemplateName = dto.TemplateName;
        form.LogoSize = dto.LogoSize;
        form.LastModified = DateTime.UtcNow;
        form.LastModifiedBy = user.Id.ToString();

        await _formRepo.UpdateAsync(form);

        var existingFields = await _fieldRepo.FindListAsync(f =>
            f.FormId == form.Id && f.IsDeleted == false);

        foreach (var field in existingFields)
        {
            field.IsDeleted = true;
            field.LastModified = DateTime.UtcNow;
            field.LastModifiedBy = user.Id.ToString();
            await _fieldRepo.UpdateAsync(field);
        }

        foreach (var field in dto.Fields)
        {
            var entity = new FormFieldEntity
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                FieldLabel = field.FieldLabel,
                FieldType = field.FieldType,
                Section = field.Section,
                Position = field.Position,
                IsRequired = field.IsRequired,
                Created = DateTime.UtcNow,
                CreatedBy = user.Id.ToString(),
                IsDeleted = false
            };

            await _fieldRepo.CreateAsync(entity);
        }

        return new ServiceResult<FormDto>(new FormDto
        {
            Id = form.Id,
            HospitalBranchId = form.HospitalBranchId,
            FormName = form.FormName,
            TemplateName = form.TemplateName,
            LogoSize = form.LogoSize
        });
    }

    public async Task<ServiceResult<IEnumerable<FormDto>>> GetByHospitalAsync(ClaimsPrincipal claim)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return new ServiceResult<IEnumerable<FormDto>>(HttpStatusCode.Unauthorized);

        var user = await _usersRepository.GetUserByEmailAsync(email);

        if (user == null)
            return new ServiceResult<IEnumerable<FormDto>>(HttpStatusCode.Unauthorized);

        var branches = await _branchRepository.GetByHospitalIdAsync(user.HospitalId);

        var branchDict = branches
            .Where(b => b.IsDeleted != true)
            .ToDictionary(b => b.Id, b => b);

        var branchIds = branchDict.Keys.ToList();

        if (!branchIds.Any())
            return new ServiceResult<IEnumerable<FormDto>>(new List<FormDto>());

        var forms = await _formRepo.GetByHospitalIdAsync(user.HospitalId);

        var result = new List<FormDto>();

        foreach (var form in forms)
        {
            branchDict.TryGetValue(form.HospitalBranchId, out var branch);

            var fields = await _fieldRepo.FindListAsync(f =>
                f.FormId == form.Id && f.IsDeleted == false);

            FileStorageEntity? logoFile = null;

            if (branch?.LogoFileId != null)
            {
                var files = await _fileRepository.GetByIdsAsync(
                    new List<Guid> { branch.LogoFileId.Value });

                logoFile = files.FirstOrDefault();
            }

            result.Add(new FormDto
            {
                Id = form.Id,
                HospitalBranchId = form.HospitalBranchId,
                FormName = form.FormName,
                TemplateName = form.TemplateName,
                LogoSize = form.LogoSize,

                HospitalName = branch?.DisplayName,
                HospitalInfo = branch?.Info,
                LogoPath = logoFile?.Path,

                FieldValues = fields
                    .OrderBy(f => f.Position)
                    .Select(f => new FormFieldDto
                    {
                        Id = f.Id,
                        FieldLabel = f.FieldLabel,
                        FieldType = f.FieldType,
                        Section = f.Section,
                        Position = f.Position,
                        IsRequired = f.IsRequired
                    })
                    .ToList()
            });
        }

        return new ServiceResult<IEnumerable<FormDto>>(result);
    }


    public async Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal claim, Guid id)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var form = await _formRepo.GetByIdAsync(id);

        if (form == null || form.IsDeleted == true)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        var branch = await _branchRepository.GetByIdAsync(form.HospitalBranchId);

        if (branch == null || branch.HospitalId != user.HospitalId)
            return new ServiceResult<bool>(HttpStatusCode.Forbidden);

        form.IsDeleted = true;
        form.LastModified = DateTime.UtcNow;
        form.LastModifiedBy = user.Id.ToString();

        await _formRepo.UpdateAsync(form);

        return new ServiceResult<bool>(true);
    }

}

