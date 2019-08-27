using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class DesiredState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DesiredState",
                schema: "AddressRegistryLegacy",
                table: "ProjectionStates",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DesiredStateChangedAt",
                schema: "AddressRegistryLegacy",
                table: "ProjectionStates",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesiredState",
                schema: "AddressRegistryLegacy",
                table: "ProjectionStates");

            migrationBuilder.DropColumn(
                name: "DesiredStateChangedAt",
                schema: "AddressRegistryLegacy",
                table: "ProjectionStates");
        }
    }
}
