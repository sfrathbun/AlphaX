using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaX.Migrations
{
    /// <inheritdoc />
    public partial class AddMacAddressTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MacAddresses",
                columns: table => new
                {
                    MacAddressId = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    EndpointId = table.Column<string>(type: "text", nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MacAddresses", x => x.MacAddressId);
                    table.ForeignKey(
                        name: "FK_MacAddresses_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "EndpointId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MacAddresses_EndpointId",
                table: "MacAddresses",
                column: "EndpointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MacAddresses");
        }
    }
}
