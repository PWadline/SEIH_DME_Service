using System.Security.Cryptography;
using System.Text;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Interface.Services.SEIH.Hospital;

namespace Infrastructure.Services.SEIH.Hospital;

public class SeihCryptoService : ISeihCryptoService
{
    public byte[] DecryptSessionKey(byte[] encryptedKey, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem.ToCharArray());

        return rsa.Decrypt(
            encryptedKey,
            RSAEncryptionPadding.OaepSHA256);
    }

    public byte[] DecryptPayload(byte[] encryptedPayload, byte[] aesKey, byte[] iv)
    {
        using var aes = Aes.Create();

        aes.Key = aesKey;
        aes.IV = iv;

        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();

        return decryptor.TransformFinalBlock(
            encryptedPayload,
            0,
            encryptedPayload.Length);
    }
    public (byte[] encrypted, byte[] iv) EncryptPayload(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();

        aes.Key = key;
        aes.GenerateIV();

        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();

        var encrypted = encryptor.TransformFinalBlock(
            data,
            0,
            data.Length);

        return (encrypted, aes.IV);
    }

    public byte[] EncryptSessionKey(byte[] sessionKey, string publicKeyPem)
    {


        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem.ToCharArray());

        return rsa.Encrypt(
            sessionKey,
            RSAEncryptionPadding.OaepSHA256);

    }

    public string ComputeSHA256(byte[] payload)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(payload);

        return Convert.ToBase64String(hash);
    }

    public string Sign(byte[] payload, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem.ToCharArray());

        var signature = rsa.SignData(
            payload,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signature);
    }

    public bool VerifySignature(byte[] payload, string signatureBase64, string publicKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem.ToCharArray());

        var signature = Convert.FromBase64String(signatureBase64);

        return rsa.VerifyData(
            payload,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }

    public byte[] GenerateAesKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();

        return aes.Key;
    }
}

