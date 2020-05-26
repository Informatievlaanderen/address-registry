using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddParcelLinkAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressParcelLinksExtract",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    ParcelId = table.Column<Guid>(nullable: false),
                    AddressPersistentLocalId = table.Column<string>(nullable: true),
                    ParcelPersistentLocalId = table.Column<string>(nullable: true),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    AddressComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressParcelLinksExtract", x => new { x.AddressId, x.ParcelId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_AddressId",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_AddressPersistentLocalId",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                column: "AddressPersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_ParcelId",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                column: "ParcelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressParcelLinksExtract",
                schema: "AddressRegistrySyndication");
        }
    }
}
