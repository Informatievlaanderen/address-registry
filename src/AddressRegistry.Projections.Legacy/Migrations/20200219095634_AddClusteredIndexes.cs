using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddClusteredIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds");

            migrationBuilder.DropIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "HouseNumberId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds");

            migrationBuilder.DropIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions");

            migrationBuilder.DropIndex(
                name: "IX_AddressList_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressList");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "HouseNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                column: "PersistentLocalId");
        }
    }
}
