
using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository.SEIH.Hospital;

public class InstitutionKeyRepository : IInstitutionKeyRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<InstitutionKeyRepository> _logger;

    public InstitutionKeyRepository(
        AppDbContext context,
        ILogger<InstitutionKeyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(InstitutionKeyEntity key)
    {
        await _context.InstitutionKeys.AddAsync(key);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(InstitutionKeyEntity key)
    {
        var existing = await _context.InstitutionKeys
            .FirstOrDefaultAsync(k => k.Id == key.Id);

        if (existing == null)
            throw new Exception("Institution key not found");

        existing.PublicKey = key.PublicKey;
        existing.PublicKeyCreateDate = key.PublicKeyCreateDate;
        existing.PublicKeyExpireDate = key.PublicKeyExpireDate;
        existing.PublicKeyFingerprint = key.PublicKeyFingerprint;
        existing.KeyVersion = key.KeyVersion;

        await _context.SaveChangesAsync();
    }

    public async Task<List<InstitutionKeyEntity>> GetByHospitalIdAsync(Guid hospitalId)
{
    return await _context.InstitutionKeys
        .Where(x => x.HospitalId == hospitalId)
        .ToListAsync();
}

    public async Task<List<InstitutionKeyEntity>> GetAllByHospitalIdAsync(Guid hospitalId)
    {
        return await _context.InstitutionKeys
            .Where(k => k.HospitalId == hospitalId &&
                        (k.IsDeleted == null || k.IsDeleted == false))
            .OrderByDescending(k => k.KeyVersion)
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(List<InstitutionKeyEntity> keys)
{
    _context.InstitutionKeys.UpdateRange(keys);
    await _context.SaveChangesAsync();
}
}