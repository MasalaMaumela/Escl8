using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCL8.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackendProductionReadyPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResponderId",
                table: "LocationPings",
                newName: "AmbulanceId");

            migrationBuilder.RenameColumn(
                name: "AssignedResponderId",
                table: "Incidents",
                newName: "AssignedResponderUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedAmbulanceId",
                table: "Incidents",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAmbulanceId",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "AmbulanceId",
                table: "LocationPings",
                newName: "ResponderId");

            migrationBuilder.RenameColumn(
                name: "AssignedResponderUserId",
                table: "Incidents",
                newName: "AssignedResponderId");
        }
    }
}
