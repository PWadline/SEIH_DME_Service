
using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features.Record;
using Core.Domain.Entity.SEIH;
using System.IO.Compression;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Record;

public interface IRecordService
{
    Task<ServiceResult<RecordDto>> CreateAsync(ClaimsPrincipal claim, CreateRecordDto dto);
    Task<ServiceResult<RecordDto>> GetByIdAsync(ClaimsPrincipal claim, Guid id);
    Task<ServiceResult<IEnumerable<RecordDto>>> GetByHospitalAsync(ClaimsPrincipal claim);
    Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal claim, Guid id);
    Task<ServiceResult<RecordDto>> UpdateAsync(ClaimsPrincipal claim, UpdateRecordDto dto);
    Task<ServiceResult<RecordDto>> ImportFromTransferAsync(ClaimsPrincipal claim, ImportTransferredRecordDto dto, Guid branchId, ZipArchive archive, Guid transferId);
    Task<ServiceResult<bool>> ResetPatientCodeAsync(ResetPatientCodeDto dto);
    Task<ServiceResult<List<HospitalEntity>>> GetAuthorizedHospitalsAsync(Guid recordId);
}
