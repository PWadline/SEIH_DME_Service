using Core.Application.Commons.ServiceResult;
using Core.Application.Model.Features;
using System.Security.Claims;

namespace Core.Application.Interface.Services.SEIH.Hospital;

public interface ISeihCryptoService
{
    byte[] DecryptSessionKey(byte[] encryptedKey, string privateKeyPem);

    byte[] DecryptPayload(byte[] encryptedPayload, byte[] aesKey, byte[] iv);

    (byte[] encrypted, byte[] iv) EncryptPayload(byte[] data, byte[] key);

    byte[] EncryptSessionKey(byte[] sessionKey, string publicKeyPem);

    string ComputeSHA256(byte[] payload);

    string Sign(byte[] payload, string privateKeyPem);

    bool VerifySignature(byte[] payload, string signatureBase64, string publicKeyPem);

    byte[] GenerateAesKey();
}
