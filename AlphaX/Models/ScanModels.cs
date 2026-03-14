using System;
using System.Collections.Generic;

namespace AlphaX.Models
{
    // Main scan record
    public class Scan
    {
        public string ScanId { get; set; }
        public string EndpointId { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int FindingsCount { get; set; }
    }

    // Main scan result model
    public class ScanResult
    {
        public string ScanId { get; set; } = Guid.NewGuid().ToString();
        public string EndpointId { get; set; }
        public string OrganizationId { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public string ScanStatus { get; set; } = "Completed"; // Completed, Failed, In Progress

        // Summary
        public int TotalChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
        public int WarningChecks { get; set; }

        // Overall Status
        public string OverallStatus { get; set; } // Pass, Fail, Warning
        public double ComplianceScore { get; set; } // 0-100

        // Navigation properties
        public EndpointInfo EndpointInfo { get; set; }
        public ICollection<ComplianceFinding> Findings { get; set; } = new List<ComplianceFinding>();
    }

    // Endpoint information captured during scan
    public class EndpointInfo
    {
        public string EndpointInfoId { get; set; } = Guid.NewGuid().ToString();
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string Hostname { get; set; }
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public string OSBuildNumber { get; set; }
        public string CurrentUser { get; set; }
        public string ProcessorInfo { get; set; }
        public long TotalMemory { get; set; }
        public string LastBootTime { get; set; }
        public DateTime SubmissionTime { get; set; }

        // Foreign key
        public string? ScanResultScanId { get; set; }  // Add ? to make nullable
        public ScanResult? ScanResult { get; set; }

        // Navigation properties
        public ICollection<NetworkInterface> NetworkInterfaces { get; set; } = new List<NetworkInterface>();
        public ICollection<InstalledSoftware> InstalledSoftware { get; set; } = new List<InstalledSoftware>();
    }

    // Network interface information
    public class NetworkInterface
    {
        public string NetworkInterfaceId { get; set; } = Guid.NewGuid().ToString();
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string EndpointInfoId { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string IpAddresses { get; set; } // CSV stored as string
        public string Status { get; set; }
        public DateTime SubmissionTime { get; set; }

        // Foreign key
        public EndpointInfo EndpointInfo { get; set; }
    }

    // Installed software information
    public class InstalledSoftware
    {
        public string InstalledSoftwareId { get; set; } = Guid.NewGuid().ToString();
        public string SoftwareId { get; set; }
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string EndpointInfoId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime InstallDate { get; set; }
        public DateTime SubmissionTime { get; set; }

        // Foreign key
        public EndpointInfo EndpointInfo { get; set; }
    }

    // Compliance finding/rule result
    public class ComplianceFinding
    {
        public string FindingId { get; set; } = Guid.NewGuid().ToString();
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Category { get; set; } // Compliance, Security, Maintenance, Services
        public string Status { get; set; } // Pass, Fail, Warning
        public string Severity { get; set; } // Critical, High, Medium, Low, Info
        public string Description { get; set; }
        public string Remediation { get; set; }
        public string Details { get; set; } // JSON stored as string (JSONB in PostgreSQL)
        public DateTime FoundDate { get; set; } = DateTime.UtcNow;
        public DateTime SubmissionTime { get; set; }

        // Foreign key
        public ScanResult? ScanResult { get; set; }
    }

    // Security finding
    public class SecurityFinding
    {
        public string FindingId { get; set; }
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string RuleName { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Details { get; set; }
        public DateTime FoundDate { get; set; }
        public DateTime SubmissionTime { get; set; }
    }

    // Maintenance finding
    public class MaintenanceFinding
    {
        public string FindingId { get; set; }
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string RuleName { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
        public DateTime FoundDate { get; set; }
        public DateTime SubmissionTime { get; set; }
    }

    // Service finding
    public class ServiceFinding
    {
        public string FindingId { get; set; }
        public string EndpointId { get; set; }
        public string ScanId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Remediation { get; set; }
        public string Details { get; set; }
        public DateTime FoundDate { get; set; }
        public DateTime SubmissionTime { get; set; }
    }

    public class MacAddress
    {
        public string MacAddressId { get; set; }
        public string Address { get; set; }
        public string EndpointId { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }

        // Navigation property
        public virtual Endpoint Endpoint { get; set; }
    }
}