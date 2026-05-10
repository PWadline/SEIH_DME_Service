using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class RecordEntity : AuditableEntity
{
    public Guid HospitalBranchId { get; set; }
    public Guid? TransferId { get; set; }
    public string? TemplateName { get; set; }
    public string? PatientReference { get; set; }
    public int? LogoSize { get; set; }
    public bool IsTransferred { get; set; } = false;
    public bool IsNewCodeRequired { get; set; } = true;
    public string? PatientAccessCodeHash { get; set; }
    public DateTime? PatientAccessCodeUpdatedAt { get; set; }
    public ICollection<RecordFieldValueEntity> FieldValues { get; set; } = new List<RecordFieldValueEntity>();
}

