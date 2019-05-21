using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddBuildingUnitAddressMatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingUnitAddressMatchLatestItemSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    BuildingUnitId = table.Column<Guid>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false),
                    BuildingUnitOsloId = table.Column<string>(nullable: true),
                    BuildingId = table.Column<Guid>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingUnitAddressMatchLatestItemSyndication", x => new { x.BuildingUnitId, x.AddressId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_BuildingId",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnitAddressMatchLatestItemSyndication_AddressId_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "BuildingUnitAddressMatchLatestItemSyndication",
                columns: new[] { "AddressId", "IsComplete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingUnitAddressMatchLatestItemSyndication",
                schema: "AddressRegistrySyndication");
        }
    }
}
