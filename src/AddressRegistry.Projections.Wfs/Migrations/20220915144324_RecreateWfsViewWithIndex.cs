using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    public partial class RecreateWfsViewWithIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wfs].[AdresView]");
            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresView] WITH SCHEMABINDING AS
SELECT a.[PersistentLocalId] as ObjectId
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[PersistentLocalId]) as 'Id'
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
      ,a.[Status] as 'AdresStatus'
      ,a.[Position] as 'AdresPositie'
      ,a.[PositionMethod]  as 'PositieGeometrieMethode'
      ,a.[PositionSpecification] as 'PositieSpecificatie'
      ,a.[OfficiallyAssigned] as 'OfficieelToegekend'
FROM [wfs.address].[AddressDetails] as a
JOIN [wfs.streetname].[StreetNameHelper] s ON a.[StreetNameId] = s.StreetNameId AND a.[Removed] = 0 AND a.[Complete] = 1
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0 AND s.[Complete] = 1");
            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresView_ObjectId ON [wfs].[AdresView] ([ObjectId])");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wfs].[AdresView]");
            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresView] WITH SCHEMABINDING AS
SELECT a.[PersistentLocalId] as 'ObjectId'
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[PersistentLocalId]) as 'Id'
      ,a.[VersionAsString] as 'VersieId'
      ,s.[straatnaamObjectId] as 'StraatnaamObjectId'
      ,s.[straatnaamNL] as 'Straatnaam'
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
        FROM [wfs.streetname].[StreetNameHelper] as s
        INNER JOIN [wfs.municipality].[MunicipalityHelper] as m
        ON s.[NisCode] = m.[NisCode]
        WHERE s.[Removed] = 0 AND s.[Complete] = 1
    ) as s
  ON a.[StreetNameId] = s.[StraatnaamId]
WHERE a.[Removed] = 0 AND a.[Complete] = 1");
        }
    }
}
