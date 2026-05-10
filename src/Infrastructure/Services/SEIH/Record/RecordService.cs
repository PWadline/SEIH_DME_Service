
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
using Infrastructure.Repository.SEIH.Record;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.SEIH.Record;

public class RecordService : IRecordService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordFieldValueRepository _recordFieldRepository;
    private readonly ISeihFileService _fileService;
    private readonly ISeihFileRepository _fileRepository;
    private readonly IHospitalBranchRepository _branchRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<RecordService> _logger;

    public RecordService(
    IUsersRepository usersRepository,
    IRecordRepository recordRepository,
    IRecordFieldValueRepository recordFieldRepository,
    ISeihFileService fileService,
    ISeihFileRepository fileRepository,
    IHospitalBranchRepository branchRepository,
    AppDbContext context,
    ILogger<RecordService> logger)
    {
        _usersRepository = usersRepository;
        _recordRepository = recordRepository;
        _recordFieldRepository = recordFieldRepository;
        _context = context;
        _fileService = fileService;
        _fileRepository = fileRepository;
        _branchRepository = branchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<RecordDto>> CreateAsync(ClaimsPrincipal claim, CreateRecordDto dto)
    {
        _logger.LogInformation("DTO HospitalBranchId reçu = {branchId}", dto.HospitalBranchId);

        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<RecordDto>(HttpStatusCode.Unauthorized);

        if (dto.HospitalBranchId == Guid.Empty)
            return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest);

        var branch = await _branchRepository.GetByIdAsync(dto.HospitalBranchId);

        if (branch == null || branch.IsDeleted == true)
            return new ServiceResult<RecordDto>(HttpStatusCode.NotFound);

        if (branch.HospitalId != user.HospitalId)
            return new ServiceResult<RecordDto>(HttpStatusCode.Forbidden);

        if (dto.FieldValues == null || !dto.FieldValues.Any())
            return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest);

        // ===============================
        // 🔥 DEBUG FIELDS
        // ===============================
        foreach (var field in dto.FieldValues)
        {
            _logger.LogInformation("FIELD → Label: {label}, Value: {value}", field.FieldLabel, field.Value);
        }

        // ===============================
        // 🔥 EXTRACTION prénom / nom (SAFE)
        // ===============================
        string? firstName = dto.FieldValues
            .FirstOrDefault(f =>
                !string.IsNullOrWhiteSpace(f.FieldLabel) &&
                f.FieldLabel.Equals("Prénom", StringComparison.OrdinalIgnoreCase)
            )?.Value;

        string? lastName = dto.FieldValues
            .FirstOrDefault(f =>
                !string.IsNullOrWhiteSpace(f.FieldLabel) &&
                f.FieldLabel.Equals("Nom", StringComparison.OrdinalIgnoreCase)
            )?.Value;

        _logger.LogInformation("FirstName: {firstName}, LastName: {lastName}", firstName, lastName);

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            _logger.LogWarning("Nom ou prénom manquant dans FieldValues");
            return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest);
            //  return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest, "Nom ou prénom requis");
        }

        var patientReference = GeneratePatientReference(firstName, lastName);
        var hashedCode = Hash(patientReference);

        // ===============================
        // 🔥 CREATE RECORD
        // ===============================
        var record = new RecordEntity
        {
            Id = Guid.NewGuid(),
            HospitalBranchId = dto.HospitalBranchId,

            TemplateName = dto.TemplateName,
            PatientReference = patientReference,
            LogoSize = dto.LogoSize,

            // 🔐 CODE = patientReference hashé
            PatientAccessCodeHash = hashedCode,
            PatientAccessCodeUpdatedAt = DateTime.UtcNow,
            IsNewCodeRequired = true, // 🔥 IMPORTANT

            CreatedBy = user.Id.ToString(),
            Created = DateTime.UtcNow,
            IsDeleted = false
        };

        await _recordRepository.AddAsync(record);
        _logger.LogInformation("Record created with Id {id}", record.Id);

        // ===============================
        // 🔥 CREATE FIELDS
        // ===============================
        foreach (var field in dto.FieldValues)
        {
            Guid? fileId = null;

            if (field.File != null)
            {
                var uploadResult = await _fileService.UploadAsync(
                    claim,
                    new UploadFileRequestDto
                    {
                        File = field.File,
                        Category = "record-files"
                    });

                if (uploadResult.IsError)
                    return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest);

                fileId = uploadResult.Result!.FileId;
            }

            var entity = new RecordFieldValueEntity
            {
                Id = Guid.NewGuid(),
                RecordId = record.Id,
                FieldLabel = field.FieldLabel,
                FieldType = field.FieldType,
                Section = field.Section,
                Position = field.Position,
                Value = fileId == null ? field.Value : null,
                FileId = fileId,
                Created = DateTime.UtcNow,
                CreatedBy = user.Id.ToString(),
                IsDeleted = false
            };

            await _recordFieldRepository.CreateAsync(entity);
        }

        // ===============================
        // 🔥 RESPONSE
        // ===============================
        var result = new RecordDto
        {
            Id = record.Id,
            HospitalId = record.HospitalBranchId,
            Created = record.Created
        };

        return new ServiceResult<RecordDto>(result);
    }


    public async Task<ServiceResult<RecordDto>> GetByIdAsync(ClaimsPrincipal claim, Guid id)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<RecordDto>(HttpStatusCode.Unauthorized);

        var record = await _recordRepository.GetByIdAsync(id);

        if (record == null || record.IsDeleted == true)
            return new ServiceResult<RecordDto>(HttpStatusCode.NotFound);

        var branch = await _branchRepository.GetByIdAsync(record.HospitalBranchId);

        if (branch == null || branch.HospitalId != user.HospitalId)
            return new ServiceResult<RecordDto>(HttpStatusCode.Forbidden);

        var values = await _recordFieldRepository.GetByRecordIdAsync(record.Id);

        values = values
            .Where(v => v.IsDeleted != true)
            .OrderBy(v => v.Position)
            .ToList();

        var fileIds = values
            .Where(v => v.FileId.HasValue)
            .Select(v => v.FileId!.Value)
            .ToList();

        if (branch.LogoFileId.HasValue)
        {
            fileIds.Add(branch.LogoFileId.Value);
        }

        fileIds = fileIds
            .Distinct()
            .ToList();

        var files = await _fileRepository.GetByIdsAsync(fileIds);

        var fileDict = files.ToDictionary(f => f.Id, f => f);

        FileStorageEntity? logoFile = null;

        if (branch.LogoFileId.HasValue)
        {
            fileDict.TryGetValue(branch.LogoFileId.Value, out logoFile);
        }

        var dto = new RecordDto
        {
            Id = record.Id,
            HospitalId = record.HospitalBranchId,

            TemplateName = record.TemplateName,
            PatientReference = record.PatientReference,
            LogoSize = record.LogoSize,

            Created = record.Created,
            IsTransferred = record.IsTransferred,
            TransferId = record.TransferId,

            // 🔥 HÔPITAL
            HospitalName = branch.DisplayName,
            HospitalInfo = branch.Info,
            LogoPath = logoFile?.Path,

            FieldValues = values.Select(v =>
            {
                fileDict.TryGetValue(v.FileId ?? Guid.Empty, out var file);

                return new RecordFieldValueDto
                {
                    Id = v.Id,
                    FieldLabel = v.FieldLabel,
                    FieldType = v.FieldType,
                    Section = v.Section,
                    Position = v.Position,
                    Value = v.Value,
                    FileId = v.FileId,

                    // 🔥 IMPORTANT
                    FilePath = file?.Path
                };
            }).ToList()
        };

        return new ServiceResult<RecordDto>(dto);
    }
    public async Task<ServiceResult<RecordDto>> UpdateAsync(ClaimsPrincipal claim, UpdateRecordDto dto)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<RecordDto>(HttpStatusCode.Unauthorized);

        var record = await _recordRepository.GetByIdAsync(dto.Id);

        if (record == null || record.IsDeleted == true)
            return new ServiceResult<RecordDto>(HttpStatusCode.NotFound);

        var branch = await _branchRepository.GetByIdAsync(record.HospitalBranchId);

        if (branch == null || branch.HospitalId != user.HospitalId)
            return new ServiceResult<RecordDto>(HttpStatusCode.Forbidden);

        record.TemplateName = dto.TemplateName;
        record.PatientReference = dto.PatientReference;
        record.LogoSize = dto.LogoSize;
        record.LastModified = DateTime.UtcNow;
        record.LastModifiedBy = user.Id.ToString();

        await _recordRepository.UpdateAsync(record);

        var existingValues = await _recordFieldRepository.GetByRecordIdAsync(record.Id);

        foreach (var val in existingValues.Where(v => v.IsDeleted != true))
        {
            if (val.FileId.HasValue)
                await _fileService.DeleteAsync(claim, val.FileId.Value);

            val.IsDeleted = true;
            await _recordFieldRepository.UpdateAsync(val);
        }

        await _context.SaveChangesAsync();

        foreach (var field in dto.FieldValues)
        {
            Guid? fileId = field.FileId;


            if (field.File != null)
            {
                var uploadResult = await _fileService.UploadAsync(
                    claim,
                    new UploadFileRequestDto
                    {
                        File = field.File,
                        Category = "record-files"
                    });

                if (uploadResult.IsError)
                    return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest);

                fileId = uploadResult.Result!.FileId;
            }

            var entity = new RecordFieldValueEntity
            {
                Id = Guid.NewGuid(),
                RecordId = record.Id,

                FieldLabel = field.FieldLabel,
                FieldType = field.FieldType,
                Section = field.Section,
                Position = field.Position,

                FileId = fileId,
                Value = fileId == null ? field.Value : null,

                Created = DateTime.UtcNow,
                CreatedBy = user.Id.ToString(),
                IsDeleted = false
            };

            await _recordFieldRepository.CreateAsync(entity);
        }

        return new ServiceResult<RecordDto>(new RecordDto
        {
            Id = record.Id,
            HospitalId = record.HospitalBranchId,
            Created = record.Created
        });
    }

    public async Task<ServiceResult<IEnumerable<RecordDto>>> GetByHospitalAsync(ClaimsPrincipal claim)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return new ServiceResult<IEnumerable<RecordDto>>(HttpStatusCode.Unauthorized);

        var user = await _usersRepository.GetUserByEmailAsync(email);

        if (user == null)
            return new ServiceResult<IEnumerable<RecordDto>>(HttpStatusCode.Unauthorized);

        var branches = await _branchRepository.GetByHospitalIdAsync(user.HospitalId);
        var branchDict = branches
            .Where(b => b.IsDeleted != true)
            .ToDictionary(b => b.Id, b => b);

        var branchIds = branches
            .Where(b => b.IsDeleted != true)
            .Select(b => b.Id)
            .ToList();

        if (!branchIds.Any())
            return new ServiceResult<IEnumerable<RecordDto>>(new List<RecordDto>());

        var records = await _recordRepository.GetByHospitalIdAsync(user.HospitalId);

        var result = new List<RecordDto>();

        foreach (var r in records)
        {
            var values = await _recordFieldRepository.GetByRecordIdAsync(r.Id);

            values = values
                .Where(v => v.IsDeleted != true)
                .OrderBy(v => v.Position)
                .ToList();

            var fileIds = values
                .Where(v => v.FileId.HasValue)
                .Select(v => v.FileId!.Value)
                .ToList();

            var logoFileIds = branchDict.Values
                .Where(b => b.LogoFileId.HasValue)
                .Select(b => b.LogoFileId!.Value)
                .ToList();

            fileIds = fileIds
                .Concat(logoFileIds)
                .Distinct()
                .ToList();

            var files = await _fileRepository.GetByIdsAsync(fileIds);

            var fileDict = files.ToDictionary(f => f.Id, f => f);

            branchDict.TryGetValue(r.HospitalBranchId, out var branch);

            FileStorageEntity? logoFile = null;

            if (branch?.LogoFileId != null)
            {
                fileDict.TryGetValue(branch.LogoFileId.Value, out logoFile);
            }

            result.Add(new RecordDto
            {
                Id = r.Id,
                HospitalId = r.HospitalBranchId,
                TemplateName = r.TemplateName,
                PatientReference = r.PatientReference,
                LogoSize = r.LogoSize,
                Created = r.Created,
                IsTransferred = r.IsTransferred,
                LogoPath = logoFile?.Path,
                TransferId = r.TransferId,
                HospitalName = branch?.DisplayName,
                HospitalInfo = branch?.Info,
                FieldValues = values.Select(v =>

                {
                    fileDict.TryGetValue(v.FileId ?? Guid.Empty, out var file);

                    return new RecordFieldValueDto
                    {
                        Id = v.Id,
                        FieldLabel = v.FieldLabel,
                        FieldType = v.FieldType,
                        Section = v.Section,
                        Position = v.Position,
                        Value = v.Value,
                        FileId = v.FileId,
                        FilePath = file?.Path
                    };
                }).ToList()
            });
        }

        return new ServiceResult<IEnumerable<RecordDto>>(result);
    }


    public async Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal claim, Guid id)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var record = await _recordRepository.GetByIdAsync(id);

        if (record == null || record.IsDeleted == true)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        var branch = await _branchRepository.GetByIdAsync(record.HospitalBranchId);

        if (branch == null || branch.HospitalId != user.HospitalId)
            return new ServiceResult<bool>(HttpStatusCode.Forbidden);

        var values = await _recordFieldRepository
            .GetByRecordIdAsync(record.Id);

        var activeValues = values
            .Where(v => v.IsDeleted != true)
            .ToList();

        foreach (var val in activeValues)
        {
            if (val.FileId.HasValue)
            {
                var deleteResult = await _fileService.DeleteAsync(claim, val.FileId.Value);

                if (deleteResult.IsError)
                {

                    return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
                }
            }

            val.IsDeleted = true;
            val.LastModified = DateTime.UtcNow;
            val.LastModifiedBy = user.Id.ToString();

            await _recordFieldRepository.UpdateAsync(val);
        }

        // 🔥 RESET IMPORT STATE SI DOSSIER IMPORTÉ
        if (record.TransferId.HasValue)
        {
            var transfer = await _context.Set<TransferEntity>()
                .FirstOrDefaultAsync(t => t.Id == record.TransferId.Value);

            if (transfer != null)
            {
                transfer.IsImported = false;
                transfer.RecordId = null;

                transfer.LastModified = DateTime.UtcNow;
                transfer.LastModifiedBy = user.Id.ToString();
            }
        }

        record.IsDeleted = true;
        record.LastModified = DateTime.UtcNow;
        record.LastModifiedBy = user.Id.ToString();

        await _recordRepository.UpdateAsync(record);

        return new ServiceResult<bool>(true);
    }






    public async Task<ServiceResult<RecordDto>> ImportFromTransferAsync(ClaimsPrincipal claim, ImportTransferredRecordDto dto, Guid branchId, ZipArchive archive, Guid transferId)
    {
        _logger.LogInformation("Creating record for patient {ref}", dto.PatientReference);

        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<RecordDto>(HttpStatusCode.Unauthorized);

        // 📦 Récupérer tous les fichiers réellement présents dans le ZIP
        var zipFileSet = archive.Entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Name)) // évite dossiers
            .Select(e => e.FullName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation("ZIP contains {count} files", zipFileSet.Count);

        // 🔒 Démarrer une transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            var record = new RecordEntity
            {
                Id = Guid.NewGuid(),
                HospitalBranchId = branchId,
                TransferId = transferId,
                TemplateName = "gris",
                LogoSize = 70,
                PatientReference = dto.PatientReference,
                IsTransferred = true,
                Created = DateTime.UtcNow,
                CreatedBy = user.Id.ToString(),
                IsDeleted = false
            };

            await _recordRepository.AddAsync(record);

            int position = 0;

            // 🔁 Parcourir les sections et champs
            foreach (var section in dto.Sections)
            {
                foreach (var field in section.Fields)
                {
                    Guid? fileId = null;

                    // 📦 Gestion des fichiers référencés
                    if (!string.IsNullOrWhiteSpace(field.FilePath))
                    {
                        _logger.LogInformation("Checking file in ZIP: {path}", field.FilePath);

                        // ❌ Si le fichier référencé n’existe pas dans le ZIP → stop
                        if (!zipFileSet.Contains(field.FilePath))
                        {
                            _logger.LogError(
                                "Missing file in ZIP referenced by manifest: {path}",
                                field.FilePath);

                            await transaction.RollbackAsync();
                            return new ServiceResult<RecordDto>(HttpStatusCode.BadRequest);
                        }

                        var entry = archive.GetEntry(field.FilePath);

                        if (entry != null)
                        {
                            using var stream = entry.Open();

                            var uploadResult = await _fileService.UploadFromStreamAsync(
                                claim,
                                new UploadFileFromStreamDto
                                {
                                    Stream = stream,
                                    FileName = field.FileName ?? Guid.NewGuid().ToString(),
                                    Category = "record-files"
                                });

                            if (uploadResult.IsError)
                            {
                                _logger.LogError("Upload failed for file {file}", field.FileName);
                                throw new Exception($"Erreur lors de l'upload du fichier {field.FileName}");
                            }

                            fileId = uploadResult.Result!.FileId;
                        }
                    }

                    // 🧩 Création du champ
                    var fieldValue = new RecordFieldValueEntity
                    {
                        Id = Guid.NewGuid(),
                        RecordId = record.Id,
                        Section = section.Label,
                        FieldLabel = field.Label,
                        FieldType = field.Type,
                        Value = fileId == null ? field.Value : null,
                        FileId = fileId,
                        Position = position++,
                        Created = DateTime.UtcNow,
                        CreatedBy = user.Id.ToString(),
                        IsDeleted = false
                    };

                    await _recordFieldRepository.CreateAsync(fieldValue);
                }
            }

            // 💾 Sauvegarde finale
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Record imported successfully: {id}", record.Id);

            // 📤 Retour
            return new ServiceResult<RecordDto>(new RecordDto
            {
                Id = record.Id,
                HospitalId = record.HospitalBranchId,
                TemplateName = record.TemplateName,
                PatientReference = record.PatientReference,
                Created = record.Created
            });
        }
        catch (Exception ex)
        {
            // 🔄 Rollback complet
            await transaction.RollbackAsync();

            _logger.LogError(ex, "Erreur lors de l'import du transfert");
            return new ServiceResult<RecordDto>(HttpStatusCode.InternalServerError);
        }
    }


    private static string GeneratePatientReference(string firstName, string lastName)
    {
        string normalizedFirst = RemoveDiacritics(firstName).ToLower();
        string normalizedLast = RemoveDiacritics(lastName).ToLower();

        return $"{normalizedFirst[0]}{normalizedLast}";
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = Char.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string? GetFieldValue(List<CreateRecordFieldValueDto> fields, string label)
    {
        return fields
            .FirstOrDefault(f =>
                !string.IsNullOrWhiteSpace(f.FieldLabel) &&
                f.FieldLabel.Trim().Equals(label, StringComparison.OrdinalIgnoreCase)
            )?.Value;
    }

    private static string Hash(string value)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public async Task<ServiceResult<bool>> ResetPatientCodeAsync(ResetPatientCodeDto dto)
    {
        _logger.LogInformation("🔁 Reset code for record: {id}", dto.RecordId);

        var record = await _recordRepository.GetByIdAsync(dto.RecordId);

        if (record == null || record.IsDeleted == true)
        {
            _logger.LogWarning("❌ Record not found");
            return new ServiceResult<bool>(HttpStatusCode.NotFound);
        }

        if (string.IsNullOrWhiteSpace(record.PatientReference))
        {
            _logger.LogWarning("❌ PatientReference missing");
            return new ServiceResult<bool>(HttpStatusCode.BadRequest);
        }

        // 🔐 Nouveau code = patientReference hashé
        var newHash = Hash(record.PatientReference);

        record.PatientAccessCodeHash = newHash;
        record.PatientAccessCodeUpdatedAt = DateTime.UtcNow;
        record.IsNewCodeRequired = true;

        await _recordRepository.UpdateAsync(record);

        _logger.LogInformation("✅ Code reset successfully");

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<List<HospitalEntity>>> GetAuthorizedHospitalsAsync(Guid recordId)
    {
        var hospitals = await _recordRepository.GetAuthorizedHospitalsByRecordIdAsync(recordId);

        return new ServiceResult<List<HospitalEntity>>(hospitals);
    }
}


