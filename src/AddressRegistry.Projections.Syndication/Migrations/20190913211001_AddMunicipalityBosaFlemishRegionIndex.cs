using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddMunicipalityBosaFlemishRegionIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_IsFlemishRegion",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa",
                column: "IsFlemishRegion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MunicipalityBosa_IsFlemishRegion",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa");
        }
    }
}
