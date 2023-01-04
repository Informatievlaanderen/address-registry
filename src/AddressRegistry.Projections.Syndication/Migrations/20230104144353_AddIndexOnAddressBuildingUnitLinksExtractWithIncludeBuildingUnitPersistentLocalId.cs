using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddIndexOnAddressBuildingUnitLinksExtractWithIncludeBuildingUnitPersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_IsAddressLinkRemoved_IsBuildingUnitComplete_IsBuildingUnitRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_IsAddressLinkRemoved_IsBuildingUnitComplete_IsBuildingUnitRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                columns: new[] { "IsAddressLinkRemoved", "IsBuildingUnitComplete", "IsBuildingUnitRemoved", "IsBuildingComplete" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "BuildingUnitId", "BuildingUnitPergit sistentLocalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_IsAddressLinkRemoved_IsBuildingUnitComplete_IsBuildingUnitRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_IsAddressLinkRemoved_IsBuildingUnitComplete_IsBuildingUnitRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                columns: new[] { "IsAddressLinkRemoved", "IsBuildingUnitComplete", "IsBuildingUnitRemoved", "IsBuildingComplete" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "BuildingUnitPersistentLocalId" });
        }
    }
}
