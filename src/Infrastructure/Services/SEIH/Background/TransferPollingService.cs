using Core.Application.Interface.Services.SEIH.Hospital;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.SEIH.Background;

public class TransferPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransferPollingService> _logger;
    private readonly IConfiguration _config;

    public TransferPollingService(
    IServiceScopeFactory scopeFactory,
    ILogger<TransferPollingService> logger,
    IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SEIH Transfer Polling Service started.");

        // ⏳ jitter au démarrage
        _logger.LogInformation("Waiting before first poll...");

        var startupDelay = Random.Shared.Next(5, 20);

        _logger.LogInformation("Startup delay: {Delay} seconds", startupDelay);

        await Task.Delay(TimeSpan.FromSeconds(startupDelay), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var inboundService = scope.ServiceProvider
                    .GetRequiredService<IInboundTransferService>();

                var hospitalId = GetLocalHospitalId();
                _logger.LogInformation("🚀 POLLING START for hospital {HospitalId}", hospitalId);

                await inboundService.PullIncomingTransfersAsync(hospitalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transfer polling.");
            }

            // ⏱ jitter normal entre 25 et 35 secondes
            var delay = Random.Shared.Next(25, 35);

            _logger.LogInformation("Next transfer polling in {Delay} seconds", delay);

            await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
        }
    }

    private Guid GetLocalHospitalId()
    {
        var id = _config["SEIH:HospitalId"];

        if (string.IsNullOrWhiteSpace(id))
            throw new Exception("SEIH:HospitalId not configured");

        return Guid.Parse(id);
    }
}