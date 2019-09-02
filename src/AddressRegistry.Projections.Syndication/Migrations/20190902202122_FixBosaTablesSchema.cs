using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class FixBosaTablesSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "StreetNameBosa",
                schema: "AddressRegistryLegacy",
                newName: "StreetNameBosa",
                newSchema: "AddressRegistrySyndication");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "StreetNameBosa",
                schema: "AddressRegistrySyndication",
                newName: "StreetNameBosa",
                newSchema: "AddressRegistryLegacy");
        }
    }
}
