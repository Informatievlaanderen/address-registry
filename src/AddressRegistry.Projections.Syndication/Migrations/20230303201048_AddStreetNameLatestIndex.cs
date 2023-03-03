using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddStreetNameLatestIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NisCode_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                columns: new[] { "NisCode", "IsComplete", "IsRemoved" })
                .Annotation("SqlServer:Include", new[] { "NameDutchSearch", "NameEnglishSearch", "NameFrenchSearch", "NameGermanSearch" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_NisCode_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");
        }
    }
}
