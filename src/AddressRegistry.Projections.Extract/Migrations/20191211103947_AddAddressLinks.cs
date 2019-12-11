using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class AddAddressLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressLinks",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressLinks", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressLinks_PersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "AddressLinks",
                column: "PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressLinks",
                schema: "AddressRegistryExtract");
        }
    }
}
