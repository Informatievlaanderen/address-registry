using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class RenamePlanToReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions");

            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                nullable: true);
        }
    }
}
