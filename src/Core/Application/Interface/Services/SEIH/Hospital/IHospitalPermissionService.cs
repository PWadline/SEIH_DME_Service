
using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using Core.Application.Model.Features.Hospital;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface IHospitalPermissionService
{
    // Task<ServiceResult<IEnumerable<string>>> GetAllPermissionServiceAsync();
    Task<ServiceResult<IEnumerable<PermissionDto>>> GetAllPermissionServiceAsync();
}
