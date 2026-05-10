using System.Text.Json;

public static class SeihJsonOptions
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null, 
        WriteIndented = false,       
    };
}