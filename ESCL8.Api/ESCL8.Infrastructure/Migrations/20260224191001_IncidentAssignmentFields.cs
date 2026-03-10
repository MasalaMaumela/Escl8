using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESCL8.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncidentAssignmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivedUtc",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedResponderId",
                table: "Incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedUtc",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledUtc",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnRouteUtc",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedUtc",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivedUtc",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "AssignedResponderId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "AssignedUtc",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CancelledUtc",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "EnRouteUtc",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ResolvedUtc",
                table: "Incidents");
        }
    }
}
