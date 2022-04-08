using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class WmsProjection_AddScalerFunction_GetHouseNumberLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION [wms.address].[GetHouseNumberLabel](@Position geometry, @Status NVARCHAR(max), @Removed bit)
RETURNS NVARCHAR(max) WITH SCHEMABINDING
AS
BEGIN
    IF @Removed = 1 OR @Position IS NULL
        RETURN NULL;

    DECLARE
        @Val NVARCHAR(MAX),
        @Delimiter NVARCHAR = ';'

    Select @Val = COALESCE(@Val + @Delimiter + HouseNumber, HouseNumber)
    FROM (
      SELECT DISTINCT HouseNumber
      FROM [wms.address].[AddressDetails]
      WHERE [Position].STDistance(@Position) < 0.00001
        AND [Status] = @Status
        AND [Removed] = 0
        AND [Complete] = 1
    ) as HouseNumbers

    DECLARE @Ret NVARCHAR(max)
    SET @Ret = [wms.address].[GetHouseNoLabel](@Val, @Delimiter)
    RETURN @Ret
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms.address].[GetHouseNumberLabel]', N'FN') IS NOT NULL
    DROP FUNCTION [wms.address].[GetHouseNumberLabel];  ");
        }
    }
}
