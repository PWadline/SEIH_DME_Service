namespace Core.Application.Model.Features.Hospital
{
    public class RegisterPublicKeyRequest
    {
        public Guid HospitalId { get; set; }
        public string PublicKey { get; set; } = "";
        public int KeyVersion { get; set; }
    }
}
