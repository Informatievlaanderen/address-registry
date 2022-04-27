using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddV2Models : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StreetNamePersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AddressDetailsV2",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<int>(type: "int", nullable: true),
                    PositionSpecification = table.Column<int>(type: "int", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetailsV2", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressListV2",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressListV2", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_Removed",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_StreetNamePersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2_VersionTimestamp",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2",
                column: "VersionTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AddressListV2_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressListV2",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressListV2_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressListV2",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressListV2_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressListV2",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressListV2_Removed",
                schema: "AddressRegistryLegacy",
                table: "AddressListV2",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressListV2_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressListV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressListV2_StreetNamePersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressListV2",
                column: "StreetNamePersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetailsV2",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressListV2",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropColumn(
                name: "StreetNamePersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");
        }
    }
}
