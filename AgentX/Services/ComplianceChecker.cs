using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using AgentX.Models;

namespace AgentX.Services
{
    public class ComplianceChecker
    {
        public List<ComplianceFinding> RunComplianceChecks()
        {
            var findings = new List<ComplianceFinding>();

            findings.AddRange(CheckFirewall());
            findings.AddRange(CheckAntivirus());
            findings.AddRange(CheckWindowsUpdates());
            findings.AddRange(CheckServices());

            return findings;
        }

        private List<ComplianceFinding> CheckFirewall()
        {
            var findings = new List<ComplianceFinding>();

            try
            {
                using (var searcher = new ManagementObjectSearcher(@"\\.\root\SecurityCenter2",
                    "SELECT displayName, productState FROM AntivirusProduct WHERE displayName LIKE '%Firewall%'"))
                {
                    var results = searcher.Get();
                    if (results.Count > 0)
                    {
                        foreach (var obj in results)
                        {
                            var productState = obj["productState"]?.ToString() ?? "0";
                            var isEnabled = IsSecurityProductEnabled(productState);

                            findings.Add(new ComplianceFinding
                            {
                                RuleId = "COMPLIANCE-001",
                                RuleName = "Windows Firewall Enabled",
                                Category = "Security",
                                Status = isEnabled ? "Pass" : "Fail",
                                Severity = "High",
                                Description = "Windows Firewall must be enabled for endpoint protection",
                                Remediation = "Enable Windows Defender Firewall in Windows Security settings",
                                Details = new Dictionary<string, object>
                                {
                                    { "FirewallName", obj["displayName"]?.ToString() ?? "Unknown" },
                                    { "State", productState }
                                }
                            });
                        }
                    }
                    else
                    {
                        findings.Add(new ComplianceFinding
                        {
                            RuleId = "COMPLIANCE-001",
                            RuleName = "Windows Firewall Enabled",
                            Category = "Security",
                            Status = "Fail",
                            Severity = "High",
                            Description = "Windows Firewall must be enabled for endpoint protection",
                            Remediation = "Enable Windows Defender Firewall in Windows Security settings",
                            Details = new Dictionary<string, object> { { "Status", "No firewall detected" } }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new ComplianceFinding
                {
                    RuleId = "COMPLIANCE-001",
                    RuleName = "Windows Firewall Enabled",
                    Category = "Security",
                    Status = "Warning",
                    Severity = "Medium",
                    Description = "Could not verify firewall status",
                    Remediation = "Check firewall settings manually",
                    Details = new Dictionary<string, object> { { "Error", ex.Message } }
                });
            }

            return findings;
        }

        private List<ComplianceFinding> CheckAntivirus()
        {
            var findings = new List<ComplianceFinding>();

            try
            {
                using (var searcher = new ManagementObjectSearcher(@"\\.\root\SecurityCenter2",
                    "SELECT displayName, productState FROM AntivirusProduct"))
                {
                    var results = searcher.Get();
                    if (results.Count > 0)
                    {
                        foreach (var obj in results)
                        {
                            var productState = obj["productState"]?.ToString() ?? "0";
                            var isEnabled = IsSecurityProductEnabled(productState);

                            findings.Add(new ComplianceFinding
                            {
                                RuleId = "COMPLIANCE-002",
                                RuleName = "Antivirus Protection Active",
                                Category = "Security",
                                Status = isEnabled ? "Pass" : "Fail",
                                Severity = "Critical",
                                Description = "Antivirus software must be active and up-to-date",
                                Remediation = "Enable antivirus protection and ensure definitions are current",
                                Details = new Dictionary<string, object>
                                {
                                    { "AntivirusName", obj["displayName"]?.ToString() ?? "Unknown" },
                                    { "State", productState }
                                }
                            });
                        }
                    }
                    else
                    {
                        findings.Add(new ComplianceFinding
                        {
                            RuleId = "COMPLIANCE-002",
                            RuleName = "Antivirus Protection Active",
                            Category = "Security",
                            Status = "Fail",
                            Severity = "Critical",
                            Description = "Antivirus software must be active and up-to-date",
                            Remediation = "Install and enable antivirus protection",
                            Details = new Dictionary<string, object> { { "Status", "No antivirus detected" } }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new ComplianceFinding
                {
                    RuleId = "COMPLIANCE-002",
                    RuleName = "Antivirus Protection Active",
                    Category = "Security",
                    Status = "Warning",
                    Severity = "High",
                    Description = "Could not verify antivirus status",
                    Remediation = "Check antivirus settings manually",
                    Details = new Dictionary<string, object> { { "Error", ex.Message } }
                });
            }

            return findings;
        }

        private List<ComplianceFinding> CheckWindowsUpdates()
        {
            var findings = new List<ComplianceFinding>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT RebootRequired FROM Win32_QuickFixEngineering"))
                {
                    var results = searcher.Get();
                    var updatesPending = results.Count > 0;

                    findings.Add(new ComplianceFinding
                    {
                        RuleId = "COMPLIANCE-003",
                        RuleName = "Windows Updates Current",
                        Category = "Maintenance",
                        Status = !updatesPending ? "Pass" : "Warning",
                        Severity = updatesPending ? "High" : "Low",
                        Description = "System must have current Windows security updates installed",
                        Remediation = "Install pending Windows updates and restart if required",
                        Details = new Dictionary<string, object>
                        {
                            { "UpdatesInstalled", results.Count },
                            { "RebootPending", updatesPending }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                findings.Add(new ComplianceFinding
                {
                    RuleId = "COMPLIANCE-003",
                    RuleName = "Windows Updates Current",
                    Category = "Maintenance",
                    Status = "Warning",
                    Severity = "Medium",
                    Description = "Could not verify Windows update status",
                    Remediation = "Check Windows Update settings manually",
                    Details = new Dictionary<string, object> { { "Error", ex.Message } }
                });
            }

            return findings;
        }

        private List<ComplianceFinding> CheckServices()
        {
            var findings = new List<ComplianceFinding>();
            var criticalServices = new[] { "WinDefend", "MpsSvc", "wuauserv" };

            try
            {
                foreach (var serviceName in criticalServices)
                {
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT Name, State, StartMode FROM Win32_Service WHERE Name='{serviceName}'"))
                    {
                        var results = searcher.Get();
                        if (results.Count > 0)
                        {
                            foreach (var obj in results)
                            {
                                var state = obj["State"]?.ToString() ?? "Unknown";
                                var startMode = obj["StartMode"]?.ToString() ?? "Unknown";
                                var isRunning = state == "Running";
                                var isAutoStart = startMode == "Auto";

                                var ruleName = serviceName switch
                                {
                                    "WinDefend" => "Windows Defender Service Running",
                                    "MpsSvc" => "Windows Firewall Service Running",
                                    "wuauserv" => "Windows Update Service Running",
                                    _ => $"{serviceName} Service Running"
                                };

                                findings.Add(new ComplianceFinding
                                {
                                    RuleId = $"COMPLIANCE-{serviceName}",
                                    RuleName = ruleName,
                                    Category = "Services",
                                    Status = (isRunning && isAutoStart) ? "Pass" : "Fail",
                                    Severity = "High",
                                    Description = $"{ruleName} for system security and updates",
                                    Remediation = $"Ensure {serviceName} is running and set to Auto start",
                                    Details = new Dictionary<string, object>
                                    {
                                        { "ServiceName", serviceName },
                                        { "State", state },
                                        { "StartMode", startMode }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                findings.Add(new ComplianceFinding
                {
                    RuleId = "COMPLIANCE-SERVICES",
                    RuleName = "Critical Services Check",
                    Category = "Services",
                    Status = "Warning",
                    Severity = "Medium",
                    Description = "Could not verify all critical services",
                    Remediation = "Check services manually in Services.msc",
                    Details = new Dictionary<string, object> { { "Error", ex.Message } }
                });
            }

            return findings;
        }

        private bool IsSecurityProductEnabled(string productState)
        {
            // Product state is a hex value where bit pattern indicates enabled/disabled
            // We'll check if it's a valid enabled state
            if (int.TryParse(productState, out int state))
            {
                return state != 0 && state.ToString("X").Length > 2;
            }
            return false;
        }
    }
}