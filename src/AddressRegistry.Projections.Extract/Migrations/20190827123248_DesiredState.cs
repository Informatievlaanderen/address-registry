using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class DesiredState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DesiredState",
                schema: "AddressRegistryExtract",
                table: "ProjectionStates",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DesiredStateChangedAt",
                schema: "AddressRegistryExtract",
                table: "ProjectionStates",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesiredState",
                schema: "AddressRegistryExtract",
                table: "ProjectionStates");

            migrationBuilder.DropColumn(
                name: "DesiredStateChangedAt",
                schema: "AddressRegistryExtract",
                table: "ProjectionStates");
        }
    }
}
