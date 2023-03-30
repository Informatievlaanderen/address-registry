using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class AddV2Views : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresViewV2] WITH SCHEMABINDING AS
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
FROM [wms.address].AddressWms as a
JOIN [wms.streetname].[StreetNameHelperV2] s ON a.[StreetNamePersistentLocalId] = s.[PersistentLocalId] AND a.[Removed] = 0
JOIN [wms.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0
                ");

            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresViewV2_ObjectId ON [wms].[AdresViewV2] ([ObjectId])");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresVoorgesteldV2] WITH SCHEMABINDING AS
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
FROM [wms].[AdresViewV2]
WHERE [AdresStatus] = 'Voorgesteld'");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresInGebruikV2] WITH SCHEMABINDING AS
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
FROM [wms].[AdresViewV2]
WHERE [AdresStatus] = 'InGebruik'");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresGehistoreerdV2] WITH SCHEMABINDING AS
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
FROM [wms].[AdresViewV2]
WHERE [AdresStatus] = 'Gehistoreerd'");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresAfgekeurdV2] WITH SCHEMABINDING AS
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
FROM [wms].[AdresViewV2]
WHERE [AdresStatus] = 'Afgekeurd'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms].[AdresViewV2]");
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[AdresVoorgesteldV2]', N'V') IS NOT NULL
                    DROP VIEW [wms].[AdresVoorgesteldV2]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[AdresInGebruikV2]', N'V') IS NOT NULL
                    DROP VIEW [wms].[AdresInGebruikV2]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[AdresGehistoreerdV2]', N'V') IS NOT NULL
                    DROP VIEW [wms].[AdresGehistoreerdV2]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[AdresAfgekeurdV2]', N'V') IS NOT NULL
                    DROP VIEW [wms].[AdresAfgekeurdV2]; ");
        }
    }
}
