using System.Collections.Generic;

namespace AlphaX.DTOs
{
    public class ComplianceReportDto
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Remediation { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }
}