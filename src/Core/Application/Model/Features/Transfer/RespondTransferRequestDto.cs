using Core.Domain.Entity.SEIH;

namespace Core.Application.Model.Features;

public class RespondTransferRequestDto
{
    public Guid RequestId { get; set; }
    public TransferRequestStatus Status { get; set; }
    public Guid HospitalFromId { get; set; }
    public Guid HospitalToId { get; set; }
    public string InfoPatient { get; set; } = string.Empty;
    public Guid ConsentId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? TransferId { get; set; }
}
