using Core.Application.Model.Features.Record;

namespace Core.Application.Model.Features;

public class TransferListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string HospitalName { get; set; } = string.Empty;
    public string PatientReference { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsImported { get; set; } = false;
    public Guid? RecordId { get; set; }
    public RecordDto? Record { get; set; }
}
