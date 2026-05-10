using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class ConsentHospitalEntity : AuditableEntity
{
    public Guid ConsentId { get; set; }
    public Guid HospitalId { get; set; }
    public ConsentEntity? Consent { get; set; }
    public HospitalEntity? Hospital { get; set; }
}