using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IHospitalBranchRepository
{
    Task<HospitalBranchEntity?> GetByIdAsync(Guid id);
    Task<List<HospitalBranchEntity>> GetAllAsync();
    Task<List<HospitalBranchEntity>> GetByHospitalIdAsync(Guid hospitalId);
    Task AddAsync(HospitalBranchEntity branch);
    Task UpdateAsync(HospitalBranchEntity branch);
    Task DeleteAsync(Guid id);
}
