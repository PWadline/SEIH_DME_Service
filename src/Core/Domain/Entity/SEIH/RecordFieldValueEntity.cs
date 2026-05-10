using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class RecordFieldValueEntity : AuditableEntity
{
    public Guid RecordId { get; set; }
    public RecordEntity Record { get; set; } = default!;
    public string? FieldLabel { get; set; }
    public string? FieldType { get; set; } 
    public string? Section { get; set; }
    public int Position { get; set; }
    public string? Value { get; set; }
    public Guid? FileId { get; set; } 
}
