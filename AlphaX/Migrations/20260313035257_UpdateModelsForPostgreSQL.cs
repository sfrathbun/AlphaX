using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaX.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsForPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndpointId",
                table: "NetworkInterfaces",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScanId",
                table: "NetworkInterfaces",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionTime",
                table: "NetworkInterfaces",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EndpointId",
                table: "InstalledSoftware",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstalledSoftwareId",
                table: "InstalledSoftware",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScanId",
                table: "InstalledSoftware",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionTime",
                table: "InstalledSoftware",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EndpointId",
                table: "EndpointInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionTime",
                table: "EndpointInfos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EndpointId",
                table: "ComplianceFindings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionTime",
                table: "ComplianceFindings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "MaintenanceFindings",
                columns: table => new
                {
                    FindingId = table.Column<string>(type: "text", nullable: false),
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    FoundDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceFindings", x => x.FindingId);
                });

            migrationBuilder.CreateTable(
                name: "Scans",
                columns: table => new
                {
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    ScanStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScanEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FindingsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scans", x => x.ScanId);
                });

            migrationBuilder.CreateTable(
                name: "SecurityFindings",
                columns: table => new
                {
                    FindingId = table.Column<string>(type: "text", nullable: false),
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    FoundDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityFindings", x => x.FindingId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceFindings",
                columns: table => new
                {
                    FindingId = table.Column<string>(type: "text", nullable: false),
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    ScanId = table.Column<string>(type: "text", nullable: false),
                    RuleId = table.Column<string>(type: "text", nullable: false),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Remediation = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    FoundDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceFindings", x => x.FindingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceFindings");

            migrationBuilder.DropTable(
                name: "Scans");

            migrationBuilder.DropTable(
                name: "SecurityFindings");

            migrationBuilder.DropTable(
                name: "ServiceFindings");

            migrationBuilder.DropColumn(
                name: "EndpointId",
                table: "NetworkInterfaces");

            migrationBuilder.DropColumn(
                name: "ScanId",
                table: "NetworkInterfaces");

            migrationBuilder.DropColumn(
                name: "SubmissionTime",
                table: "NetworkInterfaces");

            migrationBuilder.DropColumn(
                name: "EndpointId",
                table: "InstalledSoftware");

            migrationBuilder.DropColumn(
                name: "InstalledSoftwareId",
                table: "InstalledSoftware");

            migrationBuilder.DropColumn(
                name: "ScanId",
                table: "InstalledSoftware");

            migrationBuilder.DropColumn(
                name: "SubmissionTime",
                table: "InstalledSoftware");

            migrationBuilder.DropColumn(
                name: "EndpointId",
                table: "EndpointInfos");

            migrationBuilder.DropColumn(
                name: "SubmissionTime",
                table: "EndpointInfos");

            migrationBuilder.DropColumn(
                name: "EndpointId",
                table: "ComplianceFindings");

            migrationBuilder.DropColumn(
                name: "SubmissionTime",
                table: "ComplianceFindings");
        }
    }
}
