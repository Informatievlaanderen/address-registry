using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddAddressDetailItemV2WithParent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressDetailsV2WithParent",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    ParentAddressPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<int>(type: "int", nullable: false),
                    PositionSpecification = table.Column<int>(type: "int", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetailsV2WithParent", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_Removed",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_StreetNamePersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetailsV2WithParent_VersionTimestamp",
                schema: "AddressRegistryLegacy",
                table: "AddressDetailsV2WithParent",
                column: "VersionTimestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetailsV2WithParent",
                schema: "AddressRegistryLegacy");
        }
    }
}
