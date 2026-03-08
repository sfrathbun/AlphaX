using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplianceMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScansController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<ScansController> _logger;

        public ScansController(FirestoreDb firestoreDb, ILogger<ScansController> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitScan([FromBody] ScanDataDto scanData)
        {
            try
            {
                _logger.LogInformation("Scan submission from agent: {AgentId}", scanData.AgentId);

                var scanId = Guid.NewGuid().ToString();

                var scanDoc = new
                {
                    ScanId = scanId,
                    AgentId = scanData.AgentId,
                    ScanStartTime = scanData.ScanStartTime,
                    ScanEndTime = scanData.ScanEndTime,
                    SubmissionTime = DateTime.UtcNow,
                    EndpointInfo = scanData.EndpointInfo,
                    FindingsCount = scanData.Findings?.Count ?? 0,
                    Findings = scanData.Findings
                };

                await _firestoreDb.Collection("scans").Document(scanId).SetAsync(scanDoc);

                _logger.LogInformation("Scan {ScanId} submitted successfully", scanId);

                return Ok(new { scanId = scanId, message = "Scan submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting scan");
                return StatusCode(500, new { error = "Scan submission failed", details = ex.Message });
            }
        }

        [HttpGet("{scanId}")]
        public async Task<IActionResult> GetScan(string scanId)
        {
            try
            {
                var doc = await _firestoreDb.Collection("scans").Document(scanId).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    return NotFound();
                }

                return Ok(doc.ConvertTo<dynamic>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scan");
                return StatusCode(500, new { error = "Retrieval failed", details = ex.Message });
            }
        }
    }

    public class ScanDataDto
    {
        public string AgentId { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public object EndpointInfo { get; set; }
        public List<object> Findings { get; set; }
    }
}