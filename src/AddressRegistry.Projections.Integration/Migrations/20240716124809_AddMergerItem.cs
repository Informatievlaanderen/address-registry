using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddMergerItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address_merger_items",
                schema: "integration_address",
                columns: table => new
                {
                    new_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    merged_persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address_merger_items", x => new { x.new_persistent_local_id, x.merged_persistent_local_id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_address_merger_items_merged_persistent_local_id",
                schema: "integration_address",
                table: "address_merger_items",
                column: "merged_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_merger_items_new_persistent_local_id",
                schema: "integration_address",
                table: "address_merger_items",
                column: "new_persistent_local_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_merger_items",
                schema: "integration_address");
        }
    }
}
