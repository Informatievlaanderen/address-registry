using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class AddIdAndCompleteIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Complete",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "Complete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address");

            migrationBuilder.DropIndex(
                name: "IX_Address_Complete",
                schema: "AddressRegistryExtract",
                table: "Address");
        }
    }
}
