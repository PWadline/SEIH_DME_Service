namespace Core.Application.Model.Features.Hospital;

public class UpdateRoleDto
{
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}
