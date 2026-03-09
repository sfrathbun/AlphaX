using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using AlphaX.Models;

namespace AlphaX.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _db;
        private const string ENDPOINT_COLLECTION = "endpoint";
        private const string SCANS_COLLECTION = "scans";
        private const string ORGANIZATIONS_COLLECTION = "organizations";

        public FirebaseService(FirestoreDb firestoreDb)
        {
            _db = firestoreDb;
        }

        // Agent Operations
        public async Task<Models.Endpoint> RegisterAgentAsync(string organizationId, Models.Endpoint endpoint)
        {
            endpoint.OrganizationId = organizationId;
            var docRef = _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(ENDPOINT_COLLECTION)
                .Document(endpoint.EndpointId);

            await docRef.SetAsync(endpoint);
            return endpoint;
        }

        public async Task<Models.Endpoint> GetAgentAsync(string organizationId, string endpoint)
        {
            var doc = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(ENDPOINT_COLLECTION)
                .Document(endpoint)
                .GetSnapshotAsync();

            return doc.Exists ? doc.ConvertTo<Models.Endpoint>() : null;
        }

        public async Task<List<Models.Endpoint>> GetEndpointsByOrganizationAsync(string organizationId)
        {
            var query = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(ENDPOINT_COLLECTION)
                .GetSnapshotAsync();

            var endpoints = new List<Models.Endpoint>();
            foreach (var doc in query.Documents)
            {
                endpoints.Add(doc.ConvertTo<Models.Endpoint>());
            }
            return endpoints;
        }

        public async Task UpdateAgentHeartbeatAsync(string organizationId, string agentId)
        {
            await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(ENDPOINT_COLLECTION)
                .Document(agentId)
                .UpdateAsync(new Dictionary<string, object>
                {
                    { "LastHeartbeat", DateTime.UtcNow },
                    { "Status", "Active" }
                });
        }

        // Scan Operations
        public async Task<ScanResult> SaveScanResultAsync(ScanResult scanResult)
        {
            var docRef = _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(scanResult.OrganizationId)
                .Collection(SCANS_COLLECTION)
                .Document(scanResult.ScanId);

            await docRef.SetAsync(scanResult);
            return scanResult;
        }

        public async Task<ScanResult> GetScanResultAsync(string organizationId, string scanId)
        {
            var doc = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(SCANS_COLLECTION)
                .Document(scanId)
                .GetSnapshotAsync();

            return doc.Exists ? doc.ConvertTo<ScanResult>() : null;
        }

        public async Task<List<ScanResult>> GetEndpointScanHistoryAsync(string organizationId, string agentId, int limit = 10)
        {
            var query = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(SCANS_COLLECTION)
                .WhereEqualTo("AgentId", agentId)
                .OrderByDescending("ScanEndTime")
                .Limit(limit)
                .GetSnapshotAsync();

            var scans = new List<ScanResult>();
            foreach (var doc in query.Documents)
            {
                scans.Add(doc.ConvertTo<ScanResult>());
            }
            return scans;
        }

        public async Task<List<ScanResult>> GetLatestScansAsync(string organizationId, int limit = 20)
        {
            var query = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(SCANS_COLLECTION)
                .OrderByDescending("ScanEndTime")
                .Limit(limit)
                .GetSnapshotAsync();

            var scans = new List<ScanResult>();
            foreach (var doc in query.Documents)
            {
                scans.Add(doc.ConvertTo<ScanResult>());
            }
            return scans;
        }
    }
}