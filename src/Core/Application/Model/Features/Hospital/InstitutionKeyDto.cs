namespace Core.Application.Model.Features.Hospital;

public class InstitutionKeyDto
{
    public Guid Id { get; set; }
    public Guid HospitalId { get; set; }
    public int KeyVersion { get; set; }
    public string? PublicKey { get; set; }
    public DateTime? PublicKeyCreateDate { get; set; }
    public DateTime? PublicKeyExpireDate { get; set; }
    public string? PublicKeyFingerprint { get; set; }
}
