using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface ITransferService
{
    Task<ServiceResult<bool>> CreateTransferAsync(
        ClaimsPrincipal claim,
        CreateTransferDto request);

    Task<ServiceResult<IEnumerable<TransferListItemDto>>> GetTransferListAsync(
        ClaimsPrincipal claim);
}