using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class RemakeIndexesExtractAddressLinksIncludes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                column: "IsRemoved")
                .Annotation("SqlServer:Include", new[] { "AddressId", "ParcelId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "IsComplete", "IsRemoved", "IsBuildingComplete" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "BuildingUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                column: "AddressComplete")
                .Annotation("SqlServer:Include", new[] { "AddressId", "ParcelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "AddressComplete")
                .Annotation("SqlServer:Include", new[] { "AddressId", "BuildingUnitId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                column: "IsRemoved")
                .Annotation("SqlServer:Include", new[] { "ParcelId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "IsComplete", "IsRemoved", "IsBuildingComplete" })
                .Annotation("SqlServer:Include", new[] { "BuildingUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                column: "AddressComplete")
                .Annotation("SqlServer:Include", new[] { "ParcelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "AddressComplete")
                .Annotation("SqlServer:Include", new[] { "BuildingUnitId" });
        }
    }
}
