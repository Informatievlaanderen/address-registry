using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class AddClusteredIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressLinks_PersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "AddressLinks");

            migrationBuilder.DropIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_AddressLinks_PersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "AddressLinks",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "AddressPersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressLinks_PersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "AddressLinks");

            migrationBuilder.DropIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_AddressLinks_PersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "AddressLinks",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "AddressPersistentLocalId");
        }
    }
}
