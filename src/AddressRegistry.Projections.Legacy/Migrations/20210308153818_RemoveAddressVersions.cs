using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveAddressVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressVersions",
                schema: "AddressRegistryLegacy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressVersions",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StreamPosition = table.Column<long>(type: "bigint", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complete = table.Column<bool>(type: "bit", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PositionMethod = table.Column<int>(type: "int", nullable: true),
                    PositionSpecification = table.Column<int>(type: "int", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressVersions", x => new { x.AddressId, x.StreamPosition })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                column: "PersistentLocalId")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
