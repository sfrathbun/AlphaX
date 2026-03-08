using System;
using System.Collections.Generic;
using ComplianceMonitoringAPI.Models;

namespace ComplianceMonitoringAPI.DTOs
{
    public class ScanDataDto
    {
        public string AgentId { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public EndpointInfo EndpointInfo { get; set; }
        public List<ComplianceFinding> Findings { get; set; }
    }

    public class AgentRegistrationDto
    {
        public string AgentName { get; set; }
        public string OperatingSystem { get; set; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string AgentVersion { get; set; }
    }

    public class ComplianceReportDto
    {
        public string ScanId { get; set; }
        public string AgentId { get; set; }
        public DateTime ScanDate { get; set; }
        public int TotalChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
        public int WarningChecks { get; set; }
        public double ComplianceScore { get; set; }
        public string OverallStatus { get; set; }
        public List<ComplianceFinding> Findings { get; set; }
    }
}