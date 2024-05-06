using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class CombinedIndexIsRemovedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_removed_status",
                schema: "integration_address",
                table: "address_latest_items",
                columns: new[] { "removed", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_address_latest_items_removed_status",
                schema: "integration_address",
                table: "address_latest_items");
        }
    }
}
