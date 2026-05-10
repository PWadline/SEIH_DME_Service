
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

namespace Infrastructure.Services.SEIH.Record;

public class SeihFileService : ISeihFileService
{
    private readonly IUsersRepository _usersRepository;
    private readonly ISeihFileRepository _repository;

    public SeihFileService(
        IUsersRepository usersRepository,
        ISeihFileRepository repository)
    {
        _usersRepository = usersRepository;
        _repository = repository;
    }

    public async Task<ServiceResult<UploadFileResponseDto>> UploadAsync(
        ClaimsPrincipal claim,
        UploadFileRequestDto model)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.Unauthorized);

        if (model.File == null || model.File.Length == 0)
            return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest);

        if (string.IsNullOrWhiteSpace(model.Category))
            return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest);

        var folder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "hospitals",
            user.HospitalId.ToString(),
            model.Category.Trim().ToLower()
        );

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var storedName = Guid.NewGuid() +
                         Path.GetExtension(model.File.FileName);

        var fullPath = Path.Combine(folder, storedName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        var entity = new FileStorageEntity
        {
            Id = Guid.NewGuid(),
            HospitalId = user.HospitalId,
            RelatedEntity = model.Category.Trim().ToLower(),
            OriginalFileName = model.File.FileName,
            StoredFileName = storedName,
            Path = Path.Combine(
                "uploads",
                "hospitals",
                user.HospitalId.ToString(),
                model.Category.Trim().ToLower(),
                storedName),
            ContentType = model.File.ContentType,
            Size = model.File.Length,
            Created = DateTime.UtcNow,
            CreatedBy = user.Id.ToString(),
            IsDeleted = false
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return new ServiceResult<UploadFileResponseDto>(
            new UploadFileResponseDto
            {
                FileId = entity.Id,
                OriginalFileName = entity.OriginalFileName
            });
    }

    public async Task<ServiceResult<FileDownloadDto>> DownloadAsync(
        ClaimsPrincipal claim,
        Guid fileId)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<FileDownloadDto>(HttpStatusCode.Unauthorized);

        var file = await _repository.GetByIdAsync(fileId);

        if (file == null || file.IsDeleted == true)
            return new ServiceResult<FileDownloadDto>(HttpStatusCode.NotFound);

        if (file.HospitalId != user.HospitalId)
            return new ServiceResult<FileDownloadDto>(HttpStatusCode.Forbidden);

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            file.Path);

        if (!File.Exists(fullPath))
            return new ServiceResult<FileDownloadDto>(HttpStatusCode.NotFound);

        var bytes = await File.ReadAllBytesAsync(fullPath);

        return new ServiceResult<FileDownloadDto>(
            new FileDownloadDto
            {
                FileBytes = bytes,
                ContentType = file.ContentType,
                FileName = file.OriginalFileName
            });
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
     ClaimsPrincipal claim,
     Guid fileId)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var file = await _repository.GetByIdAsync(fileId);

        if (file == null || file.IsDeleted == true)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        if (file.HospitalId != user.HospitalId)
            return new ServiceResult<bool>(HttpStatusCode.Forbidden);

        // 🔥 Suppression physique
        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            file.Path);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        // 🔥 Soft delete DB
        file.IsDeleted = true;
        file.LastModified = DateTime.UtcNow;
        file.LastModifiedBy = user.Id.ToString();

        await _repository.SaveChangesAsync();

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<FileStorageDto?>> GetByIdAsync(Guid id)
    {
        var file = await _repository.GetByIdAsync(id);

        if (file == null)
            return new ServiceResult<FileStorageDto?>(HttpStatusCode.NotFound);

        return new ServiceResult<FileStorageDto?>(
            new FileStorageDto
            {
                Id = file.Id,
                HospitalId = file.HospitalId,
                RelatedEntity = file.RelatedEntity,
                RelatedEntityId = file.RelatedEntityId,
                OriginalFileName = file.OriginalFileName,
                StoredFileName = file.StoredFileName,
                Path = file.Path,
                ContentType = file.ContentType,
                Size = file.Size
            });
    }

    public async Task<ServiceResult<UploadFileResponseDto>> UploadFromStreamAsync(
     ClaimsPrincipal claim,
     UploadFileFromStreamDto dto)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.Unauthorized);

        if (dto.Stream == null)
            return new ServiceResult<UploadFileResponseDto>(HttpStatusCode.BadRequest);

        var folder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "hospitals",
            user.HospitalId.ToString(),
            dto.Category.Trim().ToLower()
        );

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var storedName = Guid.NewGuid() + Path.GetExtension(dto.FileName);
        var fullPath = Path.Combine(folder, storedName);

        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await dto.Stream.CopyToAsync(fileStream);
        }

        var entity = new FileStorageEntity
        {
            Id = Guid.NewGuid(),
            HospitalId = user.HospitalId,
            RelatedEntity = dto.Category.Trim().ToLower(),
            OriginalFileName = dto.FileName,
            StoredFileName = storedName,
            Path = Path.Combine(
                "uploads",
                "hospitals",
                user.HospitalId.ToString(),
                dto.Category.Trim().ToLower(),
                storedName
            ),
            ContentType = "application/octet-stream",
            Size = new FileInfo(fullPath).Length,
            Created = DateTime.UtcNow,
            CreatedBy = user.Id.ToString(),
            IsDeleted = false
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return new ServiceResult<UploadFileResponseDto>(
            new UploadFileResponseDto
            {
                FileId = entity.Id,
                OriginalFileName = entity.OriginalFileName
            });
    }
}