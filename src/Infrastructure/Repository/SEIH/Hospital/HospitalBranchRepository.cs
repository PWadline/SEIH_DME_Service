
using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository.SEIH.Hospital;

public class HospitalBranchRepository : IHospitalBranchRepository
{
    private readonly AppDbContext _context;

    public HospitalBranchRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HospitalBranchEntity?> GetByIdAsync(Guid id)
        => await _context.HospitalBranches
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true);

    public async Task<List<HospitalBranchEntity>> GetAllAsync()
        => await _context.HospitalBranches
            .Where(x => x.IsDeleted != true)
            .ToListAsync();

    public async Task<List<HospitalBranchEntity>> GetByHospitalIdAsync(Guid hospitalId)
        => await _context.HospitalBranches
            .Where(x => x.HospitalId == hospitalId && x.IsDeleted != true)
            .ToListAsync();

    public async Task AddAsync(HospitalBranchEntity branch)
    {
        await _context.HospitalBranches.AddAsync(branch);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(HospitalBranchEntity branch)
    {
        _context.HospitalBranches.Update(branch);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.HospitalBranches.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}