using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddIndexOnAddressSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressLinksExtract_Addresses_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressLinksExtract_Addresses",
                columns: new[] { "IsComplete", "IsRemoved" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressLinksExtract_Addresses_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressLinksExtract_Addresses");
        }
    }
}
