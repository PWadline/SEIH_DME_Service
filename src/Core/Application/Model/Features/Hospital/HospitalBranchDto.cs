namespace Core.Application.Model.Features.Hospital;

public class HospitalBranchDto
{
    public Guid Id { get; set; }
    public Guid HospitalId { get; set; }
    public string DisplayName { get; set; } = default!;
    public string? Info { get; set; }
    public string? LogoPath { get; set; }
}
