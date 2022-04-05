using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class WmsProjection_AddTableFunction_GetView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION [wms.address].[GetAddressView](@Status NVARCHAR(max))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
(SELECT
         a.[ObjectId]
        ,a.[Id]
        ,a.[VersieId]
        ,s.[StraatnaamObjectId]
        ,s.[Straatnaam]
        ,a.[HuisnummerLabel]
        ,a.[LabelType]
        ,LEN(a.[HuisnummerLabel]) as LabelLengte
        ,s.[GemeenteObjectId]
        ,s.[Gemeentenaam]
        ,a.[PostinfoObjectId]
        ,[wms.address].[GetFullAddress](s.[Straatnaam], a.[Huisnummer], a.[Busnummer], a.[PostalCode], s.[Gemeentenaam]) as VolledigAdres
        ,a.[Huisnummer]
        ,a.[Busnummer]
        ,a.[AdresStatus]
        ,a.[AdresPositie]
        ,a.[PositieGeometrieMethode]
        ,a.[PositieSpecificatie]
        ,a.[OfficieelToegekend]
    FROM (
        SELECT
             [PersistentLocalId] as ObjectId
            ,CONCAT('https://data.vlaanderen.be/id/adres/',[PersistentLocalId]) as Id
            ,[VersionAsString] as VersieId
            ,[wms.address].[GetHouseNumberLabel]([Position], [Status], [Removed]) as HuisnummerLabel
            ,[LabelType]
            ,[PostalCode] as PostinfoObjectId
            ,[HouseNumber] as Huisnummer
            ,[BoxNumber] as Busnummer
            ,[PostalCode]
            ,[Status] as AdresStatus
            ,[Position] as AdresPositie
            ,[PositionMethod] as PositieGeometrieMethode
            ,[PositionSpecification] as PositieSpecificatie
            ,[OfficiallyAssigned] as OfficieelToegekend
            ,[StreetNameId]
            ,[Removed]
            ,[Complete]
         FROM [wms.address].AddressDetails
    ) as a
    LEFT JOIN
        (
            SELECT s.[StreetNameId] as StraatnaamId
                ,s.[PersistentLocalId] as StraatnaamObjectId
                ,s.[NisCode] as GemeenteObjectId
                ,s.[NameDutch] as Straatnaam
                ,m.[NameDutch] as Gemeentenaam
            FROM [wms.streetname].[StreetNameHelper] as s
            INNER JOIN [wms.municipality].[MunicipalityHelper] as m
            ON s.[NisCode] = m.[NisCode]
            WHERE s.[Removed] = 0 AND s.[Complete] = 1
        ) as s
      ON a.[StreetNameId] = s.[StraatnaamId]
    WHERE a.[Removed] = 0
      AND a.[Complete] = 1
      AND a.[AdresStatus] = @Status
)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms.address].[GetAddressView]', N'IF') IS NOT NULL
    DROP FUNCTION [wms.address].[GetAddressView];  ");
        }
    }
}
