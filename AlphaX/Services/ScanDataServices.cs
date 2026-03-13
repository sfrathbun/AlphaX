using AlphaX.Models;
using AlphaX.DTOs;
using AlphaX.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlphaX.Services
{
    public class ScanDataService
    {
        private readonly AlphaXContext _dbContext;
        private readonly ILogger<ScanDataService> _logger;

        public ScanDataService(AlphaXContext dbContext, ILogger<ScanDataService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<string> StoreScanAsync(ScanDataDto scanData)
        {
            var scanId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;
            string endpointInfoId = null;

            try
            {
                // 1. Store ScanResult first (this is what EndpointInfo references)
                var scanResult = new ScanResult
                {
                    ScanId = scanId,
                    EndpointId = scanData.EndpointId,
                    OrganizationId = "",
                    ScanStartTime = scanData.ScanStartTime,
                    ScanEndTime = scanData.ScanEndTime,
                    ScanStatus = "Completed",
                    TotalChecks = scanData.Findings?.Count ?? 0,
                    PassedChecks = 0,
                    FailedChecks = 0,
                    WarningChecks = 0,
                    OverallStatus = "Pass",
                    ComplianceScore = 0.0
                };
                _dbContext.ScanResults.Add(scanResult);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("📝 ScanResult record created: {ScanId}", scanId);

                // 2. Store master scan record
                var scan = new Scan
                {
                    ScanId = scanId,
                    EndpointId = scanData.EndpointId,
                    ScanStartTime = scanData.ScanStartTime,
                    ScanEndTime = scanData.ScanEndTime,
                    SubmissionTime = timestamp,
                    FindingsCount = scanData.Findings?.Count ?? 0
                };
                _dbContext.Scans.Add(scan);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("📝 Master scan record created: {ScanId}", scanId);

                // 3. Store endpoint info and get the ID
                if (scanData.EndpointInfo != null)
                {
                    endpointInfoId = await StoreEndpointInfoAsync(scanId, scanData, timestamp);
                }

                // 4. Store findings
                if (scanData.Findings != null)
                {
                    await StoreComplianceFindingsAsync(scanId, scanData, timestamp);
                    await StoreSecurityFindingsAsync(scanId, scanData, timestamp);
                    await StoreMaintenanceFindingsAsync(scanId, scanData, timestamp);
                    await StoreServiceFindingsAsync(scanId, scanData, timestamp);
                }

                // 5. Store installed software
                if (scanData.EndpointInfo?.InstalledSoftware != null)
                {
                    await StoreInstalledSoftwareAsync(scanId, scanData, timestamp, endpointInfoId);
                }

                // 6. Store network interfaces
                if (scanData.EndpointInfo?.NetworkInterfaces != null)
                {
                    await StoreNetworkInterfacesAsync(scanId, scanData, timestamp, endpointInfoId);
                }

                _logger.LogInformation("✅ All scan data stored successfully for scan ID: {ScanId}", scanId);
                return scanId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing scan data");
                throw;
            }
        }

        private async Task<string> StoreEndpointInfoAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            try
            {
                dynamic info = scanData.EndpointInfo;
                var endpointInfoId = Guid.NewGuid().ToString();
                var endpointInfo = new EndpointInfo
                {
                    EndpointInfoId = endpointInfoId,
                    EndpointId = scanData.EndpointId,
                    ScanId = scanId,
                    Hostname = info.Hostname ?? "Unknown",
                    OperatingSystem = info.OperatingSystem ?? "Unknown",
                    OSVersion = info.OSVersion ?? "Unknown",
                    OSBuildNumber = info.OSBuildNumber ?? "Unknown",
                    ProcessorInfo = info.ProcessorInfo ?? "Unknown",
                    TotalMemory = info.TotalMemory ?? 0,
                    LastBootTime = info.LastBootTime ?? "Unknown",
                    SubmissionTime = timestamp
                };

                _dbContext.EndpointInfos.Add(endpointInfo);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("📝 Endpoint info stored for scan: {ScanId}", scanId);
                return endpointInfoId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing endpoint info");
                throw;
            }
        }

        private async Task StoreComplianceFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            try
            {
                int findingCount = 0;
                foreach (var finding in scanData.Findings)
                {
                    if (finding.Category == "Compliance")
                    {
                        var complianceFinding = new ComplianceFinding
                        {
                            FindingId = finding.FindingId,
                            EndpointId = scanData.EndpointId,
                            ScanId = scanId,
                            RuleId = finding.RuleId ?? "",
                            RuleName = finding.RuleName ?? "",
                            Category = finding.Category ?? "",
                            Status = finding.Status ?? "",
                            Severity = finding.Severity ?? "",
                            Description = finding.Description ?? "",
                            Remediation = finding.Remediation ?? "",
                            Details = finding.Details ?? "{}",
                            FoundDate = finding.FoundDate,
                            SubmissionTime = timestamp
                        };

                        _dbContext.ComplianceFindings.Add(complianceFinding);
                        findingCount++;
                    }
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Stored {Count} compliance findings", findingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing compliance findings");
                throw;
            }
        }

        private async Task StoreSecurityFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            try
            {
                int findingCount = 0;
                foreach (var finding in scanData.Findings)
                {
                    if (finding.RuleId?.Contains("COMPLIANCE-001") == true || finding.RuleId?.Contains("COMPLIANCE-002") == true)
                    {
                        var securityFinding = new SecurityFinding
                        {
                            FindingId = finding.FindingId,
                            EndpointId = scanData.EndpointId,
                            ScanId = scanId,
                            RuleName = finding.RuleName ?? "",
                            Status = finding.Status ?? "",
                            Severity = finding.Severity ?? "",
                            Details = finding.Details ?? "{}",
                            FoundDate = finding.FoundDate,
                            SubmissionTime = timestamp
                        };

                        _dbContext.SecurityFindings.Add(securityFinding);
                        findingCount++;
                    }
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Stored {Count} security findings", findingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing security findings");
                throw;
            }
        }

        private async Task StoreMaintenanceFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            try
            {
                int findingCount = 0;
                foreach (var finding in scanData.Findings)
                {
                    if (finding.Category == "Maintenance")
                    {
                        var maintenanceFinding = new MaintenanceFinding
                        {
                            FindingId = finding.FindingId,
                            EndpointId = scanData.EndpointId,
                            ScanId = scanId,
                            RuleName = finding.RuleName ?? "",
                            Status = finding.Status ?? "",
                            Details = finding.Details ?? "{}",
                            FoundDate = finding.FoundDate,
                            SubmissionTime = timestamp
                        };

                        _dbContext.MaintenanceFindings.Add(maintenanceFinding);
                        findingCount++;
                    }
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Stored {Count} maintenance findings", findingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing maintenance findings");
                throw;
            }
        }

        private async Task StoreServiceFindingsAsync(string scanId, ScanDataDto scanData, DateTime timestamp)
        {
            try
            {
                int findingCount = 0;
                foreach (var finding in scanData.Findings)
                {
                    if (finding.Category == "Services")
                    {
                        var serviceFinding = new ServiceFinding
                        {
                            FindingId = finding.FindingId,
                            EndpointId = scanData.EndpointId,
                            ScanId = scanId,
                            RuleId = finding.RuleId ?? "",
                            RuleName = finding.RuleName ?? "",
                            Category = finding.Category ?? "",
                            Status = finding.Status ?? "",
                            Severity = finding.Severity ?? "",
                            Description = finding.Description ?? "",
                            Remediation = finding.Remediation ?? "",
                            Details = finding.Details ?? "{}",
                            FoundDate = finding.FoundDate,
                            SubmissionTime = timestamp
                        };

                        _dbContext.ServiceFindings.Add(serviceFinding);
                        findingCount++;
                    }
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Stored {Count} service findings", findingCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing service findings");
                throw;
            }
        }

        private async Task StoreInstalledSoftwareAsync(string scanId, ScanDataDto scanData, DateTime timestamp, string endpointInfoId)
        {
            try
            {
                foreach (var software in scanData.EndpointInfo.InstalledSoftware)
                {
                    // Convert InstallDate safely
                    DateTime installDate = DateTime.UtcNow;
                    if (software.InstallDate != null)
                    {
                        if (DateTime.TryParse(software.InstallDate.ToString(), out DateTime parsedDate))
                        {
                            installDate = parsedDate.Kind == DateTimeKind.Unspecified
                                ? DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc)
                                : parsedDate.ToUniversalTime();
                        }
                    }

                    var installedSoftware = new InstalledSoftware
                    {
                        SoftwareId = Guid.NewGuid().ToString(),
                        InstalledSoftwareId = Guid.NewGuid().ToString(),
                        EndpointId = scanData.EndpointId,
                        ScanId = scanId,
                        EndpointInfoId = endpointInfoId,
                        Name = software.Name ?? "Unknown",
                        Version = software.Version ?? "Unknown",
                        InstallDate = installDate,
                        SubmissionTime = timestamp
                    };

                    _dbContext.InstalledSoftwares.Add(installedSoftware);
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Stored {Count} installed software records", (int)scanData.EndpointInfo?.InstalledSoftware?.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing installed software");
                throw;
            }
        }

        private async Task StoreNetworkInterfacesAsync(string scanId, ScanDataDto scanData, DateTime timestamp, string endpointInfoId)
        {
            try
            {
                foreach (var networkInterface in scanData.EndpointInfo.NetworkInterfaces)
                {
                    var netInterface = new NetworkInterface
                    {
                        NetworkInterfaceId = Guid.NewGuid().ToString(),
                        EndpointId = scanData.EndpointId,
                        ScanId = scanId,
                        EndpointInfoId = endpointInfoId,
                        Name = networkInterface.Name ?? "Unknown",
                        MacAddress = networkInterface.MacAddress ?? "Unknown",
                        IpAddresses = string.Join(",", networkInterface.IpAddresses ?? new List<string>()),
                        Status = networkInterface.Status ?? "Unknown",
                        SubmissionTime = timestamp
                    };

                    _dbContext.NetworkInterfaces.Add(netInterface);
                }
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Stored {Count} network interfaces", (int)scanData.EndpointInfo.NetworkInterfaces.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing network interfaces");
                throw;
            }
        }

        public async Task<ScanResult> GetScanAsync(string scanId)
        {
            try
            {
                var scan = await _dbContext.Scans
                    .FirstOrDefaultAsync(s => s.ScanId == scanId);

                if (scan == null)
                {
                    _logger.LogWarning("Scan not found: {ScanId}", scanId);
                    return null;
                }

                // Retrieve endpoint info
                var endpointInfo = await _dbContext.EndpointInfos
                    .FirstOrDefaultAsync(e => e.ScanId == scanId);

                // Retrieve all findings
                var complianceFindings = await _dbContext.ComplianceFindings
                    .Where(f => f.ScanId == scanId)
                    .ToListAsync();

                var securityFindings = await _dbContext.SecurityFindings
                    .Where(f => f.ScanId == scanId)
                    .ToListAsync();

                var maintenanceFindings = await _dbContext.MaintenanceFindings
                    .Where(f => f.ScanId == scanId)
                    .ToListAsync();

                var serviceFindings = await _dbContext.ServiceFindings
                    .Where(f => f.ScanId == scanId)
                    .ToListAsync();

                // Combine all findings into a single list
                var allFindings = new List<ComplianceFinding>();
                allFindings.AddRange(complianceFindings);
                allFindings.AddRange(securityFindings.Cast<ComplianceFinding>());
                allFindings.AddRange(maintenanceFindings.Cast<ComplianceFinding>());
                allFindings.AddRange(serviceFindings.Cast<ComplianceFinding>());

                var scanResult = new ScanResult
                {
                    ScanId = scanId,
                    EndpointId = scan.EndpointId,
                    OrganizationId = "",
                    ScanStartTime = scan.ScanStartTime,
                    ScanEndTime = scan.ScanEndTime,
                    ScanStatus = "Completed",
                    TotalChecks = 0,
                    PassedChecks = 0,
                    FailedChecks = 0,
                    WarningChecks = 0,
                    OverallStatus = "Pass",
                    ComplianceScore = 0.0,
                    EndpointInfo = endpointInfo,
                    Findings = allFindings
                };

                _logger.LogInformation("✅ Scan retrieved: {ScanId}", scanId);
                return scanResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scan: {ScanId}", scanId);
                throw;
            }
        }
    }
}