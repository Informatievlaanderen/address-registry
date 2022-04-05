﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
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
FROM [wms.address].[GetAddressView]('Gehistoreerd')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms].[AdresGehistoreerd]', N'V') IS NOT NULL
    DROP VIEW [wms].[AdresGehistoreerd]; ");
        }
    }
}
