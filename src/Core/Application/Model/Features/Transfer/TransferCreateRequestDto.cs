namespace Core.Application.Model.Features;

public class TransferCreateRequestDto
{
    public Guid HospitalToId { get; set; }
    public Guid ConsentId { get; set; }
    public string PatientReference { get; set; } = string.Empty;
    public Guid? TransferRequestId { get; set; } 
}