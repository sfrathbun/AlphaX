using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using ComplianceMonitoringAPI.Models;

namespace ComplianceMonitoringAPI.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _db;
        private const string AGENTS_COLLECTION = "agents";
        private const string SCANS_COLLECTION = "scans";
        private const string ORGANIZATIONS_COLLECTION = "organizations";

        public FirebaseService(FirestoreDb firestoreDb)
        {
            _db = firestoreDb;
        }

        // Agent Operations
        public async Task<Agent> RegisterAgentAsync(string organizationId, Agent agent)
        {
            agent.OrganizationId = organizationId;
            var docRef = _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(AGENTS_COLLECTION)
                .Document(agent.AgentId);

            await docRef.SetAsync(agent);
            return agent;
        }

        public async Task<Agent> GetAgentAsync(string organizationId, string agentId)
        {
            var doc = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(AGENTS_COLLECTION)
                .Document(agentId)
                .GetSnapshotAsync();

            return doc.Exists ? doc.ConvertTo<Agent>() : null;
        }

        public async Task<List<Agent>> GetAgentsByOrganizationAsync(string organizationId)
        {
            var query = await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(AGENTS_COLLECTION)
                .GetSnapshotAsync();

            var agents = new List<Agent>();
            foreach (var doc in query.Documents)
            {
                agents.Add(doc.ConvertTo<Agent>());
            }
            return agents;
        }

        public async Task UpdateAgentHeartbeatAsync(string organizationId, string agentId)
        {
            await _db.Collection(ORGANIZATIONS_COLLECTION)
                .Document(organizationId)
                .Collection(AGENTS_COLLECTION)
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

        public async Task<List<ScanResult>> GetAgentScanHistoryAsync(string organizationId, string agentId, int limit = 10)
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