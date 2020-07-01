using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddIndexBuildingUnitExtractAddressComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "AddressComplete")
                .Annotation("SqlServer:Include", new[] { "BuildingUnitId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");
        }
    }
}
