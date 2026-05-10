namespace Core.Application.Interface.Services.SEIH.Transfer;

public interface ISignatureEnvelopeBuilder
{
    byte[] BuildEnvelope(
        byte[] encryptedPayload,
        byte[] encryptedKey,
        byte[] iv,
        string payloadHash,
        int keyVersion,
        string nonce,
        DateTime signedAt
    );
}
