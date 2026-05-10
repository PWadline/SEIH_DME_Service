using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class ConsentEntity : AuditableEntity
{
    public Guid RecordId { get; set; }
    public string? PatientReference { get; set; } 
    public bool IsGranted { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string ConsentHash { get; set; } = default!;
}
