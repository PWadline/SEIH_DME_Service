using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity.SEIH
{
    public class InstitutionKeyEntity : AuditableEntity
    {
        public Guid HospitalId { get; set; }
        public HospitalEntity Hospital { get; set; } = default!;
        public string PublicKey { get; set; } = default!;
        public DateTime PublicKeyCreateDate { get; set; }
        public DateTime? PublicKeyExpireDate { get; set; }
        public string PublicKeyFingerprint { get; set; } = default!;
        public int KeyVersion { get; set; }
        public bool? IsActive { get; set; }
    }
}

