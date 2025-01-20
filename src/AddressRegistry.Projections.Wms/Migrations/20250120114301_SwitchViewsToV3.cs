using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    /// <inheritdoc />
    public partial class SwitchViewsToV3 : Migration
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
FROM [wms.address].AddressWmsV3 as a
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
