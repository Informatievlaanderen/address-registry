using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class BosaVersionIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosa",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa",
                column: "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosa");

            migrationBuilder.DropIndex(
                name: "IX_MunicipalityBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa");
        }
    }
}
