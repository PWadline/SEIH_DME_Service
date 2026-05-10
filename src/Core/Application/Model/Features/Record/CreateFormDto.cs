namespace Core.Application.Model.Features.Record;

public class CreateFormDto
{
    public Guid HospitalBranchId { get; set; }
    public string FormName { get; set; } = default!;
    public string TemplateName { get; set; } = default!;
    public string? NomHopital { get; set; }
    public string? InfoHopital { get; set; }
    public string? LogoHopital { get; set; }
    public int? LogoSize { get; set; }
    public List<CreateFormFieldDto> Fields { get; set; } = new();
}

public class CreateFormFieldDto
{
    public string FieldLabel { get; set; } = default!;
    public string FieldType { get; set; } = default!;
    public int Position { get; set; }
    public bool IsRequired { get; set; }
    public string Section { get; set; } = default!;
}
