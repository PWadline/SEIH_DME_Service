using Core.Application.Model.Features.Record;
using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity.SEIH;

public class TransferEntity : AuditableEntity
{
    public Guid IdHospitalFrom { get; set; }
    public Guid IdHospitalTo { get; set; }
    public byte[]? EncryptedPayload { get; set; }
    public byte[]? EncryptedSessionKey { get; set; }
    public byte[]? IV { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public long PayloadSize { get; set; }
    public string PayloadType { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public Guid? IdConsent { get; set; }
    public string ConsentHash { get; set; } = string.Empty;
    public DateTime? ConsentExpiration { get; set; }
    public string PatientReference { get; set; } = string.Empty;
    public string Status { get; set; } = "CREATED";
    public int KeyVersion { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public string? Message { get; set; }
    public bool IsImported { get; set; } = false;
    public Guid? RecordId { get; set; }
    public RecordDto? Record { get; set; }
}
