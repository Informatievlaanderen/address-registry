using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddNullableUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressList_PersistentLocalId_1",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId_1",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressList_PersistentLocalId_1",
                schema: "AddressRegistryLegacy",
                table: "AddressList");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_PersistentLocalId_1",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");
        }
    }
}
