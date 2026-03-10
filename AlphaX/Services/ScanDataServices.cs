using Google.Cloud.Firestore;
using AlphaX.Models;
using AlphaX.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlphaX.Services
{
    public class ScanDataService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<ScanDataService> _logger;

        public ScanDataService(FirestoreDb firestoreDb, ILogger<ScanDataService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task<string> StoreScanAsync(ScanDataDto scanData)
        {
            var scanId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;

            try
            {
                // 1. Store in scans table (master record)
                await StoreScanMasterAsync(scanId, scanData, timestamp);

                // 2. Store endpoint info
                await StoreEndpointInfoAsync(scanId, scanData, timestamp);

                // 3. Store findings by type
                await StoreComplianceFindingsAsync(scanId, scanData, timestamp);
                await StoreSecurityFindingsAsync(scanId, scanData, timestamp);
                await StoreMaintenanceFindingsAsync(scanId, scanData, timestamp);
                await StoreServiceFindingsAsync(scanId, scanData, timestamp);

                // 4. Store installed software
                await StoreInstalledSoftwareAsync(scanId, scanData, timestamp);

                // 5. Store network interfaces and update MACs
                await StoreNetworkInterfacesAsync(scanId, scanData, timestamp);

                _logger.LogInformation("✅ All scan data stored successfully for scan ID: {ScanId}", scanId);
                return scanId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing scan data");
                throw;
            }
        }

        private async Task StoreScanMasterAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            var scanDoc = new Dictionary<string, object>
            {
                { "ScanId", scanId },
                { "EndpointId", scanData.EndpointId },
                { "ScanStartTime", scanData.ScanStartTime },
                { "ScanEndTime", scanData.ScanEndTime },
                { "SubmissionTime", timestamp },
                { "FindingsCount", scanData.Findings?.Count ?? 0 }
            };

            await _firestoreDb.Collection("scans").Document(scanId).SetAsync(scanDoc);
            _logger.LogInformation("📝 Master scan record created: {ScanId}", scanId);
        }

        private async Task StoreEndpointInfoAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.EndpointInfo == null)
                return;

            dynamic info = scanData.EndpointInfo;
            var endpointInfoDict = new Dictionary<string, object>
            {
                { "EndpointId", scanData.EndpointId },
                { "ScanId", scanId },
                { "Hostname", info.Hostname ?? "Unknown" },
                { "OperatingSystem", info.OperatingSystem ?? "Unknown" },
                { "OSVersion", info.OSVersion ?? "Unknown" },
                { "OSBuildNumber", info.OSBuildNumber ?? "Unknown" },
                { "ProcessorInfo", info.ProcessorInfo ?? "Unknown" },
                { "TotalMemory", info.TotalMemory ?? 0 },
                { "LastBootTime", info.LastBootTime ?? "Unknown" },
                { "SubmissionTime", timestamp }
            };

            var docId = $"{scanData.EndpointId}_{scanId}";
            await _firestoreDb.Collection("endpoint_info").Document(docId).SetAsync(endpointInfoDict);
            _logger.LogInformation("✅ Endpoint info stored");
        }

        private async Task StoreInstalledSoftwareAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.EndpointInfo == null)
                return;

            dynamic info = scanData.EndpointInfo;
            if (info.InstalledSoftware == null)
                return;

            int softwareCount = 0;
            foreach (var software in info.InstalledSoftware)
            {
                try
                {
                    string name = software["Name"]?.ToString() ?? software.Name?.ToString() ?? "Unknown";
                    string version = software["Version"]?.ToString() ?? software.Version?.ToString() ?? "Unknown";
                    string installDate = software["InstallDate"]?.ToString() ?? software.InstallDate?.ToString() ?? "Unknown";

                    if (!string.IsNullOrEmpty(name) && name != "Unknown")
                    {
                        var softwareDict = new Dictionary<string, object>
                        {
                            { "EndpointId", scanData.EndpointId },
                            { "ScanId", scanId },
                            { "Name", name },
                            { "Version", version },
                            { "InstallDate", installDate },
                            { "SubmissionTime", timestamp }
                        };

                        var docId = $"{scanData.EndpointId}_{scanId}_{Guid.NewGuid()}";
                        await _firestoreDb.Collection("installed_software").Document(docId).SetAsync(softwareDict);
                        softwareCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing software item");
                }
            }
            _logger.LogInformation("✅ Stored {Count} software items", softwareCount);
        }

        private async Task StoreComplianceFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.Findings == null)
                return;

            int findingCount = 0;
            foreach (var finding in scanData.Findings)
            {
                if (finding.Category == "Security" || finding.Category == "Configuration")
                {
                    var findingDict = new Dictionary<string, object>
                    {
                        { "EndpointId", scanData.EndpointId },
                        { "ScanId", scanId },
                        { "FindingId", finding.FindingId },
                        { "RuleId", finding.RuleId ?? "" },
                        { "RuleName", finding.RuleName ?? "" },
                        { "Status", finding.Status ?? "" },
                        { "Severity", finding.Severity ?? "" },
                        { "Description", finding.Description ?? "" },
                        { "Remediation", finding.Remediation ?? "" },
                        { "Details", finding.Details ?? new Dictionary<string, object>() },
                        { "FoundDate", finding.FoundDate },
                        { "SubmissionTime", timestamp }
                    };

                    await _firestoreDb.Collection("compliance_findings").Document(finding.FindingId).SetAsync(findingDict);
                    findingCount++;
                }
            }
            _logger.LogInformation("✅ Stored {Count} compliance findings", findingCount);
        }

        private async Task StoreSecurityFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.Findings == null)
                return;

            int findingCount = 0;
            foreach (var finding in scanData.Findings)
            {
                if (finding.RuleId?.Contains("COMPLIANCE-001") == true || finding.RuleId?.Contains("COMPLIANCE-002") == true)
                {
                    var findingDict = new Dictionary<string, object>
                    {
                        { "EndpointId", scanData.EndpointId },
                        { "ScanId", scanId },
                        { "FindingId", finding.FindingId },
                        { "RuleName", finding.RuleName ?? "" },
                        { "Status", finding.Status ?? "" },
                        { "Severity", finding.Severity ?? "" },
                        { "Details", finding.Details ?? new Dictionary<string, object>() },
                        { "FoundDate", finding.FoundDate },
                        { "SubmissionTime", timestamp }
                    };

                    await _firestoreDb.Collection("security_findings").Document(finding.FindingId).SetAsync(findingDict);
                    findingCount++;
                }
            }
            _logger.LogInformation("✅ Stored {Count} security findings", findingCount);
        }

        private async Task StoreMaintenanceFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.Findings == null)
                return;

            int findingCount = 0;
            foreach (var finding in scanData.Findings)
            {
                if (finding.Category == "Maintenance")
                {
                    var findingDict = new Dictionary<string, object>
                    {
                        { "EndpointId", scanData.EndpointId },
                        { "ScanId", scanId },
                        { "FindingId", finding.FindingId },
                        { "RuleName", finding.RuleName ?? "" },
                        { "Status", finding.Status ?? "" },
                        { "Details", finding.Details ?? new Dictionary<string, object>() },
                        { "FoundDate", finding.FoundDate },
                        { "SubmissionTime", timestamp }
                    };

                    await _firestoreDb.Collection("maintenance_findings").Document(finding.FindingId).SetAsync(findingDict);
                    findingCount++;
                }
            }
            _logger.LogInformation("✅ Stored {Count} maintenance findings", findingCount);
        }

        private async Task StoreServiceFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.Findings == null)
                return;

            int findingCount = 0;
            foreach (var finding in scanData.Findings)
            {
                if (finding.Category == "Services")
                {
                    var findingDict = new Dictionary<string, object>
                    {
                        { "EndpointId", scanData.EndpointId },
                        { "ScanId", scanId },
                        { "FindingId", finding.FindingId },
                        { "RuleName", finding.RuleName ?? "" },
                        { "ServiceName", finding.Details?["ServiceName"]?.ToString() ?? "" },
                        { "Status", finding.Status ?? "" },
                        { "State", finding.Details?["State"]?.ToString() ?? "" },
                        { "StartMode", finding.Details?["StartMode"]?.ToString() ?? "" },
                        { "Severity", finding.Severity ?? "" },
                        { "FoundDate", finding.FoundDate },
                        { "SubmissionTime", timestamp }
                    };

                    await _firestoreDb.Collection("service_findings").Document(finding.FindingId).SetAsync(findingDict);
                    findingCount++;
                }
            }
            _logger.LogInformation("✅ Stored {Count} service findings", findingCount);
        }

        private async Task StoreNetworkInterfacesAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            if (scanData.EndpointInfo == null)
                return;

            dynamic info = scanData.EndpointInfo;
            if (info.NetworkInterfaces == null)
                return;

            foreach (var nic in info.NetworkInterfaces)
            {
                try
                {
                    string nicName = nic.Name?.ToString() ?? "Unknown";
                    string macAddress = nic.MacAddress?.ToString();

                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        // Convert IpAddresses array to comma-separated string
                        string ipAddresses = "";
                        if (nic.IpAddresses != null)
                        {
                            var ipList = new List<string>();
                            foreach (var ip in nic.IpAddresses)
                            {
                                ipList.Add(ip.ToString());
                            }
                            ipAddresses = string.Join(",", ipList);
                        }

                        var nicDict = new Dictionary<string, object>
                        {
                            { "EndpointId", scanData.EndpointId },
                            { "ScanId", scanId },
                            { "Name", nicName },
                            { "MacAddress", macAddress },
                            { "Status", nic.Status?.ToString() ?? "Unknown" },
                            { "IpAddresses", ipAddresses },
                            { "SubmissionTime", timestamp }
                        };

                        var docId = $"{scanData.EndpointId}_{scanId}_{macAddress}";
                        await _firestoreDb.Collection("network_interfaces").Document(docId).SetAsync(nicDict);

                        // Update MAC address LastSeen
                        await _firestoreDb.Collection("macs").Document(macAddress).SetAsync(
                            new Dictionary<string, object> { { "LastSeen", timestamp } },
                            SetOptions.MergeAll);

                        _logger.LogInformation("✅ Updated MAC: {MacAddress}", macAddress);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing network interface");
                }
            }
        }

        public async Task<Dictionary<string, object>> GetScanAsync(string scanId)
        {
            var doc = await _firestoreDb.Collection("scans").Document(scanId).GetSnapshotAsync();
            if (!doc.Exists)
                return null;

            return doc.ConvertTo<Dictionary<string, object>>();
        }
    }
}