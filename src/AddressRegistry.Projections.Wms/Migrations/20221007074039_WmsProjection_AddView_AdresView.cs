namespace AddressRegistry.Projections.Wms.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class WmsProjection_AddView_AdresView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE VIEW [wms.address].[AdresView] WITH SCHEMABINDING AS
                (SELECT
                         a.[PersistentLocalId] as ObjectId
                        ,CONCAT('https://data.vlaanderen.be/id/adres/', a.[PersistentLocalId]) as Id
                        ,a.[VersionAsString] as VersieId
                        ,s.[PersistentLocalId] as StraatnaamObjectId
                        ,s.[NameDutch] as Straatnaam
                        ,a.[HouseNumberLabel] as HuisnummerLabel
                        ,a.[LabelType]
                        ,a.[HouseNumberLabelLength] as LabelLengte
                        ,s.[NisCode] as GemeenteObjectId
                        ,m.[NameDutch] as Gemeentenaam
                        ,a.[PostalCode] as PostinfoObjectId
                        ,[wms.address].[GetFullAddress](s.[NameDutch], a.[HouseNumber], a.[BoxNumber], a.[PostalCode], m.[NameDutch]) as VolledigAdres
                        ,a.[HouseNumber] as Huisnummer
                        ,a.[BoxNumber] as Busnummer
                        ,a.[Status] as AdresStatus
                        ,a.[Position] as AdresPositie
                        ,a.[PositionMethod] as PositieGeometrieMethode
                        ,a.[PositionSpecification] as PositieSpecificatie
                        ,a.[OfficiallyAssigned] as OfficieelToegekend
                    FROM [wms.address].AddressDetails as a
	                JOIN [wms.streetname].[StreetNameHelper] s ON a.[StreetNameId] = s.StreetNameId AND a.[Removed] = 0 AND a.[Complete] = 1
	                JOIN [wms.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0 AND s.[Complete] = 1
                )
                ");

            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresView_ObjectId ON [wms.address].[AdresView] ([ObjectId])");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [wms.address].[AdresView]");
        }
    }
}
