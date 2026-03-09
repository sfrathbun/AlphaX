using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using AlphaX.DTOs;
using AlphaX.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlphaX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScansController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<ScansController> _logger;

        public ScansController(FirestoreDb firestoreDb, ILogger<ScansController> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitScan([FromBody] ScanDataDto scanData)
        {
            try
            {
                _logger.LogInformation("Scan submission received from endpoint: {EndpointId}", scanData.EndpointId);

                var scanId = Guid.NewGuid().ToString();
                // Convert EndpointInfo to a Firestore-compatible format
                var endpointInfoDict = new Dictionary<string, object>();
                if (scanData.EndpointInfo != null)
                {
                    dynamic info = scanData.EndpointInfo;
                    endpointInfoDict["Hostname"] = info.Hostname ?? "Unknown";
                    endpointInfoDict["OperatingSystem"] = info.OperatingSystem ?? "Unknown";
                    endpointInfoDict["OSVersion"] = info.OSVersion ?? "Unknown";
                    endpointInfoDict["OSBuildNumber"] = info.OSBuildNumber ?? "Unknown";
                    endpointInfoDict["ProcessorInfo"] = info.ProcessorInfo ?? "Unknown";
                    endpointInfoDict["TotalMemory"] = info.TotalMemory ?? 0;
                    endpointInfoDict["LastBootTime"] = info.LastBootTime ?? "Unknown";

                    // Add installed software
                    if (info.InstalledSoftware != null)
                    {
                        var softwareList = new List<Dictionary<string, object>>();
                        int softwareCount = 0;

                        foreach (var software in info.InstalledSoftware)
                        {
                            try
                            {
                                // Extract values from dynamic object
                                string name = software["Name"]?.ToString() ?? software.Name?.ToString() ?? "Unknown";
                                string version = software["Version"]?.ToString() ?? software.Version?.ToString() ?? "Unknown";
                                string installDate = software["InstallDate"]?.ToString() ?? software.InstallDate?.ToString() ?? "Unknown";

                                if (!string.IsNullOrEmpty(name) && name != "Unknown")
                                {
                                    var softwareDict = new Dictionary<string, object>
                                    {
                                        { "Name", name },
                                        { "Version", version },
                                        { "InstallDate", installDate }
                                    };
                                    softwareList.Add(softwareDict);
                                    softwareCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing software: {ex.Message}");
                            }
                        }
                        endpointInfoDict["InstalledSoftware"] = softwareList;
                        Console.WriteLine($"Total software items added: {softwareCount}");
                    }
                    else
                    {
                        endpointInfoDict["InstalledSoftware"] = new List<object>();
                    }
                }

                var scanDoc = new Dictionary<string, object>
                {
                    { "ScanId", scanId },
                    { "EndpointId", scanData.EndpointId },
                    { "ScanStartTime", scanData.ScanStartTime },
                    { "ScanEndTime", scanData.ScanEndTime },
                    { "SubmissionTime", DateTime.UtcNow },
                    { "EndpointInfo", endpointInfoDict },
                    { "FindingsCount", scanData.Findings?.Count ?? 0 },
                    { "Findings", ConvertFindingsToList(scanData.Findings) }
                };

                _logger.LogInformation("📝 Storing scan to Firestore...");
                await _firestoreDb.Collection("scans").Document(scanId).SetAsync(scanDoc);
                _logger.LogInformation("✅ Scan stored successfully");

                // Update MAC addresses LastSeen timestamp
                try
                {
                    _logger.LogInformation("🔄 Updating MAC addresses...");
                    if (scanData.EndpointInfo != null)
                    {
                        dynamic endpointInfo = scanData.EndpointInfo;

                        if (endpointInfo.NetworkInterfaces != null)
                        {
                            foreach (var nic in endpointInfo.NetworkInterfaces)
                            {
                                string macAddress = nic.MacAddress?.ToString();

                                if (!string.IsNullOrEmpty(macAddress))
                                {
                                    _logger.LogInformation("Updating MAC address: {MacAddress}", macAddress);

                                    var macRef = _firestoreDb.Collection("macs").Document(macAddress);

                                    // Use SetAsync with merge to create or update
                                    await macRef.SetAsync(
                                        new Dictionary<string, object> { { "LastSeen", DateTime.UtcNow } },
                                        SetOptions.MergeAll);

                                    _logger.LogInformation("✅ Updated MAC: {MacAddress}", macAddress);
                                }
                            }
                        }
                    }
                    _logger.LogInformation("✅ MAC addresses updated");
                }
                catch (Exception macEx)
                {
                    _logger.LogWarning(macEx, "⚠️ Error updating MAC addresses (non-critical)");
                }

                _logger.LogInformation("✅ Scan stored successfully with ID: {ScanId}", scanId);

                return Ok(new { scanId = scanId, message = "Scan received successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing scan");
                return StatusCode(500, new { error = "Scan submission failed", details = ex.Message });
            }
        }

        [HttpGet("{scanId}")]
        public async Task<IActionResult> GetScan(string scanId)
        {
            try
            {
                var doc = await _firestoreDb.Collection("scans").Document(scanId).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    return NotFound();
                }

                return Ok(doc.ConvertTo<dynamic>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scan");
                return StatusCode(500, new { error = "Retrieval failed", details = ex.Message });
            }
        }

        private List<Dictionary<string, object>> ConvertFindingsToList(List<ComplianceFinding> findings)
        {
            var result = new List<Dictionary<string, object>>();

            if (findings != null)
            {
                foreach (var finding in findings)
                {
                    var findingDict = new Dictionary<string, object>
                    {
                        { "FindingId", finding.FindingId ?? "" },
                        { "RuleId", finding.RuleId ?? "" },
                        { "RuleName", finding.RuleName ?? "" },
                        { "Category", finding.Category ?? "" },
                        { "Status", finding.Status ?? "" },
                        { "Severity", finding.Severity ?? "" },
                        { "Description", finding.Description ?? "" },
                        { "Remediation", finding.Remediation ?? "" },
                        { "Details", finding.Details ?? new Dictionary<string, object>() },
                        { "FoundDate", finding.FoundDate }
                    };
                    result.Add(findingDict);
                }
            }

            return result;
        }
    }
}