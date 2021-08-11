using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddSoftDeleteToExtract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.AddColumn<bool>(
                name: "IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsParcelRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBuildingUnitComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBuildingUnitRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete_IsParcelRemoved_IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                columns: new[] { "AddressComplete", "IsParcelRemoved", "IsAddressLinkRemoved" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "ParcelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete_IsAddressLinkRemoved_IsBuildingUnitComplete_IsBuildingUnitRemoved_IsBuilding~",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                columns: new[] { "AddressComplete", "IsAddressLinkRemoved", "IsBuildingUnitComplete", "IsBuildingUnitRemoved", "IsBuildingComplete" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "BuildingUnitId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressParcelLinksExtract_AddressComplete_IsParcelRemoved_IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.DropIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressComplete_IsAddressLinkRemoved_IsBuildingUnitComplete_IsBuildingUnitRemoved_IsBuilding~",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.DropColumn(
                name: "IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.DropColumn(
                name: "IsParcelRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.DropColumn(
                name: "IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.DropColumn(
                name: "IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.DropColumn(
                name: "IsBuildingUnitComplete",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

            migrationBuilder.DropColumn(
                name: "IsBuildingUnitRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract");

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
    }
}
