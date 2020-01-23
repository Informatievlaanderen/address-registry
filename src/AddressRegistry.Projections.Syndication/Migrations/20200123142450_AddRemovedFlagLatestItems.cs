using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddRemovedFlagLatestItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                columns: new[] { "IsComplete", "IsRemoved" });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "IsComplete", "IsRemoved" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetNameLatestSyndication_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "AddressId", "IsComplete" });
        }
    }
}
