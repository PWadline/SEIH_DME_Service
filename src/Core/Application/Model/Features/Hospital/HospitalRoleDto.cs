namespace Core.Application.Model.Features.Hospital;

public class HospitalRoleDto
{
    public string? RoleName { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}

// public class HospitalRolePermissionDto
// {
//     public string? RoleName { get; set; }
//     public List<Guid> PermissionIds { get; set; } = new();
//     // public List<string>? PermissionName { get; set; }

// }