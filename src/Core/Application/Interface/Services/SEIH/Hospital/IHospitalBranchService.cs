using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features.Hospital;
using Core.Application.Model.Features;
using System.Security.Claims;


namespace Core.Application.Interface.Services.SEIH;

public interface IHospitalBranchService
{
    // Task<ServiceResult<List<HospitalBranchDto>>> GetAllByHospitalAsync(Guid hospitalId);
    // Task<ServiceResult<HospitalBranchDto?>> GetByIdAsync(Guid id);
    // Task<ServiceResult<HospitalBranchDto>> CreateAsync(CreateHospitalBranchDto dto);
    // Task<ServiceResult<bool>> UpdateAsync(UpdateHospitalBranchDto dto);
    // Task<ServiceResult<bool>> DeleteAsync(Guid id);



   Task<ServiceResult<List<HospitalBranchDto>>> GetAllByHospitalAsync(
        ClaimsPrincipal claim);

    Task<ServiceResult<HospitalBranchDto?>> GetByIdAsync(
        ClaimsPrincipal claim,
        Guid id);

    Task<ServiceResult<HospitalBranchDto>> CreateAsync(
        ClaimsPrincipal claim,
        CreateHospitalBranchDto dto);

    Task<ServiceResult<bool>> UpdateAsync(
        ClaimsPrincipal claim,
        UpdateHospitalBranchDto dto);

    Task<ServiceResult<bool>> DeleteAsync(
        ClaimsPrincipal claim,
        Guid id);
}

