
using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features.Hospital;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface IHospitalRoleService
{
    Task<ServiceResult<bool>> HospitalCreateRoleServiceAsync(ClaimsPrincipal claim, HospitalRoleDto dataModel);
    Task<ServiceResult<IEnumerable<RoleWithPermissionsDto>>> GetAllRoleServiceAsync(ClaimsPrincipal claim);
    Task<ServiceResult<bool>> UpdateRoleServiceAsync(ClaimsPrincipal claim, UpdateRoleDto dataModel);
    Task<ServiceResult<bool>> DeleteRoleServiceAsync(ClaimsPrincipal claim, DeleteRoleDto dataModel);
}
