using Core.Application.Model.Features.Hospital;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH.Hospital;

public interface IHospitalRoleRepository
{
    Task<bool> HospitalCreateRoleAsync(RolesEntity role);
    Task<bool> HospitalAddPermissionToRoleUserAsync(RolePermissionEntity rolePermission);
    Task<bool> HospitalUpdatePermissionToRoleUserAsync(RolePermissionEntity rolePermission);
    Task<bool> HospitalUpdateRoleAsync(RolesEntity role);
    Task<IEnumerable<RolesEntity>> HospitalGetAllRolesAsync();
    Task<IEnumerable<RolePermissionEntity>> HospitalGetAllRoleWithPermissionsAsync();
    Task<RolesEntity?> GetRoleByNameAsync(string roleName, Guid hospitalId);
    Task<RolePermissionEntity?> GetPermissionByNameAsync(string permissionName);
    Task<List<RoleWithPermissionsDto>> GetRolesWithPermissionsAsync(Guid hospitalId);
    Task<List<PermissionEntity>> GetAllPermissionsAsync();
    Task<bool> CreateRolePermissionsAsync(List<RolePermissionEntity> rolePermissions);
    Task<RolesEntity?> GetRoleByIdAsync(Guid roleId);
    Task<List<RolePermissionEntity>> GetRolePermissionsAsync(Guid roleId);
    Task<bool> UpdateRoleWithPermissionsAsync(RolesEntity role, List<RolePermissionEntity> permissions);
    Task<bool> DeleteRoleWithPermissionsAsync(RolesEntity role, List<RolePermissionEntity> permissions);
}


