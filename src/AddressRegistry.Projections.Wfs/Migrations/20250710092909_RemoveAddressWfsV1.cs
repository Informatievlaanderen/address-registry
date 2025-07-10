using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddressWfsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressWfs",
                schema: "wfs.address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressWfs",
                schema: "wfs.address",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressWfs", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfs_Removed",
                schema: "wfs.address",
                table: "AddressWfs",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfs_Removed_StreetNamePersistentLocalId",
                schema: "wfs.address",
                table: "AddressWfs",
                columns: new[] { "Removed", "StreetNamePersistentLocalId" })
                .Annotation("SqlServer:Include", new[] { "BoxNumber", "HouseNumber", "OfficiallyAssigned", "AddressPersistentLocalId", "Position", "PositionMethod", "PositionSpecification", "PostalCode", "Status", "VersionAsString" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfs_StreetNamePersistentLocalId",
                schema: "wfs.address",
                table: "AddressWfs",
                column: "StreetNamePersistentLocalId");
        }
    }
}
