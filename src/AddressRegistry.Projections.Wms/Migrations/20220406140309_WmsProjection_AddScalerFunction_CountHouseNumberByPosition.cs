using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class WmsProjection_AddScalerFunction_CountHouseNumberByPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION [wms.address].[CountHouseNumberByPosition](@Position geometry, @Status NVARCHAR(max))
RETURNS INT WITH SCHEMABINDING
AS
BEGIN
    DECLARE @HouseNumberCount INT
    SET @HouseNumberCount = 0

    IF @Position IS NULL
        RETURN @HouseNumberCount;

SELECT
    @HouseNumberCount = COUNT(*)
FROM
(
    SELECT DISTINCT HouseNumber
    FROM [wms.address].[AddressDetails]
    WHERE [Position] IS NOT NULL
        AND [Position].STDistance(@Position) < 1
        AND [Status] = @Status
        AND [Removed] = 0
        AND [Complete] = 1
) as HouseNumberCount
    RETURN @HouseNumberCount;
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms.address].[CountHouseNumberByPosition]', N'FN') IS NOT NULL
    DROP FUNCTION [wms.address].[CountHouseNumberByPosition];");
        }
    }
}
