using Core.Application.Interface.Repository.SEIH.Hospital;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure.Services.SEIH.User;
using Core.Application.Model.Features.Hospital;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Infrastructure.Repository.SEIH.Hospital;

public class HospitalRoleRepository : IHospitalRoleRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public HospitalRoleRepository(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<RolePermissionEntity?> GetPermissionByNameAsync(string permissionName)
    {
        throw new NotImplementedException();
    }

    public async Task<RolesEntity?> GetRoleByNameAsync(string roleName, Guid hospitalId)
    {
        if (roleName == null || hospitalId.ToString() == null)
        {
            _logger.LogWarning("Attempted to get a null role.");
            return null;
        }
        var role = await _context.Rolesv2.Where(c => c.Name == roleName && c.HospitalId == hospitalId).FirstOrDefaultAsync();

        return role;
    }

   
    public async Task<List<RoleWithPermissionsDto>> GetRolesWithPermissionsAsync(Guid hospitalId)
    {
        return await _context.Rolesv2
            .Where(r => r.HospitalId == hospitalId && r.IsDeleted == false)
            .Select(r => new RoleWithPermissionsDto
            {
                Id = r.Id,
                Name = r.Name,
                Permissions = _context.RolesPermission
                    .Where(rp => rp.RoleId == r.Id && rp.IsActive == true && rp.IsDeleted == false)
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => new PermissionDto
                        {
                            Id = p.Id,
                            Name = p.Name ?? ""
                        })
                    .ToList()
            })
            .ToListAsync();
    }
    public async Task<bool> HospitalAddPermissionToRoleUserAsync(RolePermissionEntity rolePermission)
    {
        if (rolePermission == null)
        {
            _logger.LogWarning("Attempted to create a null user.");
            return true;
        }

        try
        {
            // Génère un nouvel ID si nécessaire
            if (rolePermission.Id == Guid.Empty)
                rolePermission.Id = Guid.NewGuid();

            rolePermission.Created = DateTime.UtcNow;
            rolePermission.IsDeleted = false;

            await _context.RolesPermission.AddAsync(rolePermission);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating user.");
            return false;
        }
    }

    public async Task<bool> HospitalCreateRoleAsync(RolesEntity role)
    {
        if (role == null)
        {
            _logger.LogWarning("Attempted to create a null user.");
            return true;
        }

        try
        {
            // Génère un nouvel ID si nécessaire
            if (role.Id == Guid.Empty)
                role.Id = Guid.NewGuid();

            role.Created = DateTime.UtcNow;
            role.IsDeleted = false;

            await _context.Rolesv2.AddAsync(role);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating user.");
            return false;
        }
    }

    public Task<IEnumerable<RolesEntity>> HospitalGetAllRolesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<RolePermissionEntity>> HospitalGetAllRoleWithPermissionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> HospitalUpdatePermissionToRoleUserAsync(RolePermissionEntity rolePermission)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HospitalUpdateRoleAsync(RolesEntity role)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PermissionEntity>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => p.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<bool> CreateRolePermissionsAsync(List<RolePermissionEntity> rolePermissions)
    {
        await _context.RolesPermission.AddRangeAsync(rolePermissions);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<RolesEntity?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.Rolesv2
            .FirstOrDefaultAsync(r => r.Id == roleId && r.IsDeleted == false);
    }

    public async Task<List<RolePermissionEntity>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _context.RolesPermission
            .Where(rp => rp.RoleId == roleId && rp.IsDeleted == false)
            .ToListAsync();
    }

    public async Task<bool> UpdateRoleWithPermissionsAsync(
        RolesEntity role,
        List<RolePermissionEntity> permissions)
    {
        _context.Rolesv2.Update(role);
        _context.RolesPermission.UpdateRange(permissions);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteRoleWithPermissionsAsync(
    RolesEntity role,
    List<RolePermissionEntity> permissions)
{
    _context.Rolesv2.Update(role);
    _context.RolesPermission.UpdateRange(permissions);

    return await _context.SaveChangesAsync() > 0;
}
}
