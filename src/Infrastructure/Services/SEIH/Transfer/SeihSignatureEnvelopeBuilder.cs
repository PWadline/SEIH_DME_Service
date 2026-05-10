using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Model.Features;
using Core.Application.Interface.Services.SEIH.Transfer;


namespace Infrastructure.Services.SEIH.Transfer;

public class SeihSignatureEnvelopeBuilder : ISignatureEnvelopeBuilder
{
    private readonly ILogger<SeihSignatureEnvelopeBuilder> _logger;

    public SeihSignatureEnvelopeBuilder(ILogger<SeihSignatureEnvelopeBuilder> logger)
    {
        _logger = logger;
    }


    public byte[] BuildEnvelope(
    byte[] encryptedPayload,
    byte[] encryptedKey,
    byte[] iv,
    string payloadHash,
    int keyVersion,
    string nonce,
    DateTime signedAt)
{
    var signedAtUtc = signedAt.Kind == DateTimeKind.Utc
        ? signedAt
        : signedAt.ToUniversalTime();

    var base64Payload = Convert.ToBase64String(encryptedPayload);
    var base64Key = Convert.ToBase64String(encryptedKey);
    var base64Iv = Convert.ToBase64String(iv);

    var data =
        $"{payloadHash}|" +
        $"{keyVersion}|" +
        $"{nonce}|" +
        $"{signedAtUtc:O}|" +
        $"{base64Payload}|" +
        $"{base64Key}|" +
        $"{base64Iv}";

    _logger.LogInformation("SIGNATURE STRING: {data}", data);

    return Encoding.UTF8.GetBytes(data);
}
}