using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrabIdToOsloIds",
                schema: "AddressRegistryLegacy");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressVersions_OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                newName: "IX_AddressVersions_PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressSyndication_OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                newName: "IX_AddressSyndication_PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                newName: "PersistentLocalId");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressDetails_OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                newName: "IX_AddressDetails_PersistentLocalId");

            migrationBuilder.CreateTable(
                name: "CrabIdToPersistentLocalIds",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    HouseNumberId = table.Column<int>(nullable: true),
                    SubaddressId = table.Column<int>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrabIdToPersistentLocalIds", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "HouseNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_IsRemoved",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "PersistentLocalId",
                unique: true,
                filter: "[PersistentLocalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_SubaddressId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "SubaddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrabIdToPersistentLocalIds",
                schema: "AddressRegistryLegacy");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                newName: "IX_AddressVersions_OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressSyndication_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                newName: "IX_AddressSyndication_OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                newName: "OsloId");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                newName: "IX_AddressDetails_OsloId");

            migrationBuilder.CreateTable(
                name: "CrabIdToOsloIds",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    BoxNumber = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    HouseNumberId = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    SubaddressId = table.Column<int>(nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrabIdToOsloIds", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "HouseNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_IsRemoved",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_OsloId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "OsloId",
                unique: true,
                filter: "[OsloId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_SubaddressId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "SubaddressId");
        }
    }
}
