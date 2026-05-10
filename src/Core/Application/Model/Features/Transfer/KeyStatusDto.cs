namespace Core.Application.Model.Features;

public class KeyStatusDto
{
    public bool HasKey { get; set; }
    public string? Fingerprint { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? KeyVersion { get; set; }
}
