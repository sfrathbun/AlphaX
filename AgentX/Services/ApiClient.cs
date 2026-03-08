using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AgentX.Models;

namespace AgentX.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly string _organizationId;
        private string _agentId;

        public ApiClient(string apiBaseUrl, string organizationId)
        {
            _apiBaseUrl = apiBaseUrl;
            _organizationId = organizationId;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId);
        }

        public async Task<bool> RegisterAgentAsync(AgentRegistrationDto agentData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(agentData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/agents/register",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    _agentId = result.agentId;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendHeartbeatAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/agents/{_agentId}/heartbeat",
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
                scanData.AgentId = _agentId;
                var json = JsonConvert.SerializeObject(scanData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}/api/scans/submit",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scan submission error: {ex.Message}");
                return false;
            }
        }

        public string GetAgentId()
        {
            return _agentId;
        }

        public void SetAgentId(string agentId)
        {
            _agentId = agentId;
        }
    }

    public class AgentRegistrationDto
    {
        public string AgentName { get; set; }
        public string OperatingSystem { get; set; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string AgentVersion { get; set; }
    }
}