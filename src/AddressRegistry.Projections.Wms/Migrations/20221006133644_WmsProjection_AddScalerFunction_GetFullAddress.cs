namespace AddressRegistry.Projections.Wms.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class WmsProjection_AddScalerFunction_GetFullAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION [wms].[GetFullAddress](
    @StreetName NVARCHAR(max),
    @HouseNumber NVARCHAR(max),
    @BoxNumber NVARCHAR(max),
    @PostalCode NVARCHAR(max),
    @Municipality NVARCHAR(max)
)
RETURNS NVARCHAR(max) WITH SCHEMABINDING
AS
BEGIN
    IF @StreetName IS NOT NULL AND @BoxNumber IS NULL
        RETURN CONCAT(@StreetName, ' ',  @HouseNumber, ', ', @PostalCode, ' ', @Municipality);

    IF @StreetName IS NOT NULL AND @BoxNumber IS NOT NULL
        RETURN CONCAT(@StreetName, ' ',  @HouseNumber, ' bus ', @BoxNumber, ', ', @PostalCode, ' ', @Municipality);

    RETURN NULL;
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms].[GetFullAddress]', N'FN') IS NOT NULL
    DROP FUNCTION [wms].[GetFullAddress];");
        }
    }
}
