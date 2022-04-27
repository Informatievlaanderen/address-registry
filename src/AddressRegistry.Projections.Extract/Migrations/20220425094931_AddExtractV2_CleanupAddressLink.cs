using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class AddExtractV2_CleanupAddressLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressLinks",
                schema: "AddressRegistryExtract");

            migrationBuilder.CreateTable(
                name: "AddressV2",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressV2", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressV2_Complete",
                schema: "AddressRegistryExtract",
                table: "AddressV2",
                column: "Complete");

            migrationBuilder.CreateIndex(
                name: "IX_AddressV2_StreetNamePersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "AddressV2",
                column: "StreetNamePersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressV2",
                schema: "AddressRegistryExtract");

            migrationBuilder.CreateTable(
                name: "AddressLinks",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
