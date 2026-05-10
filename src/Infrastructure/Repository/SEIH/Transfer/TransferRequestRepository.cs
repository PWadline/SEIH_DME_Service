using Core.Application.Interface.Repository.SEIH;
using Core.Domain.Entity.SEIH;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.SEIH.Transfer;

public class TransferRequestRepository : ITransferRequestRepository
{
    private readonly AppDbContext _context;

    public TransferRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateAsync(TransferRequestEntity entity)
    {
        _context.TransferRequests.Add(entity);
        return await _context.SaveChangesAsync() > 0;
    }

   public async Task<TransferRequestEntity?> GetByIdAsync(Guid id)
{
    return await _context.TransferRequests
        .FirstOrDefaultAsync(x => x.Id == id);
}

    public async Task<IEnumerable<TransferRequestEntity>> GetHospitalRequestsAsync(Guid hospitalId)
    {
        return await _context.TransferRequests
            .Where(x => x.IdHospitalFrom == hospitalId
                     || x.IdHospitalTo == hospitalId)
            .OrderByDescending(x => x.Created)
            .ToListAsync();
    }

    public async Task<bool> UpdateAsync(TransferRequestEntity entity)
    {
        _context.TransferRequests.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}



