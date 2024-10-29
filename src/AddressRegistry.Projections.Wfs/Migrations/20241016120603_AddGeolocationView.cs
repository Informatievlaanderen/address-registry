using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class AddGeolocationView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "geolocation");

            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::geolocation TO wms");
            migrationBuilder.Sql(@"
CREATE VIEW geolocation.AddressOsloGeolocationView WITH SCHEMABINDING AS
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
    CASE WHEN a.Removed = 1 THEN NULL ELSE CONCAT('<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>', REPLACE(REPLACE(Position.STAsText(), 'POINT (', ''), ')', ''), '</gml:pos></gml:Point>') END AS 'ADRESPOSITIE_GEOMETRIE_GML',
    CASE WHEN a.Removed = 1 THEN NULL ELSE LOWER(LEFT(a.[PositionMethod], 1)) + SUBSTRING(a.[PositionMethod], 2, LEN(a.[PositionMethod])) END AS 'ADRESPOSITIE_POSITIEGEOMETRIEMETHODE',
    CASE WHEN a.Removed = 1 THEN NULL ELSE LOWER(LEFT(a.[PositionSpecification], 1)) + SUBSTRING(a.[PositionSpecification], 2, LEN(a.[PositionSpecification])) END AS 'ADRESPOSITIE_POSITIESPECIFICATIE',
    CASE WHEN a.Removed = 1 THEN NULL ELSE LOWER(LEFT(a.[Status], 1)) + SUBSTRING(a.[Status], 2, LEN(a.[Status])) END AS 'ADRESSTATUS',
    CASE WHEN a.Removed = 1 THEN NULL ELSE a.[OfficiallyAssigned] END AS 'OFFICIEELTOEGEKEND',
    a.Removed AS 'REMOVED',
    a.AddressPersistentLocalId AS 'msgkey'
FROM [wfs.address].[AddressWfs] AS a
JOIN [wfs.streetname].[StreetNameHelperV2] s ON a.StreetNamePersistentLocalId = s.PersistentLocalId
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode]");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_AddressGeolocationView_ObjectId ON [geolocation].[AddressOsloGeolocationView] ([msgkey])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW geolocation.AddressOsloGeolocationView;");
        }
    }
}
