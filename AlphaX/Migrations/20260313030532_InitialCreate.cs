using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaX.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Endpoints",
                columns: table => new
                {
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    Hostname = table.Column<string>(type: "text", nullable: false),
                    OperatingSystem = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    AgentVersion = table.Column<string>(type: "text", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastHeartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endpoints", x => x.EndpointId);
                });

            migrationBuilder.CreateTable(
                name: "ScanResults",
                columns: table => new
                {
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    ScanStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScanEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScanStatus = table.Column<string>(type: "text", nullable: false),
                    TotalChecks = table.Column<int>(type: "integer", nullable: false),
                    PassedChecks = table.Column<int>(type: "integer", nullable: false),
                    FailedChecks = table.Column<int>(type: "integer", nullable: false),
                    WarningChecks = table.Column<int>(type: "integer", nullable: false),
                    OverallStatus = table.Column<string>(type: "text", nullable: false),
                    ComplianceScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanResults", x => x.ScanId);
                    table.ForeignKey(
                        name: "FK_ScanResults_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "EndpointId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceFindings",
                columns: table => new
                {
                    FindingId = table.Column<string>(type: "text", nullable: false),
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    RuleId = table.Column<string>(type: "text", nullable: false),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Remediation = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    FoundDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceFindings", x => x.FindingId);
                    table.ForeignKey(
                        name: "FK_ComplianceFindings_ScanResults_ScanId",
                        column: x => x.ScanId,
                        principalTable: "ScanResults",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EndpointInfos",
                columns: table => new
                {
                    EndpointInfoId = table.Column<string>(type: "text", nullable: false),
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    Hostname = table.Column<string>(type: "text", nullable: false),
                    OperatingSystem = table.Column<string>(type: "text", nullable: false),
                    OSVersion = table.Column<string>(type: "text", nullable: false),
                    OSBuildNumber = table.Column<string>(type: "text", nullable: false),
                    ProcessorInfo = table.Column<string>(type: "text", nullable: false),
                    TotalMemory = table.Column<long>(type: "bigint", nullable: false),
                    LastBootTime = table.Column<string>(type: "text", nullable: false),
                    ScanResultScanId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointInfos", x => x.EndpointInfoId);
                    table.ForeignKey(
                        name: "FK_EndpointInfos_ScanResults_ScanId",
                        column: x => x.ScanId,
                        principalTable: "ScanResults",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EndpointInfos_ScanResults_ScanResultScanId",
                        column: x => x.ScanResultScanId,
                        principalTable: "ScanResults",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstalledSoftware",
                columns: table => new
                {
                    SoftwareId = table.Column<string>(type: "text", nullable: false),
                    EndpointInfoId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    InstallDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstalledSoftware", x => x.SoftwareId);
                    table.ForeignKey(
                        name: "FK_InstalledSoftware_EndpointInfos_EndpointInfoId",
                        column: x => x.EndpointInfoId,
                        principalTable: "EndpointInfos",
                        principalColumn: "EndpointInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkInterfaces",
                columns: table => new
                {
                    NetworkInterfaceId = table.Column<string>(type: "text", nullable: false),
                    EndpointInfoId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    IpAddresses = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkInterfaces", x => x.NetworkInterfaceId);
                    table.ForeignKey(
                        name: "FK_NetworkInterfaces_EndpointInfos_EndpointInfoId",
                        column: x => x.EndpointInfoId,
                        principalTable: "EndpointInfos",
                        principalColumn: "EndpointInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceFindings_ScanId",
                table: "ComplianceFindings",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_EndpointInfos_ScanId",
                table: "EndpointInfos",
                column: "ScanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EndpointInfos_ScanResultScanId",
                table: "EndpointInfos",
                column: "ScanResultScanId");

            migrationBuilder.CreateIndex(
                name: "IX_InstalledSoftware_EndpointInfoId",
                table: "InstalledSoftware",
                column: "EndpointInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkInterfaces_EndpointInfoId",
                table: "NetworkInterfaces",
                column: "EndpointInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanResults_EndpointId",
                table: "ScanResults",
                column: "EndpointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceFindings");

            migrationBuilder.DropTable(
                name: "InstalledSoftware");

            migrationBuilder.DropTable(
                name: "NetworkInterfaces");

            migrationBuilder.DropTable(
                name: "EndpointInfos");

            migrationBuilder.DropTable(
                name: "ScanResults");

            migrationBuilder.DropTable(
                name: "Endpoints");
        }
    }
}
