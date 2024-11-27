using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Extract.Migrations
{
    /// <inheritdoc />
    public partial class RemoveV1Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "AddressIdCrabHouseNumberId",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "AddressIdCrabSubaddressId",
                schema: "AddressRegistryExtract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Address",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressIdCrabHouseNumberId",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrabHouseNumberId = table.Column<int>(type: "int", nullable: true),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true)
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
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrabSubaddressId = table.Column<int>(type: "int", nullable: true),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressIdCrabSubaddressId", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_AddressPersistentLocalId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "AddressPersistentLocalId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Address_Complete",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "Complete");

            migrationBuilder.CreateIndex(
                name: "IX_Address_NisCode",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_Address_StreetNameId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "StreetNameId");
        }
    }
}
