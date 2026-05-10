using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Transfer
{
    public class TransferClientService
    {
        private readonly HttpClient _client;

        public TransferClientService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("TransferClient");
        }

        public async Task<string> StartTransferAsync(object payload)
        {
            var response = await _client.PostAsJsonAsync(
                "api/transfer/start",
                payload
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Transfer failed: {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    
    }
}