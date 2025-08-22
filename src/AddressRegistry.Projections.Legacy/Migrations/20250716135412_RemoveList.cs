using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class RemoveList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('AddressRegistryLegacy.vw_AddressListV2', 'V') IS NOT NULL
    DROP VIEW [AddressRegistryLegacy].[vw_AddressListV2]
");

            migrationBuilder.Sql(@"
IF OBJECT_ID('AddressRegistryLegacy.vw_AddressListCountV2', 'V') IS NOT NULL
    DROP VIEW [AddressRegistryLegacy].[vw_AddressListCountV2]
");

            migrationBuilder.DropTable(
                name: "AddressListV2",
                schema: "AddressRegistryLegacy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressListV2",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressListV2", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

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
    }
}
