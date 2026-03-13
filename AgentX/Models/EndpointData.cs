using System;
using System.Collections.Generic;

namespace AgentX.Models
{
    public class EndpointData
    {
        public string Hostname { get; set; }
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public string OSBuildNumber { get; set; }
        public List<NetworkInterface> NetworkInterfaces { get; set; } = new();
        public string ProcessorInfo { get; set; }
        public long TotalMemory { get; set; }
        public string LastBootTime { get; set; }
        public List<InstalledSoftware> InstalledSoftware { get; set; } = new();
    }

    public class NetworkInterface
    {
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public List<string> IpAddresses { get; set; } = new();
        public string Status { get; set; }
    }

    public class InstalledSoftware
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime InstallDate { get; set; }
    }

    public class ComplianceFinding
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Category { get; set; }
        public string Status { get; set; } // Pass, Fail, Warning
        public string Severity { get; set; } // Critical, High, Medium, Low
        public string Description { get; set; }
        public string Remediation { get; set; }
        public string Details { get; set; }
        public string ScanId { get; set; }  // Add this
        public string EndpointId { get; set; }  // Add this
    }

    public class ScanDataDto
    {
        public string EndpointId { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public EndpointData EndpointInfo { get; set; }
        public List<ComplianceFinding> Findings { get; set; } = new();
    }
}