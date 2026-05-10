using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IFileStorageRepository
{
   Task<List<FileStorageEntity>> GetByHospitalIdAsync(Guid hospitalId);
    Task<FileStorageEntity?> GetByIdAsync(Guid id);
    Task AddAsync(FileStorageEntity entity);
    Task DeleteAsync(Guid id);

}
