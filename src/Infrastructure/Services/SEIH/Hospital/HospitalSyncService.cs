using Core.Application.Interface.Services.SEIH;
using Core.Domain.Entity.SEIH;
using Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.SEIH;

public class HospitalSyncService : IHospitalSyncService
{
    private readonly ISeihTransferClient _client;
    private readonly AppDbContext _context;
    private readonly ILogger<HospitalSyncService> _logger;

    public HospitalSyncService(
        ISeihTransferClient client,
        AppDbContext context,
        ILogger<HospitalSyncService> logger)
    {
        _client = client;
        _context = context;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting hospital sync...");

        var remoteHospitals = await _client.GetHospitalsAsync(ct);

        foreach (var remote in remoteHospitals)
        {
            var local = await _context.Hospitals
                .FirstOrDefaultAsync(x => x.Id == remote.Id, ct);

            if (local == null)
            {
                local = new HospitalEntity
                {
                    Id = remote.Id
                };

                _context.Hospitals.Add(local);
            }

            // 🔹 update hospital
            local.Name = remote.Name;
            local.Code = remote.Code;
            local.City = remote.City;
            local.Department = remote.Department;
            local.IsActive = remote.IsActive;
            local.LastSyncedAt = DateTime.UtcNow;

            // 🔐 🔥 AJOUT ICI
            if (remote.Keys != null)
            {
                foreach (var remoteKey in remote.Keys)
                {
                    var existingKey = await _context.TransferInstitutionKeys
                        .FirstOrDefaultAsync(k =>
                            k.HospitalId == remote.Id &&
                            k.KeyVersion == remoteKey.KeyVersion,
                            ct);

                    if (existingKey == null)
                    {
                        _context.TransferInstitutionKeys.Add(new TransferInstitutionKeyEntity
                        {
                            Id = Guid.NewGuid(),
                            HospitalId = remote.Id,
                            PublicKey = remoteKey.PublicKey,
                            KeyVersion = remoteKey.KeyVersion,
                            ExpirationDate = remoteKey.ExpirationDate,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = remoteKey.IsActive
                        });
                    }
                    else
                    {
                        existingKey.PublicKey = remoteKey.PublicKey;
                        existingKey.IsActive = remoteKey.IsActive;
                        existingKey.ExpirationDate = remoteKey.ExpirationDate;
                        existingKey.LastModified = DateTime.UtcNow;
                    }
                }
            }
        }
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Hospital sync completed.");
    }




}


