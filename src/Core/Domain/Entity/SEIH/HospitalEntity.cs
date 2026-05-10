using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity.SEIH
{
    public class HospitalEntity : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string City { get; set; } = default!;
        public string Department { get; set; } = default!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSyncedAt { get; set; }
    }
}
