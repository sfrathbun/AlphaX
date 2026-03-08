using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgentX.Services;
using AgentX.Models;

namespace AgentX
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ApiClient _apiClient;
        private SystemInfoCollector _systemCollector;
        private ComplianceChecker _complianceChecker;
        private string _agentId;
        private readonly string _apiBaseUrl = "https://localhost:7003";
        private readonly string _organizationId = "your-org-id";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _systemCollector = new SystemInfoCollector();
            _complianceChecker = new ComplianceChecker();
            _apiClient = new ApiClient(_apiBaseUrl, _organizationId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Compliance Agent starting at: {time}", DateTimeOffset.Now);

            // Register agent on startup
            if (!await RegisterAgent())
            {
                _logger.LogError("Failed to register agent. Exiting.");
                return;
            }

            _agentId = _apiClient.GetAgentId();
            _logger.LogInformation("Agent registered with ID: {AgentId}", _agentId);

            // Run compliance scans hourly
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting compliance scan at: {time}", DateTimeOffset.Now);
                    await PerformComplianceScan();
                    _logger.LogInformation("Compliance scan completed at: {time}", DateTimeOffset.Now);

                    // Wait 1 hour (3600000 ms) before next scan
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Agent service is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during compliance scan");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Compliance Agent stopped at: {time}", DateTimeOffset.Now);
        }

        private async Task<bool> RegisterAgent()
        {
            try
            {
                var endpointInfo = _systemCollector.CollectSystemInfo();
                var ipAddress = endpointInfo.NetworkInterfaces.Count > 0
                    ? (endpointInfo.NetworkInterfaces[0].IpAddresses.Count > 0
                        ? endpointInfo.NetworkInterfaces[0].IpAddresses[0]
                        : "0.0.0.0")
                    : "0.0.0.0";

                var registrationDto = new AgentRegistrationDto
                {
                    AgentName = $"Agent-{Environment.MachineName}",
                    OperatingSystem = endpointInfo.OperatingSystem,
                    Hostname = endpointInfo.Hostname,
                    IpAddress = ipAddress,
                    AgentVersion = "1.0.0"
                };

                return await _apiClient.RegisterAgentAsync(registrationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent");
                return false;
            }
        }

        private async Task PerformComplianceScan()
        {
            try
            {
                var scanStartTime = DateTime.UtcNow;

                // Collect endpoint data
                var endpointInfo = _systemCollector.CollectSystemInfo();

                // Run compliance checks
                var findings = _complianceChecker.RunComplianceChecks();

                var scanEndTime = DateTime.UtcNow;

                // Prepare scan data
                var scanData = new ScanDataDto
                {
                    ScanStartTime = scanStartTime,
                    ScanEndTime = scanEndTime,
                    EndpointInfo = endpointInfo,
                    Findings = findings
                };

                // Submit to API
                var success = await _apiClient.SubmitScanAsync(scanData);

                if (success)
                {
                    _logger.LogInformation("Scan submitted successfully");
                }
                else
                {
                    _logger.LogError("Failed to submit scan to API");
                }

                // Send heartbeat
                await _apiClient.SendHeartbeatAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing compliance scan");
            }
        }
    }
}
