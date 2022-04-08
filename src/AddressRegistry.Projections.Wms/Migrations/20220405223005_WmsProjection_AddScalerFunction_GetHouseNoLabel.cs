using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class WmsProjection_AddScalerFunction_GetHouseNoLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION [wms.address].[GetHouseNoLabel](@ListOfHouseNumbers NVARCHAR(max), @Delimiter NVARCHAR = ';')
RETURNS NVARCHAR(max) WITH SCHEMABINDING
AS
BEGIN
    DECLARE
        @HouseNumberCount INT = 0,
        @MinHouseNumber NVARCHAR(max) = NULL,
        @MaxHouseNumber NVARCHAR(max) = NULL,
        @MinLetterHouseNumber NVARCHAR(max) = NULL,
        @MaxLetterHouseNumber NVARCHAR(max) = NULL

    SELECT @HouseNumberCount = COUNT(HouseNumber)
    FROM
    (
        SELECT [value] as HouseNumber
        FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)
    ) as HouseNumbers

    IF @HouseNumberCount = 0
        RETURN NULL

    IF @HouseNumberCount < 2 AND @HouseNumberCount > 0
    BEGIN
        DECLARE @HouseNumber NVARCHAR(max)
        SELECT TOP(1)
            @HouseNumber = HouseNumber
        FROM
        (
            SELECT [value] as HouseNumber
            FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)
        ) as HouseNumbers
        RETURN @HouseNumber
    END


    SELECT
        @MaxHouseNumber = Max([MaxHouseNumbers].[Value])
    FROM
        (
        SELECT
            Max(a.HouseNumber) AS 'Value'
            FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as a
            LEFT JOIN
            (
        SELECT DISTINCT
                HouseNumber
        , IIF(
            CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT) < 0,
            CAST([HouseNumber] AS INT),
            CAST(SUBSTRING([HouseNumber],1,CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT)) AS INT)
        ) AS ParsedHouseNumbers
            FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as HouseNumbers
            WHERE
            HouseNumber IS NOT NULL
                AND HouseNumber LIKE '[0-9.]%' --EXCLUDE LETTER STARTING HOUSENUMBERS
        ) AS b ON a.HouseNumber =  b.HouseNumber
        WHERE b.ParsedHouseNumbers IS NOT NULL
        GROUP BY ParsedHouseNumbers
    ) AS MaxHouseNumbers



    DECLARE
        @MinParsedValue INT,
        @MaxParsedValue INT

    --GET MIN
    SELECT
        @MinParsedValue = Min(b.ParsedHouseNumbers)
    FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as a
        LEFT JOIN
        (
        SELECT DISTINCT
            HouseNumber
        , IIF(
            CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT) < 0,
            CAST([HouseNumber] AS INT),
            CAST(SUBSTRING([HouseNumber],1,CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT)) AS INT)
        ) AS ParsedHouseNumbers
        FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as HouseNumbers
        WHERE
            HouseNumber IS NOT NULL
            AND HouseNumber LIKE '[0-9.]%' --EXCLUDE LETTER STARTING HOUSENUMBERS
    ) AS b ON a.HouseNumber =  b.HouseNumber
    WHERE b.ParsedHouseNumbers IS NOT NULL

    SELECT @MinHouseNumber = MIN([b].[HouseNumber])
    FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as a
        LEFT JOIN
        (
        SELECT DISTINCT
            HouseNumber
        , IIF(
            CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT) < 0,
            CAST([HouseNumber] AS INT),
            CAST(SUBSTRING([HouseNumber],1,CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT)) AS INT)
        ) AS ParsedHouseNumbers
        FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as HouseNumbers
        WHERE
            HouseNumber IS NOT NULL
            AND HouseNumber LIKE '[0-9.]%' --EXCLUDE LETTER STARTING HOUSENUMBERS
    ) AS b ON a.HouseNumber =  b.HouseNumber
    WHERE b.ParsedHouseNumbers IS NOT NULL AND b.ParsedHouseNumbers = @MinParsedValue


    --GET Max
    SELECT
        @MaxParsedValue = MAX(b.ParsedHouseNumbers)
    FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as a
        LEFT JOIN
        (
        SELECT DISTINCT
            HouseNumber
        , IIF(
            CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT) < 0,
            CAST([HouseNumber] AS INT),
            CAST(SUBSTRING([HouseNumber],1,CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT)) AS INT)
        ) AS ParsedHouseNumbers
        FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as HouseNumbers
        WHERE
            HouseNumber IS NOT NULL
            AND HouseNumber LIKE '[0-9.]%' --EXCLUDE LETTER STARTING HOUSENUMBERS
    ) AS b ON a.HouseNumber =  b.HouseNumber
    WHERE b.ParsedHouseNumbers IS NOT NULL

    SELECT @MaxHouseNumber = MAX([b].[HouseNumber])
    FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as a
        LEFT JOIN
        (
        SELECT DISTINCT
            HouseNumber
        , IIF(
            CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT) < 0,
            CAST([HouseNumber] AS INT),
            CAST(SUBSTRING([HouseNumber],1,CAST((PATINDEX('%[^0-9.]%', [HouseNumber]) - 1) AS INT)) AS INT)
        ) AS ParsedHouseNumbers
        FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as HouseNumbers
        WHERE
            HouseNumber IS NOT NULL
            AND HouseNumber LIKE '[0-9.]%' --EXCLUDE LETTER STARTING HOUSENUMBERS
    ) AS b ON a.HouseNumber =  b.HouseNumber
    WHERE b.ParsedHouseNumbers IS NOT NULL AND b.ParsedHouseNumbers = @MaxParsedValue



    -- DIRTY DATA CHECK
    SELECT @MaxLetterHouseNumber = MAX(HouseNumber), @MinLetterHouseNumber = MIN(HouseNumber)
    FROM (SELECT [value] as HouseNumber FROM [wms.address].[fn_split_string](@ListOfHouseNumbers,@Delimiter)) as HouseNumbers
    WHERE HouseNumber IS NOT NULL AND HouseNumber LIKE '[^0-9.]%'
    --HOUSENUMBERS THAT DON'T START WITH NUMBER

    IF @MinHouseNumber IS NULL
        BEGIN
            IF TRIM(LOWER(@MinLetterHouseNumber)) = TRIM(LOWER(@MaxLetterHouseNumber))
                RETURN @MinLetterHouseNumber
            ELSE
                RETURN Concat(@MinLetterHouseNumber, '-', @MaxLetterHouseNumber)
        END
    ELSE
        BEGIN
            IF @MaxLetterHouseNumber IS NOT NULL
                RETURN Concat(@MinHouseNumber, '-', @MaxLetterHouseNumber)
            ELSE
                IF TRIM(LOWER(@MinHouseNumber)) = TRIM(LOWER(@MaxHouseNumber))
                    RETURN @MinLetterHouseNumber
                ELSE
                    RETURN Concat(@MinHouseNumber, '-', @MaxHouseNumber)
        END
    RETURN NULL
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID (N'[wms.address].[GetHouseNoLabel]', N'FN') IS NOT NULL
    DROP FUNCTION [wms.address].[GetHouseNoLabel];");
        }
    }
}
