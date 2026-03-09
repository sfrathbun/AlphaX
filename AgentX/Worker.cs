using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgentX.Services;
using AgentX.Models;
using AgentX.DTOs;

namespace AgentX
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ApiClient _apiClient;
        private SystemInfoCollector _systemCollector;
        private ComplianceChecker _complianceChecker;
        private string _endpointId;
        private bool _isRegistered = false;
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

            // Try to find existing endpoint by MAC address
            if (!_isRegistered)
            {
                _logger.LogInformation("🔍 _isRegistered is false, checking for existing endpoint...");

                var systemInfo = _systemCollector.CollectSystemInfo();
                var macAddresses = ExtractMacAddresses(systemInfo);

                _logger.LogInformation("📋 Found {MacCount} MAC addresses", macAddresses.Count);

                if (macAddresses.Count > 0)
                {
                    _logger.LogInformation("Checking for existing endpoint with MAC addresses: {MacCount}", macAddresses.Count);

                    foreach (var mac in macAddresses)
                    {
                        _logger.LogInformation("🔎 Looking up MAC: {MacAddress}", mac);
                        var foundEndpointId = await _apiClient.LookupEndpointByMacAsync(mac);

                        if (!string.IsNullOrEmpty(foundEndpointId))
                        {
                            _endpointId = foundEndpointId;
                            _logger.LogInformation("✅ Found existing endpoint: {EndpointId} via MAC: {MacAddress}", _endpointId, mac);
                            _isRegistered = true;
                            break;  // Found it, no need to check other MACs
                        }
                        else
                        {
                            _logger.LogInformation("❌ MAC not found: {MacAddress}", mac);
                        }
                    }
                }

                // If not found by MAC, register as new endpoint
                if (!_isRegistered)
                {
                    _logger.LogInformation("No existing endpoint found, registering new endpoint...");

                    if (!await RegisterEndpoint(macAddresses))
                    {
                        _logger.LogError("Failed to register endpoint. Exiting.");
                        return;
                    }

                    _endpointId = _apiClient.GetEndpointId();

                    if (string.IsNullOrEmpty(_endpointId))
                    {
                        _logger.LogError("❌ EndpointId is null after registration! ApiClient failed to store it.");
                        return;
                    }

                    _logger.LogInformation("✅ Endpoint registered with ID: {EndpointId}", _endpointId);
                    _isRegistered = true;
                }
            }
            else
            {
                _logger.LogInformation("ℹ️ _isRegistered is already true, skipping registration/lookup");
            }

            // Run compliance scans hourly
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting compliance scan at: {time}", DateTimeOffset.Now);
                    await PerformComplianceScan();
                    _logger.LogInformation("Compliance scan completed at: {time}", DateTimeOffset.Now);

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

        private List<string> ExtractMacAddresses(dynamic systemInfo)
        {
            var macs = new List<string>();

            try
            {
                if (systemInfo.NetworkInterfaces != null)
                {
                    foreach (var nic in systemInfo.NetworkInterfaces)
                    {
                        if (!string.IsNullOrEmpty(nic.MacAddress))
                        {
                            macs.Add(nic.MacAddress);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting MAC addresses");
            }

            return macs;
        }

        private async Task<bool> RegisterEndpoint(List<string> macAddresses)
        {
            try
            {
                var systemInfo = _systemCollector.CollectSystemInfo();
                var ipAddress = systemInfo.NetworkInterfaces.Count > 0
                    ? (systemInfo.NetworkInterfaces[0].IpAddresses.Count > 0
                        ? systemInfo.NetworkInterfaces[0].IpAddresses[0]
                        : "0.0.0.0")
                    : "0.0.0.0";

                var registrationDto = new EndpointRegistrationDto
                {
                    AgentName = $"Agent-{Environment.MachineName}",
                    OperatingSystem = systemInfo.OperatingSystem,
                    Hostname = systemInfo.Hostname,
                    IpAddress = ipAddress,
                    AgentVersion = "1.0.0",
                    MacAddresses = macAddresses
                };

                return await _apiClient.RegisterEndpointAsync(registrationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering endpoint");
                return false;
            }
        }

        private async Task PerformComplianceScan()
        {
            try
            {
                var scanStartTime = DateTime.UtcNow;

                // Collect endpoint data
                var systemInfo = _systemCollector.CollectSystemInfo();

                // Run compliance checks
                var findings = _complianceChecker.RunComplianceChecks();

                var scanEndTime = DateTime.UtcNow;

                // Prepare scan data
                var scanData = new ScanDataDto
                {
                    EndpointId = _endpointId,
                    ScanStartTime = scanStartTime,
                    ScanEndTime = scanEndTime,
                    EndpointInfo = systemInfo,
                    Findings = findings
                };

                _logger.LogInformation("📤 Submitting scan with EndpointId: {EndpointId}", _endpointId);

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
                await _apiClient.SendHeartbeatAsync(_endpointId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing compliance scan");
            }
        }
    }
}