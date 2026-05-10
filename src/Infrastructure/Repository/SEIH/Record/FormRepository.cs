// using Core.Application.Interface.Repository.SEIH;
// using Core.Domain.Entity;
// using Core.Domain.Entity.SEIH;
// using Infrastructure.Services.SEIH.User;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using System.Data;

// namespace Infrastructure.Repository.SEIH.Record;
// public class FormRepository : IFormRepository
// {
//     private readonly AppDbContext _context;

//     public FormRepository(AppDbContext context)
//     {
//         _context = context;
//     }

//     public async Task<List<FormEntity>> GetAllAsync()
//         => await _context.Forms
//             .Where(x => x.IsDeleted != true)
//             .ToListAsync();

//     public async Task<FormEntity?> GetByIdAsync(Guid id)
//         => await _context.Forms
//             .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true);

//     public async Task<List<FormEntity>> GetByHospitalIdAsync(Guid hospitalId)
//         => await _context.Forms
//             .Where(x => x.HospitalId == hospitalId && x.IsDeleted != true)
//             .ToListAsync();

//     public async Task AddAsync(FormEntity entity)
//     {
//         await _context.Forms.AddAsync(entity);
//         await _context.SaveChangesAsync();
//     }

//     public async Task UpdateAsync(FormEntity entity)
//     {
//         _context.Forms.Update(entity);
//         await _context.SaveChangesAsync();
//     }

//     public async Task DeleteAsync(Guid id)
//     {
//         var entity = await _context.Forms.FindAsync(id);
//         if (entity != null)
//         {
//             entity.IsDeleted = true;
//             await _context.SaveChangesAsync();
//         }
//     }
// }

using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.SEIH.Record;

public class FormRepository : IFormRepository
{
    private readonly AppDbContext _context;

    public FormRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FormEntity>> GetAllAsync()
        => await _context.Forms
            .Where(x => x.IsDeleted != true)
            .ToListAsync();

    public async Task<FormEntity?> GetByIdAsync(Guid id)
        => await _context.Forms
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true);

    public async Task<List<FormEntity>> GetByHospitalIdAsync(Guid hospitalId)
    {
        return await (
            from f in _context.Forms
            join b in _context.HospitalBranches
                on f.HospitalBranchId equals b.Id
            where b.HospitalId == hospitalId
                  && f.IsDeleted != true
            select f
        ).ToListAsync();
    }

    public async Task AddAsync(FormEntity entity)
    {
        await _context.Forms.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FormEntity entity)
    {
        _context.Forms.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Forms.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}