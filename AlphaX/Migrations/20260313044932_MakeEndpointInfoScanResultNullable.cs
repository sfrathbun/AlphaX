using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaX.Migrations
{
    /// <inheritdoc />
    public partial class MakeEndpointInfoScanResultNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EndpointInfos_ScanResults_ScanResultScanId",
                table: "EndpointInfos");

            migrationBuilder.AlterColumn<string>(
                name: "ScanResultScanId",
                table: "EndpointInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_EndpointInfos_ScanResults_ScanResultScanId",
                table: "EndpointInfos",
                column: "ScanResultScanId",
                principalTable: "ScanResults",
                principalColumn: "ScanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EndpointInfos_ScanResults_ScanResultScanId",
                table: "EndpointInfos");

            migrationBuilder.AlterColumn<string>(
                name: "ScanResultScanId",
                table: "EndpointInfos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EndpointInfos_ScanResults_ScanResultScanId",
                table: "EndpointInfos",
                column: "ScanResultScanId",
                principalTable: "ScanResults",
                principalColumn: "ScanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
