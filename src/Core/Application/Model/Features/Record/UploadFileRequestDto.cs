using Microsoft.AspNetCore.Http;

namespace Core.Application.Model.Features.Record;

public class UploadFileRequestDto
{
     public IFormFile File { get; set; } = null!;
    public string Category { get; set; } = null!;
}
