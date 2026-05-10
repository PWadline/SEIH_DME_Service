namespace Core.Application.Model.Features.Record;

public class ImportTransferredRecordDto
{
    public string? TemplateName { get; set; }
    public string? PatientReference { get; set; }
    public List<TransferredSectionDto> Sections { get; set; } = new();
}

public class TransferredSectionDto
{
    public string Label { get; set; } = "";
    public List<TransferredFieldDto> Fields { get; set; } = new();
}

public class TransferredFieldDto
{
    public string Label { get; set; } = "";
    public string? Type { get; set; }
    public string? Value { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
}