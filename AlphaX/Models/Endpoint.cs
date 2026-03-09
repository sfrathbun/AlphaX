using System;
using System.Collections.Generic;

namespace AlphaX.Models
{
    public class Endpoint
    {
        public string EndpointId { get; set; } = Guid.NewGuid().ToString();
        public string OrganizationId { get; set; }
        public string Hostname { get; set; }
        public string OperatingSystem { get; set; } // Windows, Linux, macOS
        public string IpAddress { get; set; }
        public string AgentVersion { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active"; // Active, Inactive, Error
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}