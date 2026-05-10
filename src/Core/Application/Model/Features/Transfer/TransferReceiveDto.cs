namespace Core.Application.Model.Features;

public class TransferReceiveDto
{
    public Guid IdHospitalFrom { get; set; }
    public Guid IdHospitalTo { get; set; }
    public string EncryptedPayload { get; set; } = string.Empty;
    public string EncryptedKey { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public long PayloadSize { get; set; }
    public string PayloadType { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public int KeyVersion { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public string SignedData { get; set; } = string.Empty;
    public Guid IdConsent { get; set; }
    public string? Message { get; set; }
    public string ConsentHash { get; set; } = string.Empty;
    public string PatientReference { get; set; } = string.Empty;
}
