using Microsoft.EntityFrameworkCore;
using AlphaX.Models;
using System;
using System.Net.Mail;

namespace AlphaX.Data
{
    public class AlphaXContext : DbContext
    {
        public AlphaXContext(DbContextOptions<AlphaXContext> options) : base(options)
        {
        }

        public DbSet<Models.Endpoint> Endpoints { get; set; }
        public DbSet<Scan> Scans { get; set; }
        public DbSet<ScanResult> ScanResults { get; set; }
        public DbSet<EndpointInfo> EndpointInfos { get; set; }
        public DbSet<NetworkInterface> NetworkInterfaces { get; set; }
        public DbSet<InstalledSoftware> InstalledSoftware { get; set; }
        public DbSet<ComplianceFinding> ComplianceFindings { get; set; }
        public DbSet<SecurityFinding> SecurityFindings { get; set; }
        public DbSet<MaintenanceFinding> MaintenanceFindings { get; set; }
        public DbSet<ServiceFinding> ServiceFindings { get; set; }
        public DbSet<InstalledSoftware> InstalledSoftwares { get; set; }
        public DbSet<MacAddress> MacAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Endpoint - ScanResult relationship
            modelBuilder.Entity<ScanResult>()
                .HasOne(s => s.EndpointInfo)
                .WithOne()
                .HasForeignKey<EndpointInfo>(e => e.ScanId);

            // ScanResult - ComplianceFinding relationship
            modelBuilder.Entity<ComplianceFinding>()
                .HasOne(f => f.ScanResult)
                .WithMany(s => s.Findings)
                .HasForeignKey(f => f.ScanId);

            // EndpointInfo - NetworkInterface relationship
            modelBuilder.Entity<NetworkInterface>()
                .HasOne(n => n.EndpointInfo)
                .WithMany(e => e.NetworkInterfaces)
                .HasForeignKey(n => n.EndpointInfoId);

            // EndpointInfo - InstalledSoftware relationship
            modelBuilder.Entity<InstalledSoftware>()
                .HasOne(s => s.EndpointInfo)
                .WithMany(e => e.InstalledSoftware)
                .HasForeignKey(s => s.EndpointInfoId);

            // Set primary keys
            modelBuilder.Entity<Models.Endpoint>().HasKey(e => e.EndpointId);
            modelBuilder.Entity<Scan>().HasKey(s => s.ScanId);
            modelBuilder.Entity<ScanResult>().HasKey(s => s.ScanId);
            modelBuilder.Entity<EndpointInfo>().HasKey(e => e.EndpointInfoId);
            modelBuilder.Entity<NetworkInterface>().HasKey(n => n.NetworkInterfaceId);
            modelBuilder.Entity<InstalledSoftware>().HasKey(s => s.SoftwareId);
            modelBuilder.Entity<ComplianceFinding>().HasKey(f => f.FindingId);
            modelBuilder.Entity<SecurityFinding>().HasKey(f => f.FindingId);
            modelBuilder.Entity<MaintenanceFinding>().HasKey(f => f.FindingId);
            modelBuilder.Entity<ServiceFinding>().HasKey(f => f.FindingId);

            // String primary keys are not auto-generated
            modelBuilder.Entity<Models.Endpoint>()
                .Property(e => e.EndpointId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Scan>()
                .Property(s => s.ScanId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ScanResult>()
                .Property(s => s.ScanId)
                .ValueGeneratedNever();

            modelBuilder.Entity<EndpointInfo>()
                .Property(e => e.EndpointInfoId)
                .ValueGeneratedNever();

            modelBuilder.Entity<NetworkInterface>()
                .Property(n => n.NetworkInterfaceId)
                .ValueGeneratedNever();

            modelBuilder.Entity<InstalledSoftware>()
                .Property(s => s.SoftwareId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ComplianceFinding>()
                .Property(f => f.FindingId)
                .ValueGeneratedNever();

            modelBuilder.Entity<SecurityFinding>()
                .Property(f => f.FindingId)
                .ValueGeneratedNever();

            modelBuilder.Entity<MaintenanceFinding>()
                .Property(f => f.FindingId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ServiceFinding>()
                .Property(f => f.FindingId)
                .ValueGeneratedNever();
        }
    }
}