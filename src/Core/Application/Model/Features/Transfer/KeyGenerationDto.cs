namespace Core.Application.Model.Features;

public class KeyGenerationDto
{
    public string PublicKey { get; set; } = default!;
    public string PrivateKey { get; set; } = default!;
    public string Fingerprint { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
