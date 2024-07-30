using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class RemoveV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW [AddressRegistryLegacy].[vw_AddressList]
DROP VIEW [AddressRegistryLegacy].[vw_AddressListCount]");
        
            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressDetailsV2",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressList",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "CrabIdToPersistentLocalIds",
                schema: "AddressRegistryLegacy");

            migrationBuilder.Sql($@"
drop table [AddressRegistryLegacy].[KadStreetNames]
drop table [AddressRegistryLegacy].[RRAddresses]
drop table [AddressRegistryLegacy].[RRStreetNames]

drop table [AddressRegistrySyndication].[__EFMigrationsHistorySyndication]
drop table [AddressRegistrySyndication].[AddressBuildingUnitLinksExtract]
drop table [AddressRegistrySyndication].[AddressLinksExtract_Addresses]
drop table [AddressRegistrySyndication].[AddressParcelLinksExtract]
drop table [AddressRegistrySyndication].[BuildingUnitAddressMatchLatestItemSyndication]
drop table [AddressRegistrySyndication].[MunicipalityBosa]
drop table [AddressRegistrySyndication].[MunicipalityLatestSyndication]
drop table [AddressRegistrySyndication].[MunicipalitySyndication]
drop table [AddressRegistrySyndication].[ParcelAddressMatchLatestItemSyndication]
drop table [AddressRegistrySyndication].[PostalInfoPostalNamesLatestSyndication]
drop table [AddressRegistrySyndication].[PostalInfoLatestSyndication]
drop table [AddressRegistrySyndication].[ProjectionStates]
drop table [AddressRegistrySyndication].[StreetNameBosa]
drop table [AddressRegistrySyndication].[StreetNameLatestSyndication]
drop table [AddressRegistrySyndication].[StreetNameSyndication]
drop schema [AddressRegistrySyndication]

delete from [AddressRegistryLegacy].[ProjectionStates]
  where Name in (
  'AddressRegistry.Projections.Legacy.AddressDetail.AddressDetailProjections'
  ,'AddressRegistry.Projections.Legacy.AddressDetailV2.AddressDetailProjectionsV2'
  ,'AddressRegistry.Projections.Legacy.AddressList.AddressListProjections'
  ,'AddressRegistry.Projections.Legacy.AddressVersion.AddressVersionProjections'
  ,'AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId.CrabIdToPersistentLocalIdProjections'
  )

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "AddressRegistryLegacy",
                table: "ProjectionStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "AddressRegistryLegacy",
                table: "ProjectionStates",
                column: "Name")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<int>(type: "int", nullable: true),
                    PositionSpecification = table.Column<int>(type: "int", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressDetailsV2",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<int>(type: "int", nullable: false),
                    PositionSpecification = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetailsV2", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressList",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressList", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "CrabIdToPersistentLocalIds",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberId = table.Column<int>(type: "int", nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubaddressId = table.Column<int>(type: "int", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrabIdToPersistentLocalIds", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId_1",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_StreetNameId_Complete",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                columns: new[] { "StreetNameId", "Complete" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_VersionTimestamp",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "VersionTimestamp");

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
                name: "IX_AddressList_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_Complete_Removed",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                columns: new[] { "Complete", "Removed" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_Complete_Removed_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                columns: new[] { "Complete", "Removed", "PersistentLocalId" })
                .Annotation("SqlServer:Include", new[] { "StreetNameId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_PersistentLocalId_1",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_StreetNameId",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "StreetNameId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "HouseNumberId")
                .Annotation("SqlServer:Clustered", true);

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
    }
}
