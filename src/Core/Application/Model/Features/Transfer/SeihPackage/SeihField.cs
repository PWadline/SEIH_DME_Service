namespace Core.Application.Model.Features;

public class SeihField
{
    public string Label { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string? Value { get; set; }
    public SeihFileMeta? File { get; set; }


//Changement x
    public string? FileName { get; set; }
public string? FilePath { get; set; }
}
