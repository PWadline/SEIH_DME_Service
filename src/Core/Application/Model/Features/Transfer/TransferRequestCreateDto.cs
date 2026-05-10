namespace Core.Application.Model.Features;

public class CreateTransferRequestDto
{
    public Guid HospitalToId { get; set; }
    public string InfoPatient { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ParentRequestId { get; set; }
}
