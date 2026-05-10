namespace Core.Application.Model.Features.Record;

public class UploadFileFromStreamDto
{
    public Stream Stream { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string Category { get; set; } = default!;
}