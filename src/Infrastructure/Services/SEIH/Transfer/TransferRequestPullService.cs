
using Microsoft.Extensions.Configuration;
using Application.Abstractions;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Domain.Entity.SEIH;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace Infrastructure.Services.Transfer
{

    public class TransferRequestPullService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;

        public TransferRequestPullService(
            IServiceProvider serviceProvider,
            IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
        }
        private Guid GetCurrentHospitalId()
        {
            var value = _config["SEIH:HospitalId"];

            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("SEIH:HospitalId is not configured.");

            if (!Guid.TryParse(value, out var hospitalId))
                throw new Exception("SEIH:HospitalId is not a valid GUID.");

            return hospitalId;
        }
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var client = scope.ServiceProvider
                    .GetRequiredService<ISeihTransferClient>();

                var repository = scope.ServiceProvider
                    .GetRequiredService<ITransferRequestRepository>();

                var hospitalService = scope.ServiceProvider
                    .GetRequiredService<IHospitalService>();

                // 🔴 Ici tu dois récupérer l'ID de l’hôpital courant
                var hospitalId = GetCurrentHospitalId();

                var incoming = await client
                    .GetIncomingRequestsAsync(hospitalId);

                foreach (var dto in incoming)
                {
                    var exists = await repository.GetByIdAsync(dto.RequestId);
                    if (exists != null)
                        continue;

                    var entity = new TransferRequestEntity
                    {
                        Id = dto.RequestId,
                        IdHospitalFrom = dto.HospitalFromId,
                        IdHospitalTo = dto.HospitalToId,
                        InfoPatient = dto.InfoPatient,
                        RequestReason = dto.RequestReason,
                        ResponseReason = dto.ResponseReason,
                        Status = TransferRequestStatus.Pending
                    };

                    await repository.CreateAsync(entity);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }


    }

}