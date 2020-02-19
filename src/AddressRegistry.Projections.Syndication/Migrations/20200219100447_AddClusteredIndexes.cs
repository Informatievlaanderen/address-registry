using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddClusteredIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StreetNameLatestSyndication",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_PostalInfoLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StreetNameLatestSyndication",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "StreetNameId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInfoLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StreetNameLatestSyndication",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_PostalInfoLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication");

            migrationBuilder.DropIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StreetNameLatestSyndication",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "StreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInfoLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                column: "AddressId");
        }
    }
}
