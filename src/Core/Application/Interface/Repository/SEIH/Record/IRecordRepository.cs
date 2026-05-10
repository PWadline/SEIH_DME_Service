using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IRecordRepository
{
    Task<List<RecordEntity>> GetAllAsync();
    Task<RecordEntity?> GetByIdAsync(Guid id);
    Task<List<RecordEntity>> GetByHospitalBranchIdAsync(Guid hospitalBranchId);
    Task AddAsync(RecordEntity entity);
    Task UpdateAsync(RecordEntity entity);
    Task DeleteAsync(Guid id);
    Task<List<RecordEntity>> GetByHospitalIdAsync(Guid hospitalId);
    Task<List<HospitalEntity>> GetAuthorizedHospitalsByRecordIdAsync(Guid recordId);
    Task<bool> IsHospitalAuthorizedForRecordAsync(Guid recordId, Guid hospitalId);
}




