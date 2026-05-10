using Core.Domain.Commons;
using Core.Domain.Entity.SEIH;

namespace Core.Domain.Entity;

public class FileStorageEntity : AuditableEntity
{
     public Guid HospitalId { get; set; }
    public string RelatedEntity { get; set; } = null!; 
    public Guid? RelatedEntityId { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}

