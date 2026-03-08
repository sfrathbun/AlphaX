using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using AlphaX.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlphaX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<AgentsController> _logger;

        public AgentsController(FirestoreDb firestoreDb, ILogger<AgentsController> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAgent([FromBody] AgentRegistrationDto agentData)
        {
            try
            {
                _logger.LogInformation("Agent registration request from {Hostname}", agentData.Hostname);

                // Generate agent ID
                var agentId = Guid.NewGuid().ToString();

                // Create agent document in Firestore
                var agentDoc = new
                {
                    AgentId = agentId,
                    AgentName = agentData.AgentName,
                    OperatingSystem = agentData.OperatingSystem,
                    Hostname = agentData.Hostname,
                    IpAddress = agentData.IpAddress,
                    AgentVersion = agentData.AgentVersion,
                    RegistrationTime = DateTime.UtcNow,
                    LastHeartbeat = DateTime.UtcNow,
                    Status = "Active"
                };

                await _firestoreDb.Collection("agents").Document(agentId).SetAsync(agentDoc);

                _logger.LogInformation("Agent registered successfully with ID: {AgentId}", agentId);

                return Ok(new { agentId = agentId, message = "Agent registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent");
                return StatusCode(500, new { error = "Registration failed", details = ex.Message });
            }
        }

        [HttpPost("{agentId}/heartbeat")]
        public async Task<IActionResult> SendHeartbeat(string agentId)
        {
            try
            {
                _logger.LogInformation("Heartbeat received from agent: {AgentId}", agentId);

                var agentRef = _firestoreDb.Collection("agents").Document(agentId);
                await agentRef.UpdateAsync("LastHeartbeat", DateTime.UtcNow);

                return Ok(new { message = "Heartbeat received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing heartbeat");
                return StatusCode(500, new { error = "Heartbeat failed", details = ex.Message });
            }
        }

        [HttpGet("{agentId}")]
        public async Task<IActionResult> GetAgent(string agentId)
        {
            try
            {
                var doc = await _firestoreDb.Collection("agents").Document(agentId).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    return NotFound();
                }

                return Ok(doc.ConvertTo<dynamic>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent");
                return StatusCode(500, new { error = "Retrieval failed", details = ex.Message });
            }
        }
    }

    public class AgentRegistrationDto
    {
        public string AgentName { get; set; }
        public string OperatingSystem { get; set; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string AgentVersion { get; set; }
    }
}