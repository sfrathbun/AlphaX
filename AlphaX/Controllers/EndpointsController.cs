using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using AlphaX.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlphaX.Controllers
{
    [ApiController]
    [Route("api/endpoints")]
    public class EndpointController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<EndpointController> _logger;

        public EndpointController(FirestoreDb firestoreDb, ILogger<EndpointController> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterEndpoint([FromBody] EndpointRegistrationDto endpointData)
        {
            try
            {
                _logger.LogInformation("Endpoint registration request from {Hostname}", endpointData.Hostname);

                var endpointId = Guid.NewGuid().ToString();

                // Create endpoint document
                var endpointDoc = new
                {
                    EndpointId = endpointId,
                    AgentName = endpointData.AgentName,
                    OperatingSystem = endpointData.OperatingSystem,
                    Hostname = endpointData.Hostname,
                    IpAddress = endpointData.IpAddress,
                    AgentVersion = endpointData.AgentVersion,
                    RegistrationTime = DateTime.UtcNow,
                    LastHeartbeat = DateTime.UtcNow,
                    Status = "Active"
                };

                await _firestoreDb.Collection("endpoints").Document(endpointId).SetAsync(endpointDoc);

                // Store MAC addresses
                if (endpointData.MacAddresses != null && endpointData.MacAddresses.Count > 0)
                {
                    foreach (var mac in endpointData.MacAddresses)
                    {
                        var macDoc = new
                        {
                            MacAddress = mac,
                            EndpointId = endpointId,
                            FirstSeen = DateTime.UtcNow,
                            LastSeen = DateTime.UtcNow
                        };

                        await _firestoreDb.Collection("macs").Document(mac).SetAsync(macDoc);
                    }
                }

                _logger.LogInformation("Endpoint registered with ID: {EndpointId}, MACs: {MacCount}", endpointId, endpointData.MacAddresses?.Count ?? 0);

                return Ok(new { endpointId = endpointId, message = "Endpoint registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering endpoint");
                return StatusCode(500, new { error = "Registration failed", details = ex.Message });
            }
        }

        [HttpPost("lookup-by-mac")]
        public async Task<IActionResult> LookupEndpointByMac([FromBody] MacLookupDto request)
        {
            try
            {
                _logger.LogInformation("MAC lookup request for: {MacAddress}", request.MacAddress);

                var doc = await _firestoreDb.Collection("macs").Document(request.MacAddress).GetSnapshotAsync();

                if (!doc.Exists)
                {
                    return NotFound(new { message = "MAC address not found" });
                }

                var endpointId = doc.GetValue<string>("EndpointId");

                // Update LastSeen
                await _firestoreDb.Collection("macs").Document(request.MacAddress)
                    .UpdateAsync("LastSeen", DateTime.UtcNow);

                return Ok(new { endpointId = endpointId, message = "Endpoint found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up endpoint by MAC");
                return StatusCode(500, new { error = "Lookup failed", details = ex.Message });
            }
        }

        [HttpPost("{endpointId}/heartbeat")]
        public async Task<IActionResult> SendHeartbeat(string endpointId)
        {
            try
            {
                _logger.LogInformation("Heartbeat received from endpoint: {EndpointId}", endpointId);

                var endpointRef = _firestoreDb.Collection("endpoints").Document(endpointId);
                await endpointRef.UpdateAsync("LastHeartbeat", DateTime.UtcNow);

                return Ok(new { message = "Heartbeat received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing heartbeat");
                return StatusCode(500, new { error = "Heartbeat failed", details = ex.Message });
            }
        }

        [HttpGet("{endpointId}")]
        public async Task<IActionResult> GetEndpoint(string endpointId)
        {
            try
            {
                var doc = await _firestoreDb.Collection("endpoints").Document(endpointId).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    return NotFound();
                }

                return Ok(doc.ConvertTo<dynamic>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving endpoint");
                return StatusCode(500, new { error = "Retrieval failed", details = ex.Message });
            }
        }
    }

    public class MacLookupDto
    {
        public string MacAddress { get; set; }
    }
}