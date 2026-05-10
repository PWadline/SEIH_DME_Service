using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class FormEntity : AuditableEntity
{
    public string? FormName { get; set; } 
    public string? TemplateName { get; set; } 
    // public Guid HospitalId { get; set; }  
    public Guid HospitalBranchId { get; set; }
    public int? LogoSize { get; set; } 
}
