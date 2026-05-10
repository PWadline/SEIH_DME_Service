using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IInstitutionKeyRepository
{
    Task AddAsync(InstitutionKeyEntity key);
    Task UpdateAsync(InstitutionKeyEntity key);
    Task<List<InstitutionKeyEntity>> GetByHospitalIdAsync(Guid hospitalId);
    Task<List<InstitutionKeyEntity>> GetAllByHospitalIdAsync(Guid hospitalId);
    Task UpdateRangeAsync(List<InstitutionKeyEntity> keys);
}
