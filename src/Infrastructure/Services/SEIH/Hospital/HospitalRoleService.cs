using AutoMapper;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Repository.SEIH.Hospital;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Model.Features.Hospital;
using Core.Domain;
using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;
using Infrastructure.Repository.SEIH.Hospital;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace Infrastructure.Services.SEIH.Hospital;

public class HospitalRoleService : IHospitalRoleService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IHospitalRoleRepository _hospitalRoleRepository;
    public HospitalRoleService(IUsersRepository usersRepository, IMapper mapper, IHospitalRoleRepository hospitalRoleRepository)
    {
        _usersRepository = usersRepository;
        _hospitalRoleRepository = hospitalRoleRepository;
    }

    public async Task<ServiceResult<IEnumerable<RoleWithPermissionsDto>>> GetAllRoleServiceAsync(ClaimsPrincipal claim)
    {
        var email = claim.Claims
            .Where(c => c.Type == ClaimTypes.Email)
            .Select(c => c.Value)
            .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);

        if (manager == null)
            return new ServiceResult<IEnumerable<RoleWithPermissionsDto>>(HttpStatusCode.Unauthorized);

        var roles = await _hospitalRoleRepository.GetRolesWithPermissionsAsync(manager.HospitalId);

        return new ServiceResult<IEnumerable<RoleWithPermissionsDto>>(roles);
    }



    public async Task<ServiceResult<bool>> HospitalCreateRoleServiceAsync(ClaimsPrincipal claim, HospitalRoleDto dataModel)
    {
        var email = claim.Claims 
            .Where(c => c.Type == ClaimTypes.Email)
            .Select(c => c.Value)
            .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);
        if (manager == null)
        {
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);
        }

        var role = new RolesEntity
        {
            Id = Guid.NewGuid(),
            Name = dataModel.RoleName,
            HospitalId = manager.HospitalId,
            IsBasicRole = false,
            CreatedBy = manager.Id.ToString(),
            Created = DateTime.UtcNow,
            IsDeleted = false,
        };

        var roleCreation = await _hospitalRoleRepository.HospitalCreateRoleAsync(role);
        if (!roleCreation)
        {
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
        }

        var allPermissions = await _hospitalRoleRepository.GetAllPermissionsAsync();

        Console.WriteLine("===== DEBUG PERMISSIONS =====");

        Console.WriteLine("Permissions envoyées:");
        foreach (var id in dataModel.PermissionIds)
        {
            Console.WriteLine(id);
        }

        Console.WriteLine("Permissions en base:");
        foreach (var p in allPermissions)
        {
            Console.WriteLine(p.Id);
        }

        Console.WriteLine("===== MATCHING =====");


        var rolePermissions = new List<RolePermissionEntity>();

        foreach (var permission in allPermissions)
        {
            // var isAssigned = dataModel.PermissionIds.Contains(permission.Id);
            var isAssigned = permission.Id == SystemPermissions.UpdatePassword || dataModel.PermissionIds.Contains(permission.Id);


            Console.WriteLine($"Permission {permission.Id} -> Assigned: {isAssigned}");

            rolePermissions.Add(new RolePermissionEntity
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                PermissionId = permission.Id,
                IsActive = isAssigned,
                Created = DateTime.UtcNow,
                CreatedBy = manager.Id.ToString(),
                IsDeleted = false
            });
        }

        var result = await _hospitalRoleRepository.CreateRolePermissionsAsync(rolePermissions);

        if (!result)
        {
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);
        }

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<bool>> UpdateRoleServiceAsync(ClaimsPrincipal claim, UpdateRoleDto dataModel)
    {
        var email = claim.Claims
            .Where(c => c.Type == ClaimTypes.Email)
            .Select(c => c.Value)
            .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);
        if (manager == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var role = await _hospitalRoleRepository.GetRoleByIdAsync(dataModel.RoleId);

        if (role == null)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        role.Name = dataModel.RoleName;
        role.LastModified = DateTime.UtcNow;
        role.LastModifiedBy = manager.Id.ToString();

        var existingPermissions = await _hospitalRoleRepository
            .GetRolePermissionsAsync(role.Id);

        foreach (var rp in existingPermissions)
        {
            // var isAssigned = dataModel.PermissionIds.Contains(rp.PermissionId);
            var isAssigned =rp.PermissionId == SystemPermissions.UpdatePassword || dataModel.PermissionIds.Contains(rp.PermissionId);

            rp.IsActive = isAssigned;
            rp.LastModified = DateTime.UtcNow;
            rp.LastModifiedBy = manager.Id.ToString();
        }

        var result = await _hospitalRoleRepository.UpdateRoleWithPermissionsAsync(role, existingPermissions);

        if (!result)
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);

        return new ServiceResult<bool>(true);
    }

    public async Task<ServiceResult<bool>> DeleteRoleServiceAsync(
        ClaimsPrincipal claim,
        DeleteRoleDto dataModel)
    {
        var email = claim.Claims
            .Where(c => c.Type == ClaimTypes.Email)
            .Select(c => c.Value)
            .FirstOrDefault();

        var manager = await _usersRepository.GetUserByEmailAsync(email!);

        if (manager == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        // 1️⃣ récupérer le rôle
        var role = await _hospitalRoleRepository.GetRoleByIdAsync(dataModel.RoleId);

        if (role == null)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        // 2️⃣ soft delete du rôle
        role.IsDeleted = true;
        role.LastDeleted = DateTime.UtcNow;
        role.LastDeletedBy = manager.Id.ToString();

        // 3️⃣ récupérer les permissions liées
        var rolePermissions = await _hospitalRoleRepository
            .GetRolePermissionsAsync(role.Id);

        foreach (var rp in rolePermissions)
        {
            rp.IsDeleted = true;
            rp.LastDeleted = DateTime.UtcNow;
            rp.LastDeletedBy = manager.Id.ToString();
        }

        var result = await _hospitalRoleRepository
            .DeleteRoleWithPermissionsAsync(role, rolePermissions);

        if (!result)
            return new ServiceResult<bool>(HttpStatusCode.InternalServerError);

        return new ServiceResult<bool>(true);
    }
}
