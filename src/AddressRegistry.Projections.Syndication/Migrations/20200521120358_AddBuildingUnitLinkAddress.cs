using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddBuildingUnitLinkAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.AddColumn<bool>(
                name: "IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AddressBuildingUnitLinksExtract",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    BuildingId = table.Column<Guid>(nullable: false),
                    AddressPersistentLocalId = table.Column<string>(nullable: true),
                    BuildingUnitPersistentLocalId = table.Column<string>(nullable: true),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    AddressComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressBuildingUnitLinksExtract", x => new { x.AddressId, x.BuildingUnitId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressLinksExtract_Addresses",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: false),
                    BoxNumber = table.Column<string>(nullable: false),
                    PostalCode = table.Column<string>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressLinksExtract_Addresses", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "IsComplete", "IsRemoved", "IsBuildingComplete" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressId",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_AddressPersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "AddressPersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_BuildingId",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressBuildingUnitLinksExtract_BuildingUnitId",
                schema: "AddressRegistrySyndication",
                table: "AddressBuildingUnitLinksExtract",
                column: "BuildingUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressBuildingUnitLinksExtract",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "AddressLinksExtract_Addresses",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved_IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.DropColumn(
                name: "IsBuildingComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_IsComplete_IsRemoved",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "IsComplete", "IsRemoved" });
        }
    }
}
