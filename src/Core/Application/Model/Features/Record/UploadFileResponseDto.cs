namespace Core.Application.Model.Features.Record;

public class UploadFileResponseDto
{
    public Guid FileId { get; set; }
    public string OriginalFileName { get; set; } = null!;
}
