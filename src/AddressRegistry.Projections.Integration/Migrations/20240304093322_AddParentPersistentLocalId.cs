using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    public partial class AddParentPersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_persistent_local_id",
                schema: "integration_address",
                table: "address_versions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_parent_persistent_local_id",
                schema: "integration_address",
                table: "address_versions",
                column: "parent_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_parent_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items",
                column: "parent_persistent_local_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_address_versions_parent_persistent_local_id",
                schema: "integration_address",
                table: "address_versions");

            migrationBuilder.DropIndex(
                name: "IX_address_latest_items_parent_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items");

            migrationBuilder.DropColumn(
                name: "parent_persistent_local_id",
                schema: "integration_address",
                table: "address_versions");

            migrationBuilder.DropColumn(
                name: "parent_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items");
        }
    }
}
