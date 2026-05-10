
using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features.Record;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Record;

public interface ISeihFileService
{
    Task<ServiceResult<UploadFileResponseDto>> UploadAsync(ClaimsPrincipal user, UploadFileRequestDto model);
    Task<ServiceResult<FileDownloadDto>> DownloadAsync(ClaimsPrincipal user, Guid fileId);
    Task<ServiceResult<bool>> DeleteAsync(ClaimsPrincipal user, Guid fileId);
    Task<ServiceResult<FileStorageDto?>> GetByIdAsync(Guid id);
    Task<ServiceResult<UploadFileResponseDto>> UploadFromStreamAsync(ClaimsPrincipal claim,UploadFileFromStreamDto dto);
}