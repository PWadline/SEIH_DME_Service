using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface ISeihFileRepository
{
   Task AddAsync(FileStorageEntity file);
    Task<FileStorageEntity?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
    Task<List<FileStorageEntity>> GetByIdsAsync(List<Guid> ids);
}




