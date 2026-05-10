using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface IInboundTransferService
{
    Task PullIncomingTransfersAsync(Guid hospitalId);
}
