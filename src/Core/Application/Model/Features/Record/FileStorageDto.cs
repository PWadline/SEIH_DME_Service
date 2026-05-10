namespace Core.Application.Model.Features.Record;

public class FileStorageDto
{
    public Guid Id { get; set; }
    public Guid HospitalId { get; set; }

    public string RelatedEntity { get; set; } = null!;
    public Guid? RelatedEntityId { get; set; }

    public string OriginalFileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
}