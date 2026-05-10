namespace Core.Application.Model.Features.Hospital;

public class HospitalWithKeysDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Code { get; set; } = "";

    public string City { get; set; } = "";

    public string Department { get; set; } = "";

    public bool IsActive { get; set; }

    public List<TransferInstitutionKeyDto> Keys { get; set; } = new();
}

