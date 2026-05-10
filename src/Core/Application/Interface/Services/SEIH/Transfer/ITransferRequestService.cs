using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface ITransferRequestService
{
   Task<ServiceResult<bool>> CreateAsync(
        ClaimsPrincipal claim,
        CreateTransferRequestDto dto);

    Task<ServiceResult<bool>> RespondAsync(
        ClaimsPrincipal claim,
        RespondTransferRequestDto dto);

    Task<ServiceResult<IEnumerable<TransferRequestListItemDto>>> 
        GetMyRequestsAsync(ClaimsPrincipal claim);

    Task<ServiceResult<bool>> LinkTransferAsync(
        Guid requestId,
        Guid transferId);
}
