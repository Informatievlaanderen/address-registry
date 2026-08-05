using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    using Infrastructure;

    /// <inheritdoc />
    public partial class AddWmsV4_Lambert2008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressWmsV4",
                schema: "wms.address",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    ParentAddressPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HouseNumberLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberLabelLength = table.Column<int>(type: "int", nullable: true),
                    LabelType = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressWmsV4", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV4_ParentAddressPersistentLocalId",
                schema: "wms.address",
                table: "AddressWmsV4",
                column: "ParentAddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV4_PositionX_PositionY_Removed_Status",
                schema: "wms.address",
                table: "AddressWmsV4",
                columns: new[] { "PositionX", "PositionY", "Removed", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV4_Removed_Status",
                schema: "wms.address",
                table: "AddressWmsV4",
                columns: new[] { "Removed", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV4_Status",
                schema: "wms.address",
                table: "AddressWmsV4",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV4_StreetNamePersistentLocalId",
                schema: "wms.address",
                table: "AddressWmsV4",
                column: "StreetNamePersistentLocalId");

            // The V3 bounding box (22279.17, 153050.23, 258873.3, 244022.31) expressed in Lambert 2008:
            // all four corners transformed, then the envelope padded out to the next 100 m.
            migrationBuilder.Sql(
                $"CREATE SPATIAL INDEX [SPATIAL_AddressV4_Position] ON [{Schema.Wms}].[AddressWmsV4] ([Position])\n" +
                @"USING  GEOMETRY_GRID
	                WITH (
		                BOUNDING_BOX =(522200, 653000, 758900, 744100),
		                GRIDS =(
			                LEVEL_1 = MEDIUM,
			                LEVEL_2 = MEDIUM,
			                LEVEL_3 = MEDIUM,
			                LEVEL_4 = MEDIUM),
	                CELLS_PER_OBJECT = 5)
	                GO");

            foreach (var view in ViewNames)
            {
                migrationBuilder.Sql($@"
IF OBJECT_ID('wms.{view}', 'V') IS NOT NULL
    DROP VIEW [wms].[{view}]
");
            }

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[AdresViewV4] WITH SCHEMABINDING AS
SELECT  a.[AddressPersistentLocalId] as ObjectId
        ,CONCAT('https://data.vlaanderen.be/id/adres/', a.[AddressPersistentLocalId]) as Id
        ,a.[VersionAsString] as VersieId
        ,s.[PersistentLocalId] as StraatnaamObjectId
        ,s.[NameDutch] as Straatnaam
        ,a.[HouseNumberLabel] as HuisnummerLabel
        ,a.[LabelType]
        ,a.[HouseNumberLabelLength] as LabelLengte
        ,s.[NisCode] as GemeenteObjectId
        ,m.[NameDutch] as Gemeentenaam
        ,a.[PostalCode] as PostinfoObjectId
        ,[wms].[GetFullAddress](s.[NameDutch], a.[HouseNumber], a.[BoxNumber], a.[PostalCode], m.[NameDutch]) as VolledigAdres
        ,a.[HouseNumber] as Huisnummer
        ,a.[BoxNumber] as Busnummer
        ,a.[Status] as AdresStatus
        ,a.[Position] as AdresPositie
        ,a.[PositionMethod] as PositieGeometrieMethode
        ,a.[PositionSpecification] as PositieSpecificatie
        ,a.[OfficiallyAssigned] as OfficieelToegekend
FROM [wms.address].AddressWmsV4 as a
JOIN [wms.streetname].[StreetNameHelperV2] s ON a.[StreetNamePersistentLocalId] = s.[PersistentLocalId] AND a.[Removed] = 0
JOIN [wms.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_AdresViewV4_ObjectId ON [wms].[AdresViewV4] ([ObjectId])");

            foreach (var (view, status) in StatusViews)
            {
                migrationBuilder.Sql($@"
CREATE VIEW [wms].[{view}] WITH SCHEMABINDING AS
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
FROM [wms].[AdresViewV4]
WHERE [AdresStatus] = '{status}'");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var view in ViewNames)
            {
                migrationBuilder.Sql($@"
IF OBJECT_ID('wms.{view}', 'V') IS NOT NULL
    DROP VIEW [wms].[{view}]
");
            }

            migrationBuilder.DropTable(
                name: "AddressWmsV4",
                schema: "wms.address");
        }

        /// <summary>The status views select from <c>AdresViewV4</c>, so they are dropped before it.</summary>
        private static readonly (string View, string Status)[] StatusViews =
        [
            ("AdresVoorgesteldV4", "Voorgesteld"),
            ("AdresInGebruikV4", "InGebruik"),
            ("AdresGehistoreerdV4", "Gehistoreerd"),
            ("AdresAfgekeurdV4", "Afgekeurd")
        ];

        private static readonly string[] ViewNames =
            [.. StatusViews.Select(x => x.View), "AdresViewV4"];
    }
}
