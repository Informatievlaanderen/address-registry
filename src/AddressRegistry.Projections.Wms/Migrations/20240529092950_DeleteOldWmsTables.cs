using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    /// <inheritdoc />
    public partial class DeleteOldWmsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms].[AdresAfgekeurd]");
            migrationBuilder.Sql("DROP VIEW [wms].[AdresVoorgesteld]");
            migrationBuilder.Sql("DROP VIEW [wms].[AdresInGebruik]");
            migrationBuilder.Sql("DROP VIEW [wms].[AdresGehistoreerd]");
            migrationBuilder.Sql("DROP VIEW [wms].[AdresView]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresView] WITH SCHEMABINDING AS
SELECT  a.[AddressPersistentLocalId] as ObjectId
        ,CONCAT('https://data.vlaanderen.be/id/adres/', a.[AddressPersistentLocalId]) as Id
        ,a.[VersionAsString] as VersieId
        ,s.[PersistentLocalId] as StraatnaamObjectId
        ,s.[NameDutch] as Straatnaam
        ,a.[HouseNumberLabel] as HuisnummerLabel
        ,a.[LabelType]
        ,a.[HouseNumberLabelLength] as LabelLengte
        ,s.[NisCode] as GemeenteObjectId
        ,m.[NameDutch] as Gemeentenaam
        ,a.[PostalCode] as PostinfoObjectId
        ,[wms].[GetFullAddress](s.[NameDutch], a.[HouseNumber], a.[BoxNumber], a.[PostalCode], m.[NameDutch]) as VolledigAdres
        ,a.[HouseNumber] as Huisnummer
        ,a.[BoxNumber] as Busnummer
        ,a.[Status] as AdresStatus
        ,a.[Position] as AdresPositie
        ,a.[PositionMethod] as PositieGeometrieMethode
        ,a.[PositionSpecification] as PositieSpecificatie
        ,a.[OfficiallyAssigned] as OfficieelToegekend
FROM [wms.address].AddressWmsV2 as a
JOIN [wms.streetname].[StreetNameHelperV2] s ON a.[StreetNamePersistentLocalId] = s.[PersistentLocalId] AND a.[Removed] = 0
JOIN [wms.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_AdresView_ObjectId ON [wms].[AdresView] ([ObjectId])");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresVoorgesteld] WITH SCHEMABINDING AS
SELECT
     [ObjectId]
    ,[Id]
    ,[VersieId]
    ,[StraatnaamObjectId]
    ,[Straatnaam]
    ,[HuisnummerLabel]
    ,[LabelType]
    ,[LabelLengte]
    ,[GemeenteObjectId]
    ,[Gemeentenaam]
    ,[PostinfoObjectId]
    ,[VolledigAdres]
    ,[Huisnummer]
    ,[Busnummer]
    ,[AdresStatus]
    ,[AdresPositie]
    ,[PositieGeometrieMethode]
    ,[PositieSpecificatie]
    ,[OfficieelToegekend]
FROM [wms].[AdresView]
WHERE [AdresStatus] = 'Voorgesteld';");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresInGebruik] WITH SCHEMABINDING AS
SELECT
     [ObjectId]
    ,[Id]
    ,[VersieId]
    ,[StraatnaamObjectId]
    ,[Straatnaam]
    ,[HuisnummerLabel]
    ,[LabelType]
    ,[LabelLengte]
    ,[GemeenteObjectId]
    ,[Gemeentenaam]
    ,[PostinfoObjectId]
    ,[VolledigAdres]
    ,[Huisnummer]
    ,[Busnummer]
    ,[AdresStatus]
    ,[AdresPositie]
    ,[PositieGeometrieMethode]
    ,[PositieSpecificatie]
    ,[OfficieelToegekend]
FROM [wms].[AdresView]
WHERE [AdresStatus] = 'InGebruik'");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresGehistoreerd] WITH SCHEMABINDING AS
SELECT
     [ObjectId]
    ,[Id]
    ,[VersieId]
    ,[StraatnaamObjectId]
    ,[Straatnaam]
    ,[HuisnummerLabel]
    ,[LabelType]
    ,[LabelLengte]
    ,[GemeenteObjectId]
    ,[Gemeentenaam]
    ,[PostinfoObjectId]
    ,[VolledigAdres]
    ,[Huisnummer]
    ,[Busnummer]
    ,[AdresStatus]
    ,[AdresPositie]
    ,[PositieGeometrieMethode]
    ,[PositieSpecificatie]
    ,[OfficieelToegekend]
FROM [wms].[AdresView]
WHERE [AdresStatus] = 'Gehistoreerd'");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresAfgekeurd] WITH SCHEMABINDING AS
SELECT
     [ObjectId]
    ,[Id]
    ,[VersieId]
    ,[StraatnaamObjectId]
    ,[Straatnaam]
    ,[HuisnummerLabel]
    ,[LabelType]
    ,[LabelLengte]
    ,[GemeenteObjectId]
    ,[Gemeentenaam]
    ,[PostinfoObjectId]
    ,[VolledigAdres]
    ,[Huisnummer]
    ,[Busnummer]
    ,[AdresStatus]
    ,[AdresPositie]
    ,[PositieGeometrieMethode]
    ,[PositieSpecificatie]
    ,[OfficieelToegekend]
FROM [wms].[AdresView]
WHERE [AdresStatus] = 'Afgekeurd'");

            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "wms.address");

            migrationBuilder.DropTable(
                name: "AddressWms",
                schema: "wms.address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "wms.address",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberLabelLength = table.Column<int>(type: "int", nullable: true),
                    LabelType = table.Column<int>(type: "int", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionX = table.Column<double>(type: "float", nullable: true),
                    PositionY = table.Column<double>(type: "float", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressWms",
                schema: "wms.address",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HouseNumberLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberLabelLength = table.Column<int>(type: "int", nullable: true),
                    LabelType = table.Column<int>(type: "int", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressWms", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "wms.address",
                table: "AddressDetails",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PositionX_PositionY_Removed_Complete_Status",
                schema: "wms.address",
                table: "AddressDetails",
                columns: new[] { "PositionX", "PositionY", "Removed", "Complete", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNameId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Status",
                schema: "wms.address",
                table: "AddressDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_StreetNameId",
                schema: "wms.address",
                table: "AddressDetails",
                column: "StreetNameId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWms_PositionX_PositionY_Removed_Status",
                schema: "wms.address",
                table: "AddressWms",
                columns: new[] { "PositionX", "PositionY", "Removed", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWms_Status",
                schema: "wms.address",
                table: "AddressWms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWms_StreetNamePersistentLocalId",
                schema: "wms.address",
                table: "AddressWms",
                column: "StreetNamePersistentLocalId");
        }
    }
}
