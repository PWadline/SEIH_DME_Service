namespace Core.Application.Model.Features.Hospital;

public class AddRolesToUserDto
{
    public string UserEmail { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
