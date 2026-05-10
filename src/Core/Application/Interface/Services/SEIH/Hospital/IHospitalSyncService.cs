using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;
using Core.Application.Model.Features.Hospital;

namespace Core.Application.Interface.Services.SEIH;

public interface IHospitalSyncService
{
    Task SyncAsync(CancellationToken ct = default);
}


