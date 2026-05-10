using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IConsentRepository
{
    Task<List<HospitalEntity>> GetAuthorizedHospitalsByRecordIdAsync(Guid recordId);
}
