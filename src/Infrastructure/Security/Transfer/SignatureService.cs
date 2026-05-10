using System.Security.Cryptography;
using System.Text;
using Core.Application.Interface.Security;
using Microsoft.Extensions.Configuration;

public class SignatureService
{
    public string Sign(string data, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var bytes = Encoding.UTF8.GetBytes(data);

        var signature = rsa.SignData(
            bytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signature);
    }
}

