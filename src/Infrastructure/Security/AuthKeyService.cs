using Core.Application.Interface.Security;
using Microsoft.Extensions.Configuration;

public class AuthKeyService : IAuthKeyService
{
    private readonly IConfiguration _config;

    public AuthKeyService(IConfiguration config)
    {
        _config = config;
    }

    public string GetPrivateKeyPem()
    {
        var path = _config["SEIH:AuthKey:PrivateKeyPath"];
        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            path!);

        return File.ReadAllText(fullPath);
    }
}