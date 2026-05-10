
// using Core.Application.Commons.ServiceResult;
// using Core.Application.Model.Features.Record;
// using System.Security.Claims;

// namespace Core.Application.Interface.Services.SEIH.Record;

// public interface IFormService
// {
// Task<ServiceResult<FormDto>> CreateAsync(
//         ClaimsPrincipal claim,
//         CreateFormDto dto);

//     Task<ServiceResult<FormDto>> GetByIdAsync(
//         ClaimsPrincipal claim,
//         Guid id);

//     Task<ServiceResult<IEnumerable<FormDto>>> GetByHospitalAsync(
//         ClaimsPrincipal claim);

//     Task<ServiceResult<bool>> DeleteAsync(
//         ClaimsPrincipal claim,
//         Guid id);

//     Task<ServiceResult<FormDto>> UpdateAsync(
//         ClaimsPrincipal claim,
//         UpdateFormDto dto);
// }

using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features.Record;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Core.Application.Interface.Services.SEIH.Record
{
    public interface IFormService
    {
        Task<ServiceResult<FormDto>> CreateAsync(ClaimsPrincipal claim, CreateFormDto dto);
        Task<ServiceResult<FormDto>> GetByIdAsync(ClaimsPrincipal claim, Guid id);
        Task<ServiceResult<FormDto>> UpdateAsync(ClaimsPrincipal claim, UpdateFormDto dto);
        Task<ServiceResult<IEnumerable<FormDto>>> GetByHospitalAsync(ClaimsPrincipal claim);
        Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal claim, Guid id);
    }
}