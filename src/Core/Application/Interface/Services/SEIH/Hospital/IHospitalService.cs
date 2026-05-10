using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;
using Core.Application.Model.Features.Hospital;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Services.SEIH;

public interface IHospitalService
{
    Task<ServiceResult<List<HospitalDto>>> GetAllAsync();
    Task<ServiceResult<HospitalDto?>> GetByIdAsync(Guid id);
    Task<ServiceResult<bool>> UpdateAsync(UpdateHospitalDto dto);
    Task<ServiceResult<bool>> DeleteAsync(Guid id);
    Task<ServiceResult<HospitalDto>> CreateAsync(CreateHospitalDto dto);
    Task<ServiceResult<KeyGenerationDto>> GenerateAsync(ClaimsPrincipal claim);
    Task<ServiceResult> ConfirmAsync(ClaimsPrincipal claim, string publicKey);
    Task<ServiceResult<KeyStatusDto>> GetKeyStatusAsync(ClaimsPrincipal claim);
}


