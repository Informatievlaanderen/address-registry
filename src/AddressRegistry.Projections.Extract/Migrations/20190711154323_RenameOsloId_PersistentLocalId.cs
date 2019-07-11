using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressOsloId",
                schema: "AddressRegistryExtract",
                table: "Address",
                newName: "AddressPersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address",
                newName: "AddressOsloId");
        }
    }
}
