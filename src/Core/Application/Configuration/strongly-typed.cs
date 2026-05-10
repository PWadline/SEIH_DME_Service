public class SeihOptions
{
    public string BaseUrl { get; set; } = null!;
    public ClientCertOptions ClientCert { get; set; } = null!;
}

public class ClientCertOptions
{
    public string Path { get; set; } = null!;
    public string Password { get; set; } = null!;
}