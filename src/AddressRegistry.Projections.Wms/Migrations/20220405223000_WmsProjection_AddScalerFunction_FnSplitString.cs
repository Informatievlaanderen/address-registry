using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class WmsProjection_AddScalerFunction_FnSplitString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
Create FUNCTION [wms.address].[fn_split_string]
(
    @string    nvarchar(max),
    @delimiter nvarchar(max)
)
/*
    The same as STRING_SPLIT for compatibility level < 130
    https://docs.microsoft.com/en-us/sql/t-sql/functions/string-split-transact-sql?view=sql-server-ver15
*/
RETURNS TABLE WITH SCHEMABINDING AS RETURN
(
    SELECT
      --ROW_NUMBER ( ) over(order by (select 0))                            AS id     --  intuitive, but not correect
        Split.a.value('let $n := . return count(../*[. << $n]) + 1', 'int') AS id
      , Split.a.value('.', 'NVARCHAR(MAX)')                                 AS value
    FROM
    (
        SELECT CAST('<X>'+REPLACE(@string, @delimiter, '</X><X>')+'</X>' AS XML) AS String
    ) AS a
    CROSS APPLY String.nodes('/X') AS Split(a)
)
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms.address].[fn_split_string]', N'FN') IS NOT NULL
    DROP FUNCTION [wms.address].[fn_split_string];");
        }
    }
}
