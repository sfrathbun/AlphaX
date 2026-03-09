using System;
using System.Collections.Generic;
using AlphaX.Models;
using Newtonsoft.Json.Linq;

namespace AlphaX.DTOs
{
    public class ScanDataDto
    {
        public string EndpointId { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public dynamic EndpointInfo { get; set; }
        public List<ComplianceFinding> Findings { get; set; }
    }
}