using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "ParcelOsloId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                newName: "ParcelPersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "BuildingUnitOsloId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                newName: "BuildingUnitPersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                newName: "PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "ParcelPersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                newName: "ParcelOsloId");

            migrationBuilder.RenameColumn(
                name: "BuildingUnitPersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                newName: "BuildingUnitOsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                newName: "OsloId");
        }
    }
}
