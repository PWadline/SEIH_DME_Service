using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Infrastructure.Repository.SEIH.Record;

public class SeihFileRepository : ISeihFileRepository
{
    private readonly AppDbContext _context;

    public SeihFileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(FileStorageEntity file)
    {
        await _context.Files.AddAsync(file);
    }

    public async Task<FileStorageEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Files
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<FileStorageEntity>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Files
            .Where(f => ids.Contains(f.Id) && f.IsDeleted != true)
            .ToListAsync();
    }


}