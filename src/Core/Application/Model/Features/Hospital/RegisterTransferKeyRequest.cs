namespace Core.Application.Model.Features.Hospital
{
    public class RegisterTransferKeyRequest
{
    public Guid HospitalId { get; set; }
    public string PublicKey { get; set; } = default!;
    public int KeyVersion { get; set; }
}
}
