namespace Core.DTOs.External;

public class PublicHospitalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}