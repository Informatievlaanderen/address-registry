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
    DECLARE
        @HouseNumberLabel NVARCHAR(max),
        @HouseNumberCount INT

    SET @HouseNumberLabel = NULL
    SET @HouseNumberCount = 0

    IF @Removed = 1 OR @Position IS NULL
        RETURN @HouseNumberLabel;

    SET @HouseNumberCount = [wms.address].[CountHouseNumberByPosition](@Position, @Status)

    SELECT
        @HouseNumberLabel =
        CASE
                WHEN @HouseNumberCount = 0
                    THEN NULL
                WHEN @HouseNumberCount = 1
                    THEN Min(HouseNumber.Label)
                WHEN Min(HouseNumber.Label) = Max(HouseNumber.Label)
                    THEN Min(HouseNumber.Label)
                    ELSE CONCAT(Min(HouseNumber.Label), '-' , Max(HouseNumber.Label))
            END
    FROM
        (
            SELECT
                HouseNumber as Label
            FROM [wms.address].[AddressDetails]
            WHERE [Position].STDistance(@Position) < 1
                AND [Status] = @Status
                AND [Removed] = 0
                AND [Complete] = 1
        ) as HouseNumber
    RETURN @HouseNumberLabel;
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
