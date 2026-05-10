using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;
using Core.Application.Model.Features.Hospital;

namespace Core.Application.Interface.Services.SEIH;

public interface IInstitutionKeyService
{
    Task<ServiceResult<RegisterPublicKeyResponse>> RegisterPublicKeyAsync(ClaimsPrincipal claim, string publicKey);
}
