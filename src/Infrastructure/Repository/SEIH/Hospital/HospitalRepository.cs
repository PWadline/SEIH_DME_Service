
using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repository.SEIH.Hospital;

public class HospitalRepository : IHospitalRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public HospitalRepository(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public Task AddAsync(HospitalEntity hospital)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<HospitalEntity>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<HospitalEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Hospitals
            .Where(h => h.Id == id &&
                       (h.IsDeleted == null || h.IsDeleted == false))
            .FirstOrDefaultAsync();
    }

    public async Task<HospitalEntity?> GetHospitalByIdAsync(Guid hospitalId)
    {
        return await _context.Hospitals
            .FirstOrDefaultAsync(h => h.Id == hospitalId);
    }

    public async Task<HospitalEntity?> GetHospitalByNameAsync(string Name)
    {
        if (string.IsNullOrWhiteSpace(Name))
            return null;

        return await _context.Hospitals
                             .Where(u => u.Name == Name && (u.IsDeleted == null || u.IsDeleted == false))
                             .FirstOrDefaultAsync();
    }



    public async Task UpdateAsync(HospitalEntity hospital)
    {
        var existing = await _context.Hospitals
            .FirstOrDefaultAsync(h => h.Id == hospital.Id);

        if (existing == null)
            throw new Exception("Hospital not found");


        existing.Name = hospital.Name;
        existing.Code = hospital.Code;
        existing.Address = hospital.Address;
        existing.City = hospital.City;
        existing.Department = hospital.Department;
        existing.Email = hospital.Email;
        existing.PhoneNumber = hospital.PhoneNumber;

        await _context.SaveChangesAsync();
    }

    public async Task<List<HospitalEntity>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Hospitals
            .Where(h => ids.Contains(h.Id) &&
                       (h.IsDeleted == null || h.IsDeleted == false))
            .ToListAsync();
    }

}
