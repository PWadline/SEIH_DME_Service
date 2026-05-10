namespace Core.Application.Model.Features.Record;

public class FormDto
{
    public Guid Id { get; set; }
        public Guid HospitalBranchId { get; set; }
    public string? FormName { get; set; }
    public string? TemplateName { get; set; }
    public int? LogoSize { get; set; } 
    public string? HospitalName { get; set; }
    public string? HospitalInfo { get; set; }
    public string? LogoPath { get; set; }
    public List<FormFieldDto> FieldValues { get; set; } = new();

}

public class FormFieldDto
{
    public Guid Id { get; set; }
    public string? FieldLabel { get; set; }
    public string? FieldType { get; set; }
    public string? Section { get; set; }
    public int Position { get; set; }
    public bool IsRequired { get; set; }


}
