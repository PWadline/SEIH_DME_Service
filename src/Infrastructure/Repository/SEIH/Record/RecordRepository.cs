using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Infrastructure.Repository.SEIH.Record;

public class RecordRepository : IRecordRepository
{
    private readonly AppDbContext _context;

    public RecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecordEntity>> GetAllAsync()
        => await _context.Records
            .Where(x => x.IsDeleted != true)
            .ToListAsync();

    public async Task<RecordEntity?> GetByIdAsync(Guid id)
        => await _context.Records
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true);

    public async Task<List<RecordEntity>> GetByHospitalBranchIdAsync(Guid hospitalBranchId)
    => await _context.Records
        .Where(x => x.HospitalBranchId == hospitalBranchId && x.IsDeleted != true)
        .ToListAsync();

    public async Task AddAsync(RecordEntity entity)
    {
        await _context.Records.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RecordEntity entity)
    {
        _context.Records.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Records.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<RecordEntity>> GetByHospitalIdAsync(Guid hospitalId)
    {
        return await (
         from r in _context.Records
         join b in _context.HospitalBranches
             on r.HospitalBranchId equals b.Id
         where b.HospitalId == hospitalId
               && r.IsDeleted != true
         select r
     ).ToListAsync();
    }
    public async Task<List<HospitalEntity>> GetAuthorizedHospitalsByRecordIdAsync(Guid recordId)
    {
        return await (
            from c in _context.Consents
            join ch in _context.ConsentHospitals on c.Id equals ch.ConsentId
            join h in _context.Hospitals on ch.HospitalId equals h.Id
            where c.RecordId == recordId
                  && c.IsGranted == true
                  && c.IsDeleted != true
                  && h.IsDeleted != true
            select h
        ).Distinct().ToListAsync();
    }

    public async Task<bool> IsHospitalAuthorizedForRecordAsync(Guid recordId, Guid hospitalId)
    {
        return await (
            from c in _context.Consents
            join ch in _context.ConsentHospitals on c.Id equals ch.ConsentId
            where c.RecordId == recordId
                  && c.IsGranted == true
                  && (c.IsDeleted != true || c.IsDeleted == null)
                  && ch.HospitalId == hospitalId
            select ch
        ).AnyAsync();
    }

}