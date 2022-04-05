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
    DECLARE @HouseNumberLabel NVARCHAR(max)
    SET @HouseNumberLabel = NULL

    IF @Removed = 1 OR @Position IS NULL
        RETURN @HouseNumberLabel;

    SELECT
        @HouseNumberLabel = CASE
                    WHEN COUNT(HouseNumber.Label) = 0
                        THEN NULL
                    WHEN Min(HouseNumber.Label) = Max(HouseNumber.Label)
                        THEN Min(HouseNumber.Label)
                        ELSE CONCAT(Min(HouseNumber.Label), '-' , Max(HouseNumber.Label))
                END
    FROM
        (
            SELECT
            CASE
                WHEN BoxNumber IS NULL
                THEN HouseNumber
                ELSE CONCAT(HouseNumber, '/', BoxNumber)
            END as Label
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
