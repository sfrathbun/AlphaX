using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using AlphaX.DTOs;
using AlphaX.Models;
using AlphaX.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlphaX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScansController : ControllerBase
    {
        private readonly ScanDataService _scanDataService;
        private readonly ILogger<ScansController> _logger;

        public ScansController(ScanDataService scanDataService, ILogger<ScansController> logger)
        {
            _scanDataService = scanDataService;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitScan([FromBody] ScanDataDto scanData)
        {
            try
            {
                _logger.LogInformation("Scan submission received from endpoint: {EndpointId}", scanData.EndpointId);

                var scanId = await _scanDataService.StoreScanAsync(scanData);

                _logger.LogInformation("✅ Scan stored successfully with ID: {ScanId}", scanId);

                return Ok(new { scanId = scanId, message = "Scan received successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing scan");
                return StatusCode(500, new { error = "Scan submission failed", details = ex.Message });
            }
        }

        [HttpGet("{scanId}")]
        public async Task<IActionResult> GetScan(string scanId)
        {
            try
            {
                var scan = await _scanDataService.GetScanAsync(scanId);
                if (scan == null)
                    return NotFound();

                return Ok(scan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scan");
                return StatusCode(500, new { error = "Retrieval failed", details = ex.Message });
            }
        }
    }
}