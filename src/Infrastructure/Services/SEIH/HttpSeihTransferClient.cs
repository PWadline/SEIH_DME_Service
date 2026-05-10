using Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static Core.Application.Contracts.Transfers;
using Core.Application.Model.Features.Hospital;
using Core.Application.Model.Features;
using Core.Application.Interface.Services.SEIH;
using Core.Application.Interface.Services.SEIH.Hospital;
using System.Security.Cryptography;

namespace Infrastructure.Services.SEIH;

public sealed class HttpSeihTransferClient : ISeihTransferClient
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpSeihTransferClient> _logger;
    private readonly IConfiguration _config;
    private readonly ISeihCryptoService _cryptoService;
    private readonly string _privateKeyPem;

    public HttpSeihTransferClient(
     HttpClient http,
     ILogger<HttpSeihTransferClient> logger,
     IConfiguration config,
     ISeihCryptoService cryptoService)
    {
        _http = http;
        _logger = logger;
        _config = config;
        _cryptoService = cryptoService;

        // 🔥 AJOUT ICI
        var privateKeyPath = _config["SEIH:AuthKey:PrivateKeyPath"]!;
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), privateKeyPath);

        _privateKeyPem = File.ReadAllText(fullPath);
    }

    public async Task<CreateTransferResult> PrepareAsync(long fileSize, Guid senderHospitalId, Guid recipientHospitalId, CancellationToken ct = default)
    {
        var create = new
        {
            SenderHospitalId = senderHospitalId,
            RecipientHospitalId = recipientHospitalId,
            Size = fileSize,
            ContentType = "application/octet-stream"
        };

        var body = JsonSerializer.Serialize(create);

        using var request = CreateSignedRequest(HttpMethod.Post, "/api/rest/transfers", body);

        var resp = await _http.SendAsync(request, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            _logger.LogError("PREPARE FAILED: {status} {error}", resp.StatusCode, error);
            throw new Exception(error);
        }

        var payload = await resp.Content.ReadFromJsonAsync<CreateTransferResult>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Empty response from /api/transfers");

        _logger.LogInformation("Created transfer {TransferId}", payload.TransferId);

        return payload;
    }

    public async Task<UploadCompleteResult> UploadAsync(Stream file, string transferId, CancellationToken ct = default)
    {
        const int chunkSize = 2 * 1024 * 1024;
        var buffer = new byte[chunkSize];
        int part = 0, read;
        _logger.LogWarning("UPLOAD PART {part} START", part);

        while ((read = await file.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
        {
            part++;

            var bytes = buffer.AsMemory(0, read).ToArray();

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/rest/transfers/{transferId}/parts?part={part}");

            request.Content = new ByteArrayContent(bytes);
            request.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");


            var hashBytes = SHA256.HashData(bytes);
            var hash = Convert.ToHexString(hashBytes);


            AddSecurityHeaders(request, hash);

            var resp = await _http.SendAsync(request, ct);
            resp.EnsureSuccessStatusCode();
            _logger.LogWarning("UPLOAD PART {part} SUCCESS", part);
        }

        _logger.LogWarning("TOTAL PARTS SENT: {count}", part);

        var complete = new { ManifestJws = "{ \"jws\": \"<todo>\" }" };
        var completeBody = JsonSerializer.Serialize(complete);

        using var completeRequest = CreateSignedRequest(
            HttpMethod.Post,
            $"/api/rest/transfers/{transferId}/complete",
            completeBody);

        var r2 = await _http.SendAsync(completeRequest, ct);
        r2.EnsureSuccessStatusCode();

        return new UploadCompleteResult(transferId, part);
    }

    private HttpRequestMessage CreateSignedRequest(HttpMethod method, string url, string body)
    {
        var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrWhiteSpace(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            // 🔥 AJOUT ICI
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            var hashBytes = SHA256.HashData(bodyBytes);
            var hash = Convert.ToHexString(hashBytes);

            AddSecurityHeaders(request, hash);
        }
        else
        {
            AddSecurityHeaders(request, "");
        }

        return request;
    }


    private void AddSecurityHeaders(HttpRequestMessage request, string payloadToSign)
    {
        var apiKey = _config["SEIH:ApiKey"]!;
        var keyVersion = _config["SEIH:AuthKey:KeyVersion"]!;

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();


        var payload = timestamp + payloadToSign;

        var signature = _cryptoService.Sign(
            Encoding.UTF8.GetBytes(payload),
            _privateKeyPem
        );

        request.Headers.Remove("X-API-KEY");
        request.Headers.Remove("X-TIMESTAMP");
        request.Headers.Remove("X-SIGNATURE");
        request.Headers.Remove("X-KEY-VERSION");

        request.Headers.Add("X-API-KEY", apiKey);
        request.Headers.Add("X-TIMESTAMP", timestamp);
        request.Headers.Add("X-SIGNATURE", signature);
        request.Headers.Add("X-KEY-VERSION", keyVersion);
    }


    public async Task<bool> CreateTransferAsync(TransferReceiveDto dto)
    {
        var body = JsonSerializer.Serialize(dto);

        using var request = CreateSignedRequest(
            HttpMethod.Post,
            "/api/rest/seih/transfer/receive",
            body);

        _logger.LogInformation(
            "TRANSFER DEBUG → From:{From} To:{To} KeyVersion:{Key} Nonce:{Nonce}",
            dto.IdHospitalFrom,
            dto.IdHospitalTo,
            dto.KeyVersion,
            dto.Nonce);

        var response = await _http.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("HUB RESPONSE STATUS: {status}", response.StatusCode);
        _logger.LogInformation("HUB RESPONSE BODY: {body}", responseBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"SEIH HUB ERROR {response.StatusCode} : {responseBody}");
        }

        return true;
    }


    public async Task<string> PingAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/rest/seih/hospital/ping");

        AddSecurityHeaders(request, "");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }


    public async Task<List<IncomingTransferDto>> GetIncomingAsync(Guid hospitalId)
    {
        var body = JsonSerializer.Serialize(new { HospitalId = hospitalId });

        using var request = CreateSignedRequest(
            HttpMethod.Post,
            "api/rest/seih/transfer/incoming",
            body
        );

        _logger.LogWarning("===== DEBUG INCOMING =====");
        _logger.LogWarning("HospitalId envoyé: {id}", hospitalId);
        _logger.LogWarning("ApiKey utilisé: {key}", _config["SEIH:ApiKey"]);

        var response = await _http.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogWarning("🔥 RESPONSE STATUS: {status}", response.StatusCode);
        _logger.LogWarning("🔥 RESPONSE BODY: {body}", responseBody);

        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<List<IncomingTransferDto>>(responseBody, options)
                     ?? new List<IncomingTransferDto>();


        if (result.Count > 0)
        {
            _logger.LogInformation("===== DEBUG DESERIALIZATION =====");
            _logger.LogInformation("RAW JSON: {json}", responseBody);
            _logger.LogInformation("DESERIALIZED ID: {id}", result[0].Id);
            _logger.LogInformation("DESERIALIZED From: {from}", result[0].IdHospitalFrom);
            _logger.LogInformation("DESERIALIZED To: {to}", result[0].IdHospitalTo);
        }

        return result;
    }

    public async Task AckAsync(Guid transferId, Guid hospitalId)
    {
        var apiKey = _config["SEIH:ApiKey"]
            ?? throw new InvalidOperationException("ApiKey missing");

        var dto = new
        {
            TransferId = transferId,
            HospitalId = hospitalId
        };

        var body = JsonSerializer.Serialize(dto);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/rest/seih/transfer/ack");

        request.Content = new StringContent(body, Encoding.UTF8, "application/json");


        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hashBytes = SHA256.HashData(bodyBytes);
        var hash = Convert.ToHexString(hashBytes);


        AddSecurityHeaders(request, hash);

        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("ACK RESPONSE STATUS: {status}", response.StatusCode);
        _logger.LogInformation("ACK RESPONSE BODY: {body}", responseBody);

        response.EnsureSuccessStatusCode();
    }


    public async Task FailAsync(Guid transferId, Guid hospitalId)
    {
        var dto = new
        {
            TransferId = transferId,
            HospitalId = hospitalId
        };

        var body = JsonSerializer.Serialize(dto);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/rest/seih/transfer/fail");

        request.Content = new StringContent(
            body,
            Encoding.UTF8,
            "application/json");


        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hashBytes = SHA256.HashData(bodyBytes);
        var hash = Convert.ToHexString(hashBytes);

        AddSecurityHeaders(request, hash);

        var response = await _http.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("FAIL RESPONSE STATUS: {status}", response.StatusCode);
        _logger.LogInformation("FAIL RESPONSE BODY: {body}", responseBody);

        response.EnsureSuccessStatusCode();
    }


    public async Task<List<HospitalWithKeysDto>> GetHospitalsAsync(CancellationToken ct)
    {
        var apiKey = _config["SEIH:ApiKey"];

        if (_http.DefaultRequestHeaders.Contains("X-API-KEY"))
            _http.DefaultRequestHeaders.Remove("X-API-KEY");

        _http.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

        var response = await _http.PostAsync(
            "api/rest/seih/hospital/network/list",
            null,
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<List<HospitalWithKeysDto>>(cancellationToken: ct);

        return result ?? new List<HospitalWithKeysDto>();
    }


    public async Task SendMetadataAsync(string transferId, object metadata)
    {
        var body = JsonSerializer.Serialize(metadata);

        using var request = CreateSignedRequest(
            HttpMethod.Post,
            $"/api/rest/transfers/{transferId}/metadata",
            body
        );

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }


    public async Task<bool> SendTransferRequestAsync(TransferRequestNetworkDto dto)
    {
        var body = JsonSerializer.Serialize(dto);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/dme/network/transfer-request/receive");

        request.Content = new StringContent(
            body,
            Encoding.UTF8,
            "application/json");

        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hashBytes = SHA256.HashData(bodyBytes);
        var hash = Convert.ToHexString(hashBytes);

        AddSecurityHeaders(request, hash);

        var response = await _http.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return true;
    }


    public async Task SendTransferRequestResponseAsync(TransferRequestResponseNetworkDto dto, CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(dto);
        Console.WriteLine("===== DME A SEND RESPONSE =====");
        Console.WriteLine(body);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hashBytes = SHA256.HashData(bodyBytes);
        var hash = Convert.ToHexString(hashBytes);

        // var request = new HttpRequestMessage(HttpMethod.Post, "/api/rest/transfers");

        var request = new HttpRequestMessage(HttpMethod.Post, "/dme/network/transfer-request/response");

        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        AddSecurityHeaders(request, hash);

        _logger.LogInformation(
            "Sending SEIH transfer request response for request {RequestId}",
            dto.RequestId);

        var response = await _http.SendAsync(request, ct);

        response.EnsureSuccessStatusCode();
    }


    public async Task<IEnumerable<TransferRequestNetworkDto>> GetIncomingRequestsAsync(Guid hospitalId)
    {
        var response = await _http.PostAsJsonAsync(
            "dme/network/transfer-request/incoming",
            new { HospitalId = hospitalId });

        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<TransferRequestNetworkDto>();

        return await response.Content
            .ReadFromJsonAsync<IEnumerable<TransferRequestNetworkDto>>()
            ?? Enumerable.Empty<TransferRequestNetworkDto>();
    }


    public async Task RegisterPublicKeyAsync(RegisterPublicKeyRequest dto)
    {
        var body = JsonSerializer.Serialize(dto);

        using var request = CreateSignedRequest(
            HttpMethod.Post,
            "/api/rest/seih/hospital/register-public-key",
            body);

        var response = await _http.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"HUB KEY REGISTRATION FAILED {response.StatusCode} : {responseBody}");
        }
    }


    public async Task RegisterTransferKeyAsync(RegisterTransferKeyRequest dto)
    {
        var body = JsonSerializer.Serialize(dto);

        using var request = CreateSignedRequest(
            HttpMethod.Post,
            "/api/rest/seih/hospital/register-transfer-key",
            body);

        var response = await _http.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"HUB TRANSFER KEY REGISTRATION FAILED {response.StatusCode} : {responseBody}");
        }
    }


    public async Task<Stream> DownloadAsync(string transferId)
    {
        var apiKey = _config["SEIH:ApiKey"];

        if (_http.DefaultRequestHeaders.Contains("X-API-KEY"))
            _http.DefaultRequestHeaders.Remove("X-API-KEY");

        _http.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

        var response = await _http.PostAsJsonAsync(
            "/api/rest/transfers/download",
            new { TransferId = transferId }
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("DOWNLOAD STATUS: {status}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("DOWNLOAD ERROR: {body}", responseBody);
            throw new Exception($"Download failed: {response.StatusCode}");
        }

        return await response.Content.ReadAsStreamAsync();
    }

}


