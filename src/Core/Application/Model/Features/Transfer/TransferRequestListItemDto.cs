namespace Core.Application.Model.Features;

public class TransferRequestListItemDto
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; 
    public string WorkflowType { get; set; } = string.Empty; 
    public string? NumeroTransfert { get; set; }
    public string? RequestReason { get; set; }
    public string? ResponseReason { get; set; }
    public DateTime Created { get; set; }
    public string PatientInfo { get; set; } = string.Empty;
    public bool Consentement { get; set; }
    public string Statut { get; set; } = string.Empty;

}

