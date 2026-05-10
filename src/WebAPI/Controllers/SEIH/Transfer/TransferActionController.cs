using Application.Abstractions;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Record;
using Core.Application.Model.Features.Record;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/transfers")]
[AllowAnonymous]
public class TransferActionController : ControllerBase
{
    private static readonly Guid TransferredBranchId =
    Guid.Parse("13da326b-0f78-40bc-bab9-bd8e6cdfb293");
    private readonly AppDbContext _context;
    private readonly ISeihCryptoService _cryptoService;
    private readonly ILogger<TransferActionController> _logger;
    private readonly IConfiguration _config;
    private readonly ISeihTransferClient _client;
    private readonly IRecordService _recordService;

    public TransferActionController(
    AppDbContext context,
    ISeihCryptoService cryptoService,
    ILogger<TransferActionController> logger,
    IConfiguration config,
    ISeihTransferClient client,
    IRecordService recordService)
    {
        _context = context;
        _cryptoService = cryptoService;
        _logger = logger;
        _config = config;
        _client = client;
        _recordService = recordService;
    }

    [HttpPost("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var transfer = await _context.Transfers.FindAsync(id);

        if (transfer == null)
            return NotFound();

        try
        {
            // 🔥 IMPORTANT → récupérer depuis HUB
            var stream = await _client.DownloadAsync(id.ToString());

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            var encryptedBytes = ms.ToArray();

            if (transfer.EncryptedSessionKey == null || transfer.IV == null)
            {
                _logger.LogError("Missing encryption data for transfer {Id}", id);
                return StatusCode(500, "Invalid transfer data");
            }

            var privateKeyPath = _config["SEIH:TransferKey:PrivateKeyPath"];
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), privateKeyPath!);
            var privateKeyPem = await System.IO.File.ReadAllTextAsync(fullPath);

            _logger.LogWarning("🔐 PRIVATE KEY USED FOR DECRYPT: {key}", privateKeyPem);
            var aesKey = _cryptoService.DecryptSessionKey(
                transfer.EncryptedSessionKey,
                privateKeyPem
            );

            var decrypted = _cryptoService.DecryptPayload(
                encryptedBytes,
                aesKey,
                transfer.IV
            );

            return base.File(decrypted, "application/zip", $"transfer_{id}.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DOWNLOAD FAILED → {Id}", id);
            return StatusCode(500, "Decryption failed");
        }

    }

    [HttpPost("{id}/import")]

    public async Task<IActionResult> Import(Guid id)
    {
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == id);

        _logger.LogInformation("Transfer loaded: {id}", transfer?.Id);

        if (transfer == null)
            return NotFound("Transfer not found");

        // 🔥 Vérifier directement sur le transfert
        if (transfer.IsImported)
        {
            return BadRequest("Ce transfert a déjà été importé.");
        }

        try
        {
            _logger.LogInformation("Calling HUB download for transfer {id}", id);

            var encryptedStream = await _client.DownloadAsync(id.ToString());

            _logger.LogInformation("HUB download finished");

            using var ms = new MemoryStream();
            await encryptedStream.CopyToAsync(ms);

            var encryptedBytes = ms.ToArray();

            var privateKeyPath = _config["SEIH:TransferKey:PrivateKeyPath"];

            if (string.IsNullOrWhiteSpace(privateKeyPath))
                return StatusCode(500, "Missing private key path configuration");

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                privateKeyPath
            );

            if (!System.IO.File.Exists(fullPath))
                return StatusCode(500, "Private key file not found");

            var privateKeyPem =
                await System.IO.File.ReadAllTextAsync(fullPath);

            if (
                transfer.EncryptedSessionKey == null ||
                transfer.IV == null
            )
            {
                _logger.LogError(
                    "Transfer {Id} is missing encryption data",
                    id
                );

                return StatusCode(
                    500,
                    "Transfer encryption data missing"
                );
            }

            var aesKey = _cryptoService.DecryptSessionKey(
                transfer.EncryptedSessionKey,
                privateKeyPem
            );

            var decryptedZipBytes =
                _cryptoService.DecryptPayload(
                    encryptedBytes,
                    aesKey,
                    transfer.IV
                );

            using var zipStream = new MemoryStream(decryptedZipBytes);

            using var archive = new ZipArchive(
                zipStream,
                ZipArchiveMode.Read
            );

            var jsonEntry = archive.GetEntry("manifest.json");

            if (jsonEntry == null)
                return BadRequest(
                    "manifest.json not found in transfer"
                );

            string json;

            using (var reader = new StreamReader(jsonEntry.Open()))
            {
                json = await reader.ReadToEndAsync();
            }

            var dto =
                JsonSerializer.Deserialize<ImportTransferredRecordDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (dto == null)
                return BadRequest("Invalid manifest format");

            // 🔥 ON GARDE transferId dans RecordEntity
            var result =
                await _recordService.ImportFromTransferAsync(
                    User,
                    dto,
                    TransferredBranchId,
                    archive,
                    id
                );

            if (result.IsError)
                return BadRequest(result);

            // 🔥 MARQUER LE TRANSFERT COMME IMPORTÉ
            transfer.IsImported = true;

            // 🔥 LIER LE DOSSIER IMPORTÉ
            transfer.RecordId = result.Result!.Id;

            _context.Transfers.Update(transfer);

            await _context.SaveChangesAsync();

            await _client.AckAsync(
                id,
                transfer.IdHospitalTo
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "IMPORT FAILED for transfer {Id}",
                id
            );

            return StatusCode(500, "Import failed");
        }
    }



}