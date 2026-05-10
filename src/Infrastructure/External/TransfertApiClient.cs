using System.Text.Json;
using Core.DTOs.External;
using System.Net.Http.Json;
using Core.Application.Model.Features;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.External;

public class TransfertApiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public TransfertApiClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    // 🔁 CREATE TRANSFER
    public async Task<bool> CreateTransferAsync(TransferReceiveDto request)
    {
        Console.WriteLine("CALLING RECEIVE ENDPOINT");
        Console.WriteLine(_http.BaseAddress + "api/rest/seih/transfer/receive");
        Console.WriteLine(JsonSerializer.Serialize(request));


        Console.WriteLine("DEBUG DTO:");
        Console.WriteLine(JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        // 🔐 HEADERS nécessaires pour le middleware du HUB
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        _http.DefaultRequestHeaders.Remove("X-KEY-VERSION");
        _http.DefaultRequestHeaders.Add("X-KEY-VERSION", request.KeyVersion.ToString());

        _http.DefaultRequestHeaders.Remove("X-TIMESTAMP");
        _http.DefaultRequestHeaders.Add("X-TIMESTAMP", timestamp);

        // ⚠️ Pour test seulement (signature désactivée côté hub)
        var apiKey = _config["SEIH:ApiKey"];

        _http.DefaultRequestHeaders.Remove("X-API-KEY");
        _http.DefaultRequestHeaders.Add("X-API-KEY", apiKey);


        _http.DefaultRequestHeaders.Remove("X-SIGNATURE");
        _http.DefaultRequestHeaders.Add("X-SIGNATURE", "test-signature");

        var response = await _http.PostAsJsonAsync(
            "api/rest/seih/transfer/receive",
            request);

        Console.WriteLine("STATUS CODE: " + response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Transfert API error: {error}");
        }

        return true;
    }

    public async Task<List<TransferDto>> GetTransferListAsync(Guid hospitalId)
    {
        var response = await _http.PostAsJsonAsync(
            "api/rest/seih/transfer/incoming",
            hospitalId
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Transfert API error: {error}");
        }

        var result = await response.Content
            .ReadFromJsonAsync<List<TransferDto>>();

        return result ?? new List<TransferDto>();
    }
    // public async Task<List<TransferDto>> GetTransferListAsync(Guid hospitalId)
    // {
    //     var response = await _http.PostAsync(
    //         "api/rest/seih/transfer/incoming",
    //         hospitalId);

    //     if (!response.IsSuccessStatusCode)
    //     {
    //         var error = await response.Content.ReadAsStringAsync();
    //         throw new Exception($"Transfert API error: {error}");
    //     }

    //     var result = await response.Content
    //         .ReadFromJsonAsync<List<TransferDto>>();

    //     return result ?? new List<TransferDto>();
    // }

    public async Task<List<PublicHospitalDto>> GetHospitalsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<PublicHospitalDto>>(
                "seih/hospital/public/list")
            ?? new List<PublicHospitalDto>();
        }
        catch
        {
            return new List<PublicHospitalDto>();
        }
    }
}