using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Application.Model.Features;
using Core.Application.Model.Features.Hospital;
using static Core.Application.Contracts.Transfers;

namespace Application.Abstractions;

public interface ISeihTransferClient
{
    Task<CreateTransferResult> PrepareAsync(long fileSize,Guid senderHospitalId,Guid recipientHospitalId,CancellationToken ct = default);
    Task<UploadCompleteResult> UploadAsync(Stream file, string transferId, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string transferId); 
    Task<List<HospitalWithKeysDto>> GetHospitalsAsync(CancellationToken ct);
    Task<string> PingAsync();
    Task<bool> CreateTransferAsync(TransferReceiveDto dto);
    Task<bool> SendTransferRequestAsync(TransferRequestNetworkDto dto);
    Task SendTransferRequestResponseAsync(TransferRequestResponseNetworkDto dto, CancellationToken ct = default);
    Task<List<IncomingTransferDto>> GetIncomingAsync(Guid hospitalId);
    Task AckAsync(Guid transferId, Guid hospitalId);
    Task FailAsync(Guid transferId, Guid hospitalId);
    Task<IEnumerable<TransferRequestNetworkDto>>GetIncomingRequestsAsync(Guid hospitalId);
    Task RegisterPublicKeyAsync(RegisterPublicKeyRequest dto);
    Task RegisterTransferKeyAsync(RegisterTransferKeyRequest dto);
    Task SendMetadataAsync(string transferId, object metadata);

    
}