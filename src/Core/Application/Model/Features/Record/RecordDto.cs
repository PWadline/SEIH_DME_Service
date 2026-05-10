namespace Core.Application.Model.Features.Record;

public class RecordDto
{
    public Guid Id { get; set; }
    public Guid HospitalId { get; set; }
    public string? TemplateName { get; set; }
    public string? PatientReference { get; set; }
    public bool IsTransferred { get; set; }
     public Guid? TransferId { get; set; }
     public bool IsImported { get; set; } = false;
    public int? LogoSize { get; set; }
    public string? LogoPath { get; set; }
    public DateTime Created { get; set; }
    public string? HospitalName { get; set; }
    public string? HospitalInfo { get; set; }
    public List<RecordFieldValueDto> FieldValues { get; set; } = new();
}


public class RecordFieldValueDto
{
    public Guid Id { get; set; }
    public string? FieldLabel { get; set; }
    public string? FieldType { get; set; }
    public string? Section { get; set; }
    public int Position { get; set; }
    public string? Value { get; set; }
    public string? FilePath { get; set; }
    public Guid? FileId { get; set; }
}
