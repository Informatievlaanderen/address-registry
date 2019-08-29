using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class ColumnIndexSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "CI_AddressSyndication_Position",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "Position")
                .Annotation("SqlServer:ColumnStoreIndex", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "CI_AddressSyndication_Position",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");
        }
    }
}
