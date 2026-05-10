namespace Core.Application.Model.Features.Hospital;

public class TransfertHospitalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Department { get; set; } = default!;
    public bool IsActive { get; set; }
}
