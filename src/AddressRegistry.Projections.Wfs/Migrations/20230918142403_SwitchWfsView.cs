using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    public partial class SwitchWfsView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresView', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresView]
");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresView] WITH SCHEMABINDING AS
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
      ,a.[Status] as 'AdresStatus'
      ,a.[Position] as 'AdresPositie'
      ,a.[PositionMethod]  as 'PositieGeometrieMethode'
      ,a.[PositionSpecification] as 'PositieSpecificatie'
      ,a.[OfficiallyAssigned] as 'OfficieelToegekend'
FROM [wfs.address].[AddressWfs] as a
JOIN [wfs.streetname].[StreetNameHelperV2] s ON a.StreetNamePersistentLocalId = s.PersistentLocalId AND a.[Removed] = 0
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0
");

            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresView_ObjectId ON [wfs].[AdresView] ([ObjectId])");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresView', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresView]
");

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
    }
}
