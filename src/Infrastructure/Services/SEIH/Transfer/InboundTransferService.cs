using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Application.Abstractions;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Transfer;
using Core.Application.Model.Features;
using Core.Domain.Entity.SEIH;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.SEIH.Hospital;

public class InboundTransferService : IInboundTransferService
{
    private readonly ISeihTransferClient _client;
    private readonly ISeihCryptoService _cryptoService;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly ILogger<InboundTransferService> _logger;
    private readonly ISignatureEnvelopeBuilder _envelopeBuilder;


    public InboundTransferService(
        ISeihTransferClient client,
        ISeihCryptoService cryptoService,
        IConfiguration config,
        AppDbContext context,
        ILogger<InboundTransferService> logger,
        ISignatureEnvelopeBuilder envelopeBuilder)
    {
        _client = client;
        _cryptoService = cryptoService;
        _config = config;
        _context = context;
        _logger = logger;
        _envelopeBuilder = envelopeBuilder;
    }

    public async Task PullIncomingTransfersAsync(Guid hospitalId)
    {
        _logger.LogInformation("📥 PullIncomingTransfersAsync CALLED for {HospitalId}", hospitalId);

        var transfers = await _client.GetIncomingAsync(hospitalId);

        foreach (var t in transfers)
        {
            _logger.LogInformation("➡️ PROCESSING TRANSFER {Id}", t.Id);

            if (t.IdHospitalTo != hospitalId)
                continue;

            string tempFolder = "";
            try
            {
                // ===============================
                // 📥 DOWNLOAD
                // ===============================
                var stream = await _client.DownloadAsync(t.Id.ToString());
                _logger.LogInformation("******************************BREACKPOINT STREAM HIT");

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                var encryptedBytes = ms.ToArray();

                _logger.LogInformation("FILE SIZE RECEIVED: {size}", encryptedBytes.Length);

                // ===============================
                // 🔐 HASH CHECK
                // ===============================
                var computedHash = _cryptoService.ComputeSHA256(encryptedBytes);
                _logger.LogInformation("******************************BREACKPOINT computedHash HIT");

                if (computedHash != t.PayloadHash)
                    throw new Exception("Payload integrity check failed");

                _logger.LogInformation("******************************BREACKPOINT if (computedHash != t.PayloadHash) HIT");

                // ===============================
                // 🔐 SIGNATURE
                // ===============================
                var sourceKey = await _context.TransferInstitutionKeys
                    .Where(k => k.HospitalId == t.IdHospitalFrom && k.KeyVersion == t.KeyVersion)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("******************************BREACKPOINT sourceKey HIT");

                if (sourceKey == null)
                    throw new Exception("Source public key not found");





                // ===============================
                // 🔐 DECRYPT
                // ===============================
                var privateKeyPath = _config["SEIH:TransferKey:PrivateKeyPath"];
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), privateKeyPath!);
                var privateKeyPem = await File.ReadAllTextAsync(fullPath);

                var encryptedKey = Convert.FromBase64String(t.EncryptedKey);
                var iv = Convert.FromBase64String(t.IV);

                var aesKey = _cryptoService.DecryptSessionKey(encryptedKey, privateKeyPem);

                _logger.LogInformation("******************************BREACKPOINT aesKey HIT");
                var decrypted = _cryptoService.DecryptPayload(encryptedBytes, aesKey, iv);

                _logger.LogInformation("******************************BREACKPOINT decrypted HIT");

                // ===============================
                // 📦 ZIP PROCESS
                // ===============================
                tempFolder = Path.Combine(Path.GetTempPath(), $"IN_{t.Id}");

                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);

                Directory.CreateDirectory(tempFolder);

                var zipPath = Path.Combine(tempFolder, "package.zip");

                _logger.LogInformation("******************************BREACKPOINT zipPath HIT");

                await File.WriteAllBytesAsync(zipPath, decrypted);

                ZipFile.ExtractToDirectory(zipPath, tempFolder);

                // ===============================
                // 📄 MANIFEST
                // ===============================
                var manifestPath = Path.Combine(tempFolder, "manifest.json");

                _logger.LogInformation("******************************BREACKPOINT manifestPath HIT");

                if (!File.Exists(manifestPath))
                    throw new Exception("manifest.json not found");

                var json = await File.ReadAllTextAsync(manifestPath);

                _logger.LogInformation("✅ JSON OK");

                // ===============================
                // 🧠 BUSINESS
                // ===============================
                await ProcessSeihPackage(json, t, encryptedBytes.Length);

                await _client.AckAsync(t.Id, hospitalId);

                _logger.LogInformation("✅ TRANSFER SUCCESS {Id}", t.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ TRANSFER FAILED {Id} → {msg}", t.Id, ex.Message);
                await SafeFailAsync(t.Id, hospitalId);
            }
            finally
            {
                // ===============================
                // 🧹 CLEANUP
                // ===============================
                try
                {
                    if (!string.IsNullOrEmpty(tempFolder) && Directory.Exists(tempFolder))
                        Directory.Delete(tempFolder, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Cleanup failed: {msg}", ex.Message);
                }
            }
        }
    }

    private async Task ProcessSeihPackage(string json, IncomingTransferDto t, int payloadSize)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var package = JsonSerializer.Deserialize<SeihTransferPackage>(json, options);

        if (package == null)
            throw new Exception("Invalid SEIH package");

        _logger.LogInformation("Received package for patient {Patient}", package.PatientReference);

        var transferHistory = new TransferEntity
        {
            Id = t.Id,
            IdHospitalFrom = t.IdHospitalFrom,
            IdHospitalTo = t.IdHospitalTo,

            PayloadType = "SEIH_PACKAGE",
            SchemaVersion = package.SchemaVersion,
            PatientReference = package.PatientReference,

            Status = "RECEIVED",
            Message = package.Message,

            Nonce = t.Nonce,
            KeyVersion = t.KeyVersion,

            // 🔥 AJOUT OBLIGATOIRE
            EncryptedSessionKey = Convert.FromBase64String(t.EncryptedKey),
            IV = Convert.FromBase64String(t.IV),
            PayloadHash = t.PayloadHash,
            PayloadSize = payloadSize
        };

        _context.Transfers.Add(transferHistory);

        await _context.SaveChangesAsync();
    }

    private async Task SafeFailAsync(Guid transferId, Guid hospitalId)
    {
        try
        {
            await _client.FailAsync(transferId, hospitalId);
            _logger.LogInformation("Transfer {Id} marked as FAILED on hub.", transferId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify hub about FAILED transfer {Id}", transferId);
        }
    }
}