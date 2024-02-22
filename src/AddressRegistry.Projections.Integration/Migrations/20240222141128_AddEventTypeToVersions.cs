using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    public partial class AddEventTypeToVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "integration_address",
                table: "address_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_type",
                schema: "integration_address",
                table: "address_versions",
                column: "type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_address_versions_type",
                schema: "integration_address",
                table: "address_versions");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "integration_address",
                table: "address_versions");
        }
    }
}
