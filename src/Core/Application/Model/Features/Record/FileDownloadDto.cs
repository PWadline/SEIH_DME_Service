namespace Core.Application.Model.Features.Record;

public class FileDownloadDto
{
    public byte[] FileBytes { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
