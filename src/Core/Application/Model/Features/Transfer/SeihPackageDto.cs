public class SeihPackageDto
{
    public string PatientReference { get; set; } = "";
    public List<SectionDto> Sections { get; set; } = new();
}

public class SectionDto
{
    public string Label { get; set; } = "";
    public List<FieldDto> Fields { get; set; } = new();
}

public class FieldDto
{
    public string Label { get; set; } = "";
    public string? Value { get; set; }
}