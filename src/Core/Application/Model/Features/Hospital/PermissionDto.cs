namespace Core.Application.Model.Features.Hospital;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RoleWithPermissionsDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

