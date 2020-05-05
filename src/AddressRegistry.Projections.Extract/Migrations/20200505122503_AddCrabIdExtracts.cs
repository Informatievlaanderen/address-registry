using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class AddCrabIdExtracts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressIdCrabHouseNumberId",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    CrabHouseNumberId = table.Column<int>(nullable: true),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressIdCrabHouseNumberId", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressIdCrabSubaddressId",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    CrabSubaddressId = table.Column<int>(nullable: true),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressIdCrabSubaddressId", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressIdCrabHouseNumberId",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "AddressIdCrabSubaddressId",
                schema: "AddressRegistryExtract");
        }
    }
}
