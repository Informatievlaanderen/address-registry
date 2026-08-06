using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class AddWfsV3_Lambert2008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressWfsV3",
                schema: "wfs.address",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    ParentAddressPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HouseNumberLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberLabelLength = table.Column<int>(type: "int", nullable: false, computedColumnSql: "CAST(LEN(ISNULL(HouseNumberLabel, '')) AS INT)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressWfsV3", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_BoxNumber",
                schema: "wfs.address",
                table: "AddressWfsV3",
                column: "BoxNumber",
                filter: "[BoxNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_HouseNumber",
                schema: "wfs.address",
                table: "AddressWfsV3",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_PositionX_PositionY_Removed_Status",
                schema: "wfs.address",
                table: "AddressWfsV3",
                columns: new[] { "PositionX", "PositionY", "Removed", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_PostalCode",
                schema: "wfs.address",
                table: "AddressWfsV3",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_Removed",
                schema: "wfs.address",
                table: "AddressWfsV3",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_Removed_StreetNamePersistentLocalId",
                schema: "wfs.address",
                table: "AddressWfsV3",
                columns: new[] { "Removed", "StreetNamePersistentLocalId" })
                .Annotation("SqlServer:Include", new[] { "BoxNumber", "HouseNumber", "OfficiallyAssigned", "AddressPersistentLocalId", "Position", "PositionMethod", "PositionSpecification", "PostalCode", "Status", "VersionAsString" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_Status",
                schema: "wfs.address",
                table: "AddressWfsV3",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV3_StreetNamePersistentLocalId",
                schema: "wfs.address",
                table: "AddressWfsV3",
                column: "StreetNamePersistentLocalId");

            // The V2 bounding box (22279.17, 153050.23, 258873.3, 244022.31) expressed in Lambert 2008:
            // all four corners transformed, then the envelope padded out to the next 100 m.
            migrationBuilder.Sql(
                @"CREATE SPATIAL INDEX [SPATIAL_AddressV3_Position] ON [wfs.address].[AddressWfsV3] ([Position])
                USING GEOMETRY_GRID
	                WITH (
		                BOUNDING_BOX =(522200, 653000, 758900, 744100),
		                GRIDS =(
			                LEVEL_1 = MEDIUM,
			                LEVEL_2 = MEDIUM,
			                LEVEL_3 = MEDIUM,
			                LEVEL_4 = MEDIUM),
	                CELLS_PER_OBJECT = 5)
	                GO");

            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresViewV3', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresViewV3]
");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresViewV3] WITH SCHEMABINDING AS
SELECT a.[AddressPersistentLocalId] as ObjectId
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[AddressPersistentLocalId]) as 'Id'
      ,a.[VersionAsString] as 'VersieId'
      ,s.PersistentLocalId as 'StraatnaamObjectId'
      ,s.NameDutch as 'Straatnaam'
      ,s.[NisCode] as 'GemeenteObjectId'
      ,m.NameDutch as 'Gemeentenaam'
      ,a.[PostalCode] as 'PostinfoObjectId'
      ,CASE
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NULL THEN CONCAT(s.NameDutch, ' ',  a.[HouseNumber], ', ', a.[PostalCode], ' ', m.NameDutch)
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NOT NULL THEN CONCAT(s.NameDutch, ' ',  a.[HouseNumber], ' bus ', a.[BoxNumber] ,', ', a.[PostalCode], ' ', m.NameDutch)
        ELSE NULL
      END AS 'VolledigAdres'
      ,a.[HouseNumber] as 'Huisnummer'
      ,a.[BoxNumber] as 'Busnummer'
      ,a.[HouseNumberLabel] as 'HuisnummerLabel'
      ,a.[HouseNumberLabelLength] as 'HuisnummerLabelLengte'
      ,a.[LabelType] as 'LabelType'
      ,a.[Status] as 'AdresStatus'
      ,a.[Position] as 'AdresPositie'
      ,a.[PositionMethod]  as 'PositieGeometrieMethode'
      ,a.[PositionSpecification] as 'PositieSpecificatie'
      ,a.[OfficiallyAssigned] as 'OfficieelToegekend'
FROM [wfs.address].[AddressWfsV3] as a
JOIN [wfs.streetname].[StreetNameHelperV2] s ON a.StreetNamePersistentLocalId = s.PersistentLocalId AND a.[Removed] = 0
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0;
");

            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresViewV3_ObjectId ON [wfs].[AdresViewV3] ([ObjectId])");

            migrationBuilder.Sql(@"
IF OBJECT_ID('geolocation.AddressOsloGeolocationViewV3', 'V') IS NOT NULL
    DROP VIEW [geolocation].[AddressOsloGeolocationViewV3]
");

            // Identical to AddressOsloGeolocationView except for the source table and the srsName, which
            // has to name EPSG 3812 because the coordinates in this table are Lambert 2008.
            migrationBuilder.Sql(@"
CREATE VIEW geolocation.AddressOsloGeolocationViewV3 WITH SCHEMABINDING AS
SELECT
    CASE WHEN a.Removed = 1 THEN NULL ELSE CONCAT('https://data.vlaanderen.be/id/adres/', a.[AddressPersistentLocalId]) END AS 'IDENTIFICATOR_ID',
    CASE WHEN a.Removed = 1 THEN NULL ELSE 'https://data.vlaanderen.be/id/adres/' END AS 'IDENTIFICATOR_NAAMRUIMTE',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[AddressPersistentLocalId] END AS 'IDENTIFICATOR_OBJECTID',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[VersionAsString] END AS 'IDENTIFICATOR_VERSIEID',
    CASE WHEN a.Removed = 1 THEN NULL ELSE s.[NisCode] END AS 'GEMEENTE_OBJECTID',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[PostalCode] END AS 'POSTINFO_OBJECTID',
    CASE WHEN a.Removed = 1 THEN NULL ELSE s.PersistentLocalId END AS 'STRAATNAAM_OBJECTID',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[HouseNumber] END AS 'HUISNUMMER',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[BoxNumber] END AS 'BUSNUMMER',
    CASE
        WHEN a.Removed = 1 THEN NULL
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NULL THEN CONCAT(s.NameDutch, ' ', a.[HouseNumber], ', ', a.[PostalCode], ' ', m.NameDutch)
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NOT NULL THEN CONCAT(s.NameDutch, ' ', a.[HouseNumber], ' bus ', a.[BoxNumber], ', ', a.[PostalCode], ' ', m.NameDutch)
        ELSE NULL
    END AS 'VOLLEDIGADRES',
    CASE WHEN a.Removed = 1 THEN NULL ELSE 'Point' END AS 'ADRESPOSITIE_GEOMETRIE_TYPE',
    CASE WHEN a.Removed = 1 THEN NULL ELSE CONCAT('<gml:Point srsName=""http://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>', REPLACE(REPLACE(Position.STAsText(), 'POINT (', ''), ')', ''), '</gml:pos></gml:Point>') END AS 'ADRESPOSITIE_GEOMETRIE_GML',
    CASE WHEN a.Removed = 1 THEN NULL ELSE LOWER(LEFT(a.[PositionMethod], 1)) + SUBSTRING(a.[PositionMethod], 2, LEN(a.[PositionMethod])) END AS 'ADRESPOSITIE_POSITIEGEOMETRIEMETHODE',
    CASE WHEN a.Removed = 1 THEN NULL ELSE LOWER(LEFT(a.[PositionSpecification], 1)) + SUBSTRING(a.[PositionSpecification], 2, LEN(a.[PositionSpecification])) END AS 'ADRESPOSITIE_POSITIESPECIFICATIE',
    CASE WHEN a.Removed = 1 THEN NULL ELSE LOWER(LEFT(a.[Status], 1)) + SUBSTRING(a.[Status], 2, LEN(a.[Status])) END AS 'ADRESSTATUS',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[OfficiallyAssigned] END AS 'OFFICIEELTOEGEKEND',
    a.Removed AS 'REMOVED',
    a.AddressPersistentLocalId AS 'msgkey'
FROM [wfs.address].[AddressWfsV3] AS a
JOIN [wfs.streetname].[StreetNameHelperV2] s ON a.StreetNamePersistentLocalId = s.PersistentLocalId
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode]");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_AddressGeolocationViewV3_ObjectId ON [geolocation].[AddressOsloGeolocationViewV3] ([msgkey])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('geolocation.AddressOsloGeolocationViewV3', 'V') IS NOT NULL
    DROP VIEW [geolocation].[AddressOsloGeolocationViewV3]
");

            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresViewV3', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresViewV3]
");

            migrationBuilder.DropTable(
                name: "AddressWfsV3",
                schema: "wfs.address");
        }
    }
}
