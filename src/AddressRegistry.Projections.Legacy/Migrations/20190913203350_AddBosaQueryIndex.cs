using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddBosaQueryIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_StreetNameId_Complete",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                columns: new[] { "StreetNameId", "Complete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_StreetNameId_Complete",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PersistentLocalId");
        }
    }
}
