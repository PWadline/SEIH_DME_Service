using Application.Abstractions;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Transfer;
using Core.Application.Model.Features;
using Core.Application.Model.Features.Hospital;
using Core.Domain.Entity.SEIH;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services.SEIH.Hospital;

public class TransferService : ITransferService
{
    private readonly ISeihTransferClient _client;
    private readonly IUsersRepository _usersRepository;
    private readonly IHospitalRepository _hospitalRepository;
    private readonly ISeihPackageBuilder _packageBuilder;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly IRecordRepository _recordRepository;
    private readonly ILogger<TransferService> _logger;
    private readonly ISignatureEnvelopeBuilder _envelopeBuilder;
    private readonly ISeihCryptoService _cryptoService;

    public TransferService(
        ISeihTransferClient client,
        IUsersRepository usersRepository,
        IHospitalRepository hospitalRepository,
        ISeihPackageBuilder packageBuilder,
        IConfiguration config,
        AppDbContext context,
        IRecordRepository recordRepository,
        ILogger<TransferService> logger,
        ISignatureEnvelopeBuilder envelopeBuilder,
        ISeihCryptoService cryptoService)
    {
        _client = client;
        _usersRepository = usersRepository;
        _hospitalRepository = hospitalRepository;
        _packageBuilder = packageBuilder;
        _config = config;
        _context = context;
        _recordRepository = recordRepository;
        _logger = logger;
        _envelopeBuilder = envelopeBuilder;
        _cryptoService = cryptoService;
    }

    public async Task<ServiceResult<bool>> CreateTransferAsync(ClaimsPrincipal claim, CreateTransferDto request)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var sourceHospital = await _hospitalRepository.GetHospitalByIdAsync(user.HospitalId);
        var targetHospital = await _hospitalRepository.GetHospitalByIdAsync(request.HospitalToId);

        if (sourceHospital == null || targetHospital == null)
            return new ServiceResult<bool>(HttpStatusCode.BadRequest);

        var targetKey = await _context.TransferInstitutionKeys
            .Where(k => k.HospitalId == targetHospital.Id && k.IsActive)
            .OrderByDescending(k => k.KeyVersion)
            .FirstOrDefaultAsync();

        if (targetKey == null)
            return new ServiceResult<bool>(HttpStatusCode.BadRequest);

        var record = await _context.Records
            .Include(r => r.FieldValues)
            .FirstOrDefaultAsync(r => r.Id == request.RecordId);

        if (record == null)
            return new ServiceResult<bool>(HttpStatusCode.BadRequest);

        var isAuthorized = await _recordRepository
    .IsHospitalAuthorizedForRecordAsync(request.RecordId, request.HospitalToId);

        if (!isAuthorized)
        {
            return new ServiceResult<bool>(
                false,
                false,
                HttpStatusCode.Forbidden,
                "Consentement non accordé pour cette institution."
            );
        }

        // ===============================
        // 📦 Construire le manifest
        // ===============================
        var package = _packageBuilder.Build(
            new HospitalBasicInfo { Name = sourceHospital.Name, Code = sourceHospital.Code ?? "" },
            new HospitalBasicInfo { Name = targetHospital.Name, Code = targetHospital.Code ?? "" },
            record
        );

        package.Message = request.Message;

        var folderName = $"TR_{sourceHospital.Code}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var tempPath = Path.Combine(Path.GetTempPath(), folderName);

        Directory.CreateDirectory(tempPath);
        Directory.CreateDirectory(Path.Combine(tempPath, "files"));

        // ===============================
        // 📄 PDF optionnel
        // ===============================
        if (!string.IsNullOrEmpty(request.Pdf))
        {
            var pdfBytes = Convert.FromBase64String(request.Pdf);
            await File.WriteAllBytesAsync(
                Path.Combine(tempPath, "clinical_summary.pdf"),
                pdfBytes
            );
        }

        // ===============================
        // 📁 Copier les fichiers + mapping
        // ===============================
        // clé = GUID original du fichier
        // valeur = chemin dans le ZIP
        var fileNameMap = new Dictionary<string, string>();
        int fileIndex = 1;

        foreach (var field in record.FieldValues)
        {
            if (field.FileId is not Guid fileId || fileId == Guid.Empty)
                continue;

            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);
            if (file == null) continue;

            var sourcePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                file.Path
            );

            if (!File.Exists(sourcePath)) continue;

            var extension = Path.GetExtension(sourcePath);

            var zipFileName = $"Fichier_{fileIndex:D3}{extension}";
            var zipRelativePath = $"files/{zipFileName}";

            var destination = Path.Combine(tempPath, zipRelativePath);
            File.Copy(sourcePath, destination, true);

            // 🔥 clé = GUID du fichier (sans extension)
            fileNameMap[file.Id.ToString()] = zipRelativePath;

            fileIndex++;
        }

        // ===============================
        // ✏️ Réécriture du manifest
        // ===============================
        foreach (var section in package.Sections)
        {
            foreach (var field in section.Fields)
            {
                if (!string.IsNullOrWhiteSpace(field.FilePath))
                {
                    // ex: files/3af46cc7-...
                    var storedId = Path.GetFileNameWithoutExtension(field.FilePath);

                    if (fileNameMap.TryGetValue(storedId, out var realPath))
                    {
                        field.FilePath = realPath;
                        field.FileName = Path.GetFileName(realPath);
                    }
                }
            }
        }

        _logger.LogInformation(
            "FINAL MANIFEST => {json}",
            JsonSerializer.Serialize(package)
        );

        // ===============================
        // 💾 Sauvegarde du manifest
        // ===============================
        await File.WriteAllTextAsync(
            Path.Combine(tempPath, "manifest.json"),
            JsonSerializer.Serialize(package, new JsonSerializerOptions
            {
                WriteIndented = true
            })
        );

        // ===============================
        // 📦 ZIP
        // ===============================
        var zipPath = Path.Combine(Path.GetTempPath(), $"{folderName}.zip");
        System.IO.Compression.ZipFile.CreateFromDirectory(tempPath, zipPath);

        // ===============================
        // 🔐 Chiffrement AES
        // ===============================
        var zipBytes = await File.ReadAllBytesAsync(zipPath);
        var aesKey = _cryptoService.GenerateAesKey();

        var (encryptedPayload, iv) =
            _cryptoService.EncryptPayload(zipBytes, aesKey);

        var encryptedPath = zipPath + ".enc";
        await File.WriteAllBytesAsync(encryptedPath, encryptedPayload);

        // ===============================
        // 🔐 Chiffrement clé AES (RSA)
        // ===============================
        var encryptedKey =
            _cryptoService.EncryptSessionKey(aesKey, targetKey.PublicKey);

        var payloadHash =
            _cryptoService.ComputeSHA256(encryptedPayload);

        // ===============================
        // 🚀 Préparation transfert
        // ===============================
        var fileSize = new FileInfo(encryptedPath).Length;

        var prepare = await _client.PrepareAsync(
            fileSize,
            sourceHospital.Id,
            targetHospital.Id
        );

        using var stream = File.OpenRead(encryptedPath);
        await _client.UploadAsync(stream, prepare.TransferId);

        // ===============================
        // 💾 Persistance
        // ===============================
        var entity = new TransferEntity
        {
            Id = Guid.Parse(prepare.TransferId),
            IdHospitalFrom = sourceHospital.Id,
            IdHospitalTo = targetHospital.Id,
            PatientReference = package.PatientReference,
            Message = request.Message,
            Status = "UPLOADED",
            PayloadHash = payloadHash,
            PayloadSize = encryptedPayload.Length,
            EncryptedSessionKey = encryptedKey,
            IV = iv,
            Created = DateTime.UtcNow
        };

        _context.Transfers.Add(entity);
        await _context.SaveChangesAsync();

        // 🔥 LIER LE TRANSFER A LA REQUEST SI FOURNIE
        if (request.TransferRequestId.HasValue)
        {
            var transferRequest = await _context.TransferRequests
                .FirstOrDefaultAsync(r => r.Id == request.TransferRequestId.Value);

            if (transferRequest != null)
            {
                // 🔥 LIAISON
                transferRequest.TransferId = entity.Id;

                // 🔥 APPROBATION AUTOMATIQUE
                transferRequest.Status = TransferRequestStatus.Approved;

                await _context.SaveChangesAsync();

                // 🔥 SYNCHRONISATION HUB
                await _client.SendTransferRequestResponseAsync(
                    new TransferRequestResponseNetworkDto
                    {
                        RequestId = transferRequest.Id,
                        Status = transferRequest.Status,
                        ResponseReason = "Transfert effectué",
                        TransferId = transferRequest.TransferId
                    });
            }
        }

        await _client.SendMetadataAsync(prepare.TransferId, new
        {
            PayloadHash = payloadHash,
            EncryptedKey = Convert.ToBase64String(encryptedKey),
            IV = Convert.ToBase64String(iv),
            targetKey.KeyVersion,
            Nonce = Guid.NewGuid().ToString()
        });

        // ===============================
        // 🧹 Nettoyage
        // ===============================
        try
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (File.Exists(encryptedPath)) File.Delete(encryptedPath);
        }
        catch { }

        return new ServiceResult<bool>(true);
    }
    public async Task<ServiceResult<IEnumerable<TransferListItemDto>>> GetTransferListAsync(ClaimsPrincipal claim)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;
        var user = await _usersRepository.GetUserByEmailAsync(email!);

        if (user == null)
            return new ServiceResult<IEnumerable<TransferListItemDto>>(HttpStatusCode.Unauthorized);

        var transfers = await _context.Transfers
            .Where(t => t.IdHospitalFrom == user.HospitalId
                     || t.IdHospitalTo == user.HospitalId)
            .OrderByDescending(t => t.Created)
            .ToListAsync();

        var hospitalIds = transfers
            .SelectMany(t => new[] { t.IdHospitalFrom, t.IdHospitalTo })
            .Distinct()
            .ToList();

        var hospitals = await _hospitalRepository.GetByIdsAsync(hospitalIds);

        var hospitalDict = hospitals.ToDictionary(h => h.Id, h => h.Name);

        int counter = transfers.Count;

        var result = transfers.Select((t, index) =>
            {
                var isReceived = t.IdHospitalTo == user.HospitalId;

                var type = isReceived ? "recu" : "envoye";

                var hospitalIdToDisplay = isReceived
                    ? t.IdHospitalFrom
                    : t.IdHospitalTo;

                var hospitalName = hospitalDict.ContainsKey(hospitalIdToDisplay)
                    ? hospitalDict[hospitalIdToDisplay]
                    : "Inconnu";

                return new TransferListItemDto
                {
                    Id = t.Id,
                    Code = $"TRF{(counter - index).ToString("D3")}",
                    HospitalName = hospitalName,
                    PatientReference = t.PatientReference ?? "N/A",
                    Message = t.Message ?? "",
                    Created = t.Created,
                    Status = t.Status.ToString(),
                    Type = type,
                    IsImported = t.IsImported,
                    RecordId = t.RecordId,
                    Record = t.Record
                };
            });
        return new ServiceResult<IEnumerable<TransferListItemDto>>(result);
    }


}

