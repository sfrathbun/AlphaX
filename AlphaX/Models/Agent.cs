using System;
using System.Collections.Generic;

namespace ComplianceMonitoringAPI.Models
{
    public class Agent
    {
        public string AgentId { get; set; } = Guid.NewGuid().ToString();
        public string OrganizationId { get; set; }
        public string AgentName { get; set; }
        public string OperatingSystem { get; set; } // Windows, Linux, macOS
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string AgentVersion { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active"; // Active, Inactive, Error
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}