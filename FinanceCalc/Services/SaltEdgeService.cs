using FinanceCalc.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FinanceCalc.Services
{
    public interface ISaltEdgeService
    {
        Task<string> BuildConnectUrlAsync(string customerId);
        Task<string> EnsureCustomerAsync(string userId);
    }

    public class SaltEdgeService : ISaltEdgeService
    {
        private readonly HttpClient _httpClient;
        private readonly SaltEdgeSettings _settings;

        public SaltEdgeService(IOptions<SaltEdgeSettings> options)
        {
            _settings = options.Value;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("App-id", _settings.AppId);
            _httpClient.DefaultRequestHeaders.Add("Secret", _settings.Secret);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> EnsureCustomerAsync(string userId)
        {
            var body = new
            {
                data = new { identifier = userId }
            };

            var response = await _httpClient.PostAsJsonAsync("https://www.saltedge.com/api/v5/customers", body);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("data").GetProperty("id").GetString();
            }

            if (json.Contains("Customer already exists"))
            {
                // Get existing
                var getResponse = await _httpClient.GetAsync($"https://www.saltedge.com/api/v5/customers/{userId}");
                var json2 = await getResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json2);
                return doc.RootElement.GetProperty("data").GetProperty("id").GetString();
            }

            throw new Exception("SaltEdge customer error: " + json);
        }

        public async Task<string> BuildConnectUrlAsync(string customerId)
        {
            var body = new
            {
                data = new
                {
                    customer_id = customerId,
                    consent = new
                    {
                        scopes = new[] { "account_details", "transactions_details" }
                    },
                    attempt = new { return_to = "https://localhost:5001/Bank/Callback" },
                    provider_codes = new[] { "dskbank_bg" }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://www.saltedge.com/api/v5/connect_sessions/create", body);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("data").GetProperty("connect_url").GetString();
        }
    }
}
