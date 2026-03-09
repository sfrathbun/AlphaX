using System.Collections.Generic;

namespace AlphaX.DTOs
{
    public class EndpointRegistrationDto
    {
        public string AgentName { get; set; }
        public string OperatingSystem { get; set; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string AgentVersion { get; set; }
        public List<string> MacAddresses { get; set; }
    }
}