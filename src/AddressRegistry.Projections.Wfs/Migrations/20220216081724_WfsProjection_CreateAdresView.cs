using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Wfs.Migrations
{
    public partial class WfsProjection_CreateAdresView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresView] WITH SCHEMABINDING AS
SELECT a.[PersistentLocalId] as 'ObjectId'
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[PersistentLocalId]) as 'Id'
      ,CONVERT(VARCHAR(33), a.[VersionTimestamp], 126) as 'VersieId'
      ,s.[straatnaamObjectId] as 'StraatnaamId'
      ,s.[straatnaamNL] as 'Straatnaam'
      ,s.[NisCode] as 'Nisgemeentecode'
      ,s.[gemeenteNaamNL] as 'Gemeentenaam'
      ,a.[PostalCode] as 'Postcode'
      ,CASE
        WHEN s.[straatnaamNL] IS NOT NULL AND a.[BoxNumber] IS NULL THEN CONCAT(s.[straatnaamNL], ' ',  a.[HouseNumber], ', ', a.[PostalCode], ' ', s.[gemeenteNaamNL])
        WHEN s.[straatnaamNL] IS NOT NULL AND a.[BoxNumber] IS NOT NULL THEN CONCAT(s.[straatnaamNL], ' ',  a.[HouseNumber], ' bus ', a.[BoxNumber] ,', ', a.[PostalCode], ' ', s.[gemeenteNaamNL])
        ELSE NULL
      END AS 'Adreslabel'
      ,a.[HouseNumber] as 'Huisnummer'
      ,a.[BoxNumber] as 'Busnummer'
      ,a.[Status] as 'Adresstatus'
      ,a.[Position] as 'Geometrie'
      ,a.[PositionMethod]  as 'Geometriemethode'
      ,a.[PositionSpecification] as 'Geometriespecificatie'
      ,a.[OfficiallyAssigned] as 'Officieeltoegekend'
FROM [wfs.address].[AddressDetails] as a
LEFT JOIN
    (
        SELECT s.[StreetNameId] as 'StraatnaamId'
            ,s.[PersistentLocalId] as 'StraatnaamObjectId'
            ,s.[NisCode]
            ,s.[NameDutch] as 'straatnaamNL'
            ,s.[Complete] as 'straatnaamCompleet'
            ,s.[Removed] as 'straatnaamVerwijderd'
            ,m.[MunicipalityId] as 'gemeenteId'
            ,m.[NameDutch] as 'gemeenteNaamNL'
            ,m.[OfficialLanguages]
            ,(SELECT Max(v) FROM (VALUES (s.[VersionTimestamp]), (m.[VersionTimestamp])) AS value(v)) as 'Version'
        FROM [wfs.streetname].[StreetNameHelper] as s
        INNER JOIN [wfs.municipality].[MunicipalityHelper] as m
        ON s.[NisCode] = m.[NisCode]
        WHERE s.[Removed] = 0 AND s.[Complete] = 1
    ) as s
  ON a.[StreetNameId] = s.[StraatnaamId]
WHERE a.[Removed] = 0 AND a.[Complete] = 1
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresView', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresView]
");
        }
    }
}
