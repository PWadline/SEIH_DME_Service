namespace Core.Application.Model.Features;

public class SeihFileMeta
{
   public string StoredName { get; set; } = default!;
    public string OriginalName { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public long Size { get; set; }
    public string Hash { get; set; } = default!;
}

