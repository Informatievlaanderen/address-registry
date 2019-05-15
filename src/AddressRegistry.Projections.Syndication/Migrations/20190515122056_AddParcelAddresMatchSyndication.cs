using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddParcelAddresMatchSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelAddressMatchLatestItemSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false),
                    ParcelOsloId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddressMatchLatestItemSyndication", x => new { x.ParcelId, x.AddressId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressMatchLatestItemSyndication_AddressId",
                schema: "AddressRegistrySyndication",
                table: "ParcelAddressMatchLatestItemSyndication",
                column: "AddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressMatchLatestItemSyndication",
                schema: "AddressRegistrySyndication");
        }
    }
}
