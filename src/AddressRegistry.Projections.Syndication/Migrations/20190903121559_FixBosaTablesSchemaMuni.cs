using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class FixBosaTablesSchemaMuni : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MunicipalityBosa",
                schema: "AddressRegistryLegacy",
                newName: "MunicipalityBosa",
                newSchema: "AddressRegistrySyndication");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryLegacy");

            migrationBuilder.RenameTable(
                name: "MunicipalityBosa",
                schema: "AddressRegistrySyndication",
                newName: "MunicipalityBosa",
                newSchema: "AddressRegistryLegacy");
        }
    }
}
