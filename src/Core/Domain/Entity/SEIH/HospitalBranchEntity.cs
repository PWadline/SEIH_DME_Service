using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity.SEIH
{
    public class HospitalBranchEntity : AuditableEntity
    {
        public Guid HospitalId { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? Info { get; set; }
        public Guid? LogoFileId { get; set; }
    }
}
