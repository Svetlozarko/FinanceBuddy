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
            try
            {
                // First, try to create a new customer
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

                // Parse the error response
                using var errorDoc = JsonDocument.Parse(json);
                var errorClass = errorDoc.RootElement.GetProperty("error").GetProperty("class").GetString();

                if (errorClass == "DuplicatedCustomer")
                {
                    // Customer already exists, get the list of customers and find by identifier
                    var getResponse = await _httpClient.GetAsync("https://www.saltedge.com/api/v5/customers");

                    if (!getResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to retrieve customers: {await getResponse.Content.ReadAsStringAsync()}");
                    }

                    var customersJson = await getResponse.Content.ReadAsStringAsync();
                    using var customersDoc = JsonDocument.Parse(customersJson);

                    var customers = customersDoc.RootElement.GetProperty("data");

                    foreach (var customer in customers.EnumerateArray())
                    {
                        var identifier = customer.GetProperty("identifier").GetString();
                        if (identifier == userId)
                        {
                            return customer.GetProperty("id").GetString();
                        }
                    }

                    throw new Exception($"Customer with identifier {userId} not found in customer list");
                }

                throw new Exception($"SaltEdge customer error: {json}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse SaltEdge response: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP request failed: {ex.Message}");
            }
        }

        public async Task<string> BuildConnectUrlAsync(string customerId)
        {
            try
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

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create connect session: {json}");
                }

                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("data").GetProperty("connect_url").GetString();
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse SaltEdge connect response: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"HTTP request failed while creating connect session: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}