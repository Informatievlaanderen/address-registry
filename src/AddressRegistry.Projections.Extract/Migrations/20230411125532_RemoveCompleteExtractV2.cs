using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class RemoveCompleteExtractV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressV2_Complete",
                schema: "AddressRegistryExtract",
                table: "AddressV2");

            migrationBuilder.DropColumn(
                name: "Complete",
                schema: "AddressRegistryExtract",
                table: "AddressV2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Complete",
                schema: "AddressRegistryExtract",
                table: "AddressV2",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AddressV2_Complete",
                schema: "AddressRegistryExtract",
                table: "AddressV2",
                column: "Complete");
        }
    }
}
