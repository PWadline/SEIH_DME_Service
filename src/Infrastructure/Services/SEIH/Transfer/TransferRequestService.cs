using Application.Abstractions;
using Core.Application.Commons.ServiceResult;
using Core.Application.Interface.Repository.SEIH;
using Core.Application.Interface.Services.SEIH.Hospital;
using Core.Application.Model.Features;
using Core.Domain.Entity.SEIH;
using System.Net;
using System.Security.Claims;


namespace Infrastructure.Services.SEIH.Hospital;

public class TransferRequestService : ITransferRequestService
{
    private readonly IUsersRepository _usersRepository;
    private readonly ITransferRequestRepository _repository;
    private readonly ISeihTransferClient _client;
    private readonly IHospitalRepository _hospitalRepository;


    public TransferRequestService(
        IHospitalRepository hospitalRepository,
        IUsersRepository usersRepository,
        ITransferRequestRepository repository,
        ISeihTransferClient client)
    {
        _usersRepository = usersRepository;
        _repository = repository;
        _client = client;
        _hospitalRepository = hospitalRepository;
    }


    public async Task<ServiceResult<bool>> CreateAsync(ClaimsPrincipal claim, CreateTransferRequestDto dto)
    {
        var user = await GetUserFromClaim(claim);
        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var entity = new TransferRequestEntity
        {
            IdHospitalFrom = user.HospitalId,
            IdHospitalTo = dto.HospitalToId,
            InfoPatient = dto.InfoPatient,
            RequestReason = dto.Reason,
            Status = TransferRequestStatus.Pending
        };
        Console.WriteLine("REASON BACKEND: " + dto.Reason);

        var created = await _repository.CreateAsync(entity);

        if (!created)
            return new ServiceResult<bool>(HttpStatusCode.BadRequest);

        await _client.SendTransferRequestAsync(new TransferRequestNetworkDto
        {
            RequestId = entity.Id,
            HospitalFromId = entity.IdHospitalFrom,
            HospitalToId = entity.IdHospitalTo,
            InfoPatient = entity.InfoPatient ?? "",
            CreatedAt = entity.Created,
            RequestReason = entity.RequestReason
        });

        return new ServiceResult<bool>(true);
    }


    public async Task<ServiceResult<bool>> RespondAsync(ClaimsPrincipal claim, RespondTransferRequestDto dto)
    {
        var user = await GetUserFromClaim(claim);
        if (user == null)
            return new ServiceResult<bool>(HttpStatusCode.Unauthorized);

        var request = await _repository.GetByIdAsync(dto.RequestId);
        if (request == null)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        if (request.Status != TransferRequestStatus.Pending)
            return new ServiceResult<bool>(false, false, HttpStatusCode.BadRequest, "Request already processed.");

        if (dto.Status == TransferRequestStatus.MoreInfo)
{
    request.Status = TransferRequestStatus.MoreInfo;
    request.ResponseReason = dto.Reason;

    await _repository.UpdateAsync(request);

    await _client.SendTransferRequestResponseAsync(
        new TransferRequestResponseNetworkDto
        {
            RequestId = request.Id,
            Status = request.Status,
            ResponseReason = request.ResponseReason,
            TransferId = request.TransferId
        });

    return new ServiceResult<bool>(true);
}

        request.Status = dto.Status;
        request.ResponseReason = dto.Reason;
        await _repository.UpdateAsync(request);

        await _client.SendTransferRequestResponseAsync(
            new TransferRequestResponseNetworkDto
            {
                RequestId = request.Id,
                Status = request.Status,
                ResponseReason = request.ResponseReason,
                TransferId = request.TransferId
            });

        return new ServiceResult<bool>(true);
    }


    public async Task<ServiceResult<IEnumerable<TransferRequestListItemDto>>> GetMyRequestsAsync(ClaimsPrincipal claim)
    {
        var user = await GetUserFromClaim(claim);
        if (user == null)
            return new ServiceResult<IEnumerable<TransferRequestListItemDto>>(HttpStatusCode.Unauthorized);

        // var entities = await _repository.GetHospitalRequestsAsync(user.HospitalId);
        // 🔥 1. Sync avec HUB
        await SyncIncomingRequests(user.HospitalId);

        // 🔥 2. Ensuite lire DB locale mise à jour
        var entities = await _repository.GetHospitalRequestsAsync(user.HospitalId);

        var hospitalIds = entities
            .SelectMany(x => new[] { x.IdHospitalFrom, x.IdHospitalTo })
            .Distinct()
            .ToList();

        var hospitals = await _hospitalRepository.GetByIdsAsync(hospitalIds);
        var hospitalDict = hospitals.ToDictionary(h => h.Id, h => h.Name);

        int counter = entities.Count();

        var result = entities.Select((x, index) =>
        {
            var isSent = x.IdHospitalFrom == user.HospitalId;

            var type = isSent ? "envoye" : "recu";



            var institution = isSent
                ? hospitalDict.GetValueOrDefault(x.IdHospitalTo, "Inconnu")
                : hospitalDict.GetValueOrDefault(x.IdHospitalFrom, "Inconnu");

            var institutionId = isSent
                ? x.IdHospitalTo
                : x.IdHospitalFrom;

            var patientInfo = string.Join(", ", (x.InfoPatient ?? "").Split('|').Select(p => p.Trim()));

            var consentement = x.IdConsent != Guid.Empty;

            var statut = x.Status switch
            {
                TransferRequestStatus.Pending => "En attente",
                TransferRequestStatus.Approved => "Approuvé",
                TransferRequestStatus.Rejected => "Rejeté",
                TransferRequestStatus.MoreInfo => "Infos demandées",
                _ => "Inconnu"
            };

            string? numeroTransfert = x.TransferId?.ToString();

            return new TransferRequestListItemDto
            {
                Id = x.Id,
                Institution = institution,
                InstitutionId = institutionId,
                Type = type,
                NumeroTransfert = numeroTransfert,
                ResponseReason = x.ResponseReason,
                RequestReason = x.RequestReason,
                Created = x.Created,
                PatientInfo = patientInfo,
                Consentement = consentement,
                Statut = statut
            };
        });

        return new ServiceResult<IEnumerable<TransferRequestListItemDto>>(result);
    }


    public async Task<ServiceResult<bool>> LinkTransferAsync(Guid requestId, Guid transferId)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null)
            return new ServiceResult<bool>(HttpStatusCode.NotFound);

        if (request.Status != TransferRequestStatus.Pending)
            return new ServiceResult<bool>(false, false, HttpStatusCode.BadRequest, "Cannot link transfer to processed request.");

        request.TransferId = transferId;
        request.Status = TransferRequestStatus.Approved;

        await _repository.UpdateAsync(request);

        await _client.SendTransferRequestResponseAsync(
        new TransferRequestResponseNetworkDto
        {
            RequestId = request.Id,
            Status = request.Status,
            ResponseReason = "Transfert effectué",
            TransferId = transferId   
        });

        return new ServiceResult<bool>(true);
    }


    private async Task<UsersEntity?> GetUserFromClaim(ClaimsPrincipal claim)
    {
        var email = claim.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _usersRepository.GetUserByEmailAsync(email);
    }

    public async Task SyncIncomingRequests(Guid hospitalId)
    {
        var incoming = await _client.GetIncomingRequestsAsync(hospitalId);

        foreach (var dto in incoming)
        {
            var existing = await _repository.GetByIdAsync(dto.RequestId);

            if (existing == null)
            {
                var entity = new TransferRequestEntity
                {
                    Id = dto.RequestId,
                    IdHospitalFrom = dto.HospitalFromId,
                    IdHospitalTo = dto.HospitalToId,
                    InfoPatient = dto.InfoPatient,
                    Status = dto.Status,
                    RequestReason = dto.RequestReason,
                    ResponseReason = dto.ResponseReason
                };

                await _repository.CreateAsync(entity);
            }
            else
            {
                // 🔥 cas mise à jour statut
                bool hasChanged = false;

                // 🔥 statut
                if (existing.Status != dto.Status)
                {
                    existing.Status = dto.Status;
                    hasChanged = true;
                }

                // 🔥 RequestReason (remplir UNIQUEMENT si vide)
                if (string.IsNullOrWhiteSpace(existing.RequestReason)
                    && !string.IsNullOrWhiteSpace(dto.RequestReason))
                {
                    existing.RequestReason = dto.RequestReason;
                    hasChanged = true;
                }

                // 🔥 ResponseReason (peut évoluer)
                if (!string.IsNullOrWhiteSpace(dto.ResponseReason) && existing.ResponseReason != dto.ResponseReason)
                {
                    existing.ResponseReason = dto.ResponseReason;
                    hasChanged = true;
                }

                if (dto.TransferId.HasValue && existing.TransferId != dto.TransferId)
                {
                    existing.TransferId = dto.TransferId;
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    await _repository.UpdateAsync(existing);
                }
            }
        }
    }

    


}

