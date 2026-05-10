namespace Core.Application.Model.Features;

public class SeihTransferPackage
{
    public string TransferId { get; set; } = default!;
    public string Timestamp { get; set; } = default!;
    public string SchemaVersion { get; set; } = "SEIH-1.0";
    public HospitalInfo HospitalSource { get; set; } = default!;
    public HospitalInfo HospitalTarget { get; set; } = default!;
    public string? Message { get; set; }
    public string PatientReference { get; set; } = default!;
    public string ConsentHash { get; set; } = default!;
    public string PayloadHash { get; set; } = default!;
    public List<SeihSection> Sections { get; set; } = new();
    public List<SeihFileMeta> Files { get; set; } = new();
}
