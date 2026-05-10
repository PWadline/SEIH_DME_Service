using Microsoft.AspNetCore.Http;

namespace Core.Application.Model.Features.Record;

public class UpdateRecordDto
{
    public Guid Id { get; set; }
    public string? TemplateName { get; set; }
    public string? PatientReference { get; set; }
    public int? LogoSize { get; set; }
    public List<UpdateRecordFieldValueDto> FieldValues { get; set; } = new();
}

public class UpdateRecordFieldValueDto
{
    public string? FieldLabel { get; set; }
    public string? Value { get; set; }
    public string? FieldType { get; set; }
    public string? Section { get; set; }
    public int Position { get; set; }
    public IFormFile? File { get; set; }
    public Guid? FileId { get; set; }   
}