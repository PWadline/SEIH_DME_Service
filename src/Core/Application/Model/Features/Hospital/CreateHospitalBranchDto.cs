using Microsoft.AspNetCore.Http;

namespace Core.Application.Model.Features.Hospital;

public class CreateHospitalBranchDto
{
    public Guid HospitalId { get; set; }
    public string DisplayName { get; set; } = default!;
    public string? Info { get; set; }
    public IFormFile? LogoFile { get; set; }
}
