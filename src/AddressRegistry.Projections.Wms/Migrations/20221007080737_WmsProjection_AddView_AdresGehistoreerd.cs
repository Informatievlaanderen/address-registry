namespace AddressRegistry.Projections.Wms.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class WmsProjection_AddView_AdresGehistoreerd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
FROM [wms].[AdresView] WITH(NOEXPAND)
WHERE [AdresStatus] = 'Gehistoreerd'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[AdresGehistoreerd]', N'V') IS NOT NULL
                    DROP VIEW [wms].[AdresGehistoreerd]; ");
        }
    }
}
