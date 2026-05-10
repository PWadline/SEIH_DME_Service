using Microsoft.AspNetCore.Http;

namespace Core.Application.Model.Features;

public class CreateTransferDto
{
    public Guid HospitalToId { get; set; }
    public Guid RecordId { get; set; }
    public Guid? TransferRequestId { get; set; }
    public string? Message { get; set; }
    public string? Pdf { get; set; }
    public List<FileTransferDto>? Files { get; set; }
}

