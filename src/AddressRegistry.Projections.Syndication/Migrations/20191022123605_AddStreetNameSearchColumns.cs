using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddStreetNameSearchColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameGermanSearch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropColumn(
                name: "NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropColumn(
                name: "NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropColumn(
                name: "NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropColumn(
                name: "NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");
        }
    }
}
