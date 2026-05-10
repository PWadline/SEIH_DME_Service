using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Infrastructure.Repository.SEIH.Record;

public class FormFieldRepository : IFormFieldRepository
{
    private readonly AppDbContext _context;

    public FormFieldRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FormFieldEntity>> GetByFormIdAsync(Guid formId)
        => await _context.FormFields
            .Where(x => x.FormId == formId && x.IsDeleted != true)
            .ToListAsync();

    public async Task CreateAsync(FormFieldEntity entity)
    {
        await _context.FormFields.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FormFieldEntity entity)
    {
        _context.FormFields.Update(entity);
        await _context.SaveChangesAsync();
    }
}