using System.Security.Cryptography;
using System.Text;
using Core.Application.Interface.Security;
using Microsoft.Extensions.Configuration;

public class HashService
{
    public string ComputeSha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}

