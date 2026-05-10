using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IHospitalRepository
{
    Task<List<HospitalEntity>> GetAllAsync();
    Task<HospitalEntity?> GetHospitalByNameAsync(string hospitalName);
    Task AddAsync(HospitalEntity hospital);
    Task UpdateAsync(HospitalEntity hospital);
    Task DeleteAsync(Guid id);
    Task<HospitalEntity?> GetByIdAsync(Guid id);
    Task<HospitalEntity?> GetHospitalByIdAsync(Guid hospitalId);
    Task<List<HospitalEntity>> GetByIdsAsync(List<Guid> ids); // pour ton nouveau besoin
}
