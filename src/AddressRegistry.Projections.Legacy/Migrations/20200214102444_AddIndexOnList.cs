using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddIndexOnList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressList_Complete_Removed_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                columns: new[] { "Complete", "Removed", "PersistentLocalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressList_Complete_Removed_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressList");
        }
    }
}
