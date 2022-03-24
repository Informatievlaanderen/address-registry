using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class Initial_WmsProjection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wms.address");

            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "wms.address",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelType = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: true),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: true),
                    PositionAsText = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "[Position].STAsText()", stored: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "wms.address",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "wms.address",
                table: "AddressDetails",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "wms.address",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Status",
                schema: "wms.address",
                table: "AddressDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_StreetNameId",
                schema: "wms.address",
                table: "AddressDetails",
                column: "StreetNameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "wms.address");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "wms.address");
        }
    }
}
