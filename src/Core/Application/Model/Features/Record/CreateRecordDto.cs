using Microsoft.AspNetCore.Http;

namespace Core.Application.Model.Features.Record;

public class CreateRecordDto
{
    public Guid HospitalBranchId { get; set; }
    public string? InfoHopital { get; set; }
    public string? LogoHopital { get; set; }
    public string? TemplateName { get; set; }
    public string? PatientReference { get; set; }
    public int? LogoSize { get; set; }
    public List<CreateRecordFieldValueDto> FieldValues { get; set; } = new();
}

public class CreateRecordFieldValueDto
{
    public string? FieldLabel { get; set; }
    public string? Value { get; set; }
    public string? FieldType { get; set; }
    public string? Section { get; set; }
    public int Position { get; set; }
    public IFormFile? File { get; set; }
}
