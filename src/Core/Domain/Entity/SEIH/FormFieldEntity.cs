using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class FormFieldEntity : AuditableEntity
{
    public Guid FormId { get; set; }
    public string? FieldLabel { get; set; }
    public string? FieldType { get; set; } 
    public string? Section { get; set; }
    public int Position { get; set; } 
    public bool IsRequired { get; set; } 
}
