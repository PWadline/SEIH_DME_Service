namespace Core.Application.Model.Features;

public class TransferDto
{
    public Guid Id { get; set; }
    public Guid IdHospitalFrom { get; set; }
    public Guid IdHospitalTo { get; set; }
    public string? EncryptedPayload { get; set; }
    public string? PayloadType { get; set; }
    public string? Status { get; set; }
    public DateTime Created { get; set; }
}
