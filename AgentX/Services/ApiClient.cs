using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AgentX.Models;
using AgentX.DTOs;

namespace AgentX.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly string _organizationId;
        private string _endpointId;

        public ApiClient(string apiBaseUrl, string organizationId)
        {
            _apiBaseUrl = apiBaseUrl;
            _organizationId = organizationId;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId);
        }

        public async Task<string> LookupEndpointByMacAsync(string macAddress)
        {
            try
            {
                var request = new { macAddress = macAddress };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/endpoints/lookup-by-mac",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    return result.endpointId;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MAC lookup error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RegisterEndpointAsync(EndpointRegistrationDto endpointData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(endpointData);
                Console.WriteLine($"📤 Sending registration to: {_apiBaseUrl}/api/endpoints/register");
                Console.WriteLine($"📤 Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/endpoints/register",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Response status: {response.StatusCode}");
                Console.WriteLine($"📥 Response body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    _endpointId = result.endpointId;  // ← Store it here
                    Console.WriteLine($"✅ EndpointId saved: {_endpointId}");
                    return true;
                }

                Console.WriteLine($"❌ Registration failed with status {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> SendHeartbeatAsync(string endpointId)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/endpoints/{endpointId}/heartbeat",
                    null);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Heartbeat error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SubmitScanAsync(ScanDataDto scanData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(scanData);
                Console.WriteLine($"📤 Sending scan data to: {_apiBaseUrl}/api/scans/submit");
                Console.WriteLine($"📤 Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/scans/submit",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Response status: {response.StatusCode}");
                Console.WriteLine($"📥 Response body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Scan submitted successfully");
                    return true;
                }

                Console.WriteLine($"❌ Scan submission failed with status {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Scan submission error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public string GetEndpointId()
        {
            return _endpointId;
        }

        public void SetEndpointId(string endpointId)
        {
            _endpointId = endpointId;
        }
    }
}