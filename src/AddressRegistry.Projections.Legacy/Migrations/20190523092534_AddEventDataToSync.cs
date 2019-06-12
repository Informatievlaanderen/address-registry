using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddEventDataToSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventDataAsXml",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDataAsXml",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");
        }
    }
}
