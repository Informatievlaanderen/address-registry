using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWfsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "wfs.address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "wfs.address",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: true),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "wfs.address",
                table: "AddressDetails",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "wfs.address",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete_StreetNameId",
                schema: "wfs.address",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete", "StreetNameId" })
                .Annotation("SqlServer:Include", new[] { "BoxNumber", "HouseNumber", "OfficiallyAssigned", "PersistentLocalId", "Position", "PositionMethod", "PositionSpecification", "PostalCode", "Status", "VersionAsString" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_StreetNameId",
                schema: "wfs.address",
                table: "AddressDetails",
                column: "StreetNameId");
        }
    }
}
