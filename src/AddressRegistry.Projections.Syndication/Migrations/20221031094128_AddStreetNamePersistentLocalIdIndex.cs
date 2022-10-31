using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddStreetNamePersistentLocalIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_PersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_PersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.AlterColumn<string>(
                name: "PersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
