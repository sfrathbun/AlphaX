using System;
using System.Collections.Generic;
using System.Linq;
using ComplianceMonitoringAPI.Models;

namespace ComplianceMonitoringAPI.Services
{
    public class ComplianceEngine
    {
        public ScanResult CalculateComplianceScore(ScanResult scanResult)
        {
            if (scanResult.Findings == null || scanResult.Findings.Count == 0)
            {
                scanResult.ComplianceScore = 100;
                scanResult.OverallStatus = "Pass";
                return scanResult;
            }

            var totalFindings = scanResult.Findings.Count;
            var passedFindings = scanResult.Findings.Count(f => f.Status == "Pass");
            var failedFindings = scanResult.Findings.Count(f => f.Status == "Fail");
            var warningFindings = scanResult.Findings.Count(f => f.Status == "Warning");

            scanResult.TotalChecks = totalFindings;
            scanResult.PassedChecks = passedFindings;
            scanResult.FailedChecks = failedFindings;
            scanResult.WarningChecks = warningFindings;

            // Calculate compliance score (0-100)
            // Failed items reduce score more than warnings
            var score = 100 - (failedFindings * 10) - (warningFindings * 5);
            scanResult.ComplianceScore = Math.Max(0, score);

            // Determine overall status
            if (failedFindings > 0)
                scanResult.OverallStatus = "Fail";
            else if (warningFindings > 0)
                scanResult.OverallStatus = "Warning";
            else
                scanResult.OverallStatus = "Pass";

            return scanResult;
        }

        public List<ComplianceFinding> GenerateDefaultRules()
        {
            return new List<ComplianceFinding>
            {
                new ComplianceFinding
                {
                    RuleId = "ENDPOINT-001",
                    RuleName = "OS Must Be Supported",
                    Category = "System",
                    Description = "Operating system must be a supported version"
                },
                new ComplianceFinding
                {
                    RuleId = "ENDPOINT-002",
                    RuleName = "Firewall Enabled",
                    Category = "Security",
                    Description = "System firewall must be enabled"
                },
                new ComplianceFinding
                {
                    RuleId = "ENDPOINT-003",
                    RuleName = "Antivirus Active",
                    Category = "Security",
                    Description = "Antivirus software must be active"
                },
                new ComplianceFinding
                {
                    RuleId = "ENDPOINT-004",
                    RuleName = "Automatic Updates",
                    Category = "Maintenance",
                    Description = "Automatic OS updates must be enabled"
                }
            };
        }
    }
}