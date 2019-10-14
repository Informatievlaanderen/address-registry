using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddStreetNameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressList_StreetNameId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "StreetNameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressList_StreetNameId",
                schema: "AddressRegistryLegacy",
                table: "AddressList");
        }
    }
}
