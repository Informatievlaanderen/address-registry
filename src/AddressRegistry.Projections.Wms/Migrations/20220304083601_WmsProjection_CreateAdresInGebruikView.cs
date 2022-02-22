using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class WmsProjection_CreateAdresInGebruikView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresInGebruik] WITH SCHEMABINDING AS
SELECT
       a.[PersistentLocalId] as 'ObjectId'
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[PersistentLocalId]) as 'Id'
      ,a.[VersionAsString] as 'VersieId'
      ,s.[straatnaamObjectId] as 'StraatnaamObjectId'
      ,s.[straatnaamNL] as 'Straatnaam'
      ,a.[HouseNumberLabel] as 'HuisnummerLabel'
      ,a.[LabelType] as 'LabelType'
      ,LEN(a.[HouseNumberLabel]) as 'LabelLengte'
      ,s.[NisCode] as 'GemeenteObjectId'
      ,s.[gemeenteNaamNL] as 'Gemeentenaam'
      ,a.[PostalCode] as 'PostinfoObjectId'
      ,CASE
        WHEN s.[straatnaamNL] IS NOT NULL AND a.[BoxNumber] IS NULL THEN CONCAT(s.[straatnaamNL], ' ',  a.[HouseNumber], ', ', a.[PostalCode], ' ', s.[gemeenteNaamNL])
        WHEN s.[straatnaamNL] IS NOT NULL AND a.[BoxNumber] IS NOT NULL THEN CONCAT(s.[straatnaamNL], ' ',  a.[HouseNumber], ' bus ', a.[BoxNumber] ,', ', a.[PostalCode], ' ', s.[gemeenteNaamNL])
        ELSE NULL
      END AS 'VolledigAdres'
      ,a.[HouseNumber] as 'Huisnummer'
      ,a.[BoxNumber] as 'Busnummer'
      ,a.[Status] as 'AdresStatus'
      ,a.[Position] as 'AdresPositie'
      ,a.[PositionMethod]  as 'PositieGeometrieMethode'
      ,a.[PositionSpecification] as 'PositieSpecificatie'
      ,a.[OfficiallyAssigned] as 'OfficieelToegekend'
FROM [wms.address].[AddressDetails] as a
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
        FROM [wms.streetname].[StreetNameHelper] as s
        INNER JOIN [wms.municipality].[MunicipalityHelper] as m
        ON s.[NisCode] = m.[NisCode]
        WHERE s.[Removed] = 0 AND s.[Complete] = 1
    ) as s
  ON a.[StreetNameId] = s.[StraatnaamId]
WHERE a.[Removed] = 0
  AND a.[Complete] = 1
  AND a.[Status] = 'InGebruik'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('wms.AdresInGebruik', 'V') IS NOT NULL
    DROP VIEW [wms].[AdresInGebruik]
");
        }
    }
}
