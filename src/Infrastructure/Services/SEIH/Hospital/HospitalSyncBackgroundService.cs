using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features;
using Core.Domain.Entity.SEIH;
using System.Net;

using Core.Application.Interface.Repository.SEIH.Hospital;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Application.Model.Features.Hospital;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Infrastructure.Services.SEIH;

public class HospitalSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HospitalSyncBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IHospitalSyncService>();

            await syncService.SyncAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}





