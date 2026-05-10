using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Infrastructure.Repository.SEIH.Record;

public class RecordFieldValueRepository : IRecordFieldValueRepository
{
    private readonly AppDbContext _context;

    public RecordFieldValueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(RecordFieldValueEntity value)
    {
        await _context.RecordFieldValues.AddAsync(value);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RecordFieldValueEntity value)
    {
        _context.RecordFieldValues.Update(value);
        await _context.SaveChangesAsync();
    }

    public async Task<List<RecordFieldValueEntity>> GetByRecordIdAsync(Guid recordId)
    {
        return await _context.RecordFieldValues
            .Where(x => x.RecordId == recordId && x.IsDeleted != true)
            .ToListAsync();
    }
}