using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.AddressMatch.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryAddressMatch");

            migrationBuilder.CreateTable(
                name: "AddressMatchDetails",
                schema: "AddressRegistryAddressMatch",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    ParentAddressPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PositionMethod = table.Column<int>(type: "int", nullable: false),
                    PositionSpecification = table.Column<int>(type: "int", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressMatchDetails", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryAddressMatch",
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
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_BoxNumber",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_HouseNumber",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_PostalCode",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_Removed",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_Status",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_StreetNamePersistentLocalId",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressMatchDetails_VersionTimestamp",
                schema: "AddressRegistryAddressMatch",
                table: "AddressMatchDetails",
                column: "VersionTimestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressMatchDetails",
                schema: "AddressRegistryAddressMatch");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryAddressMatch");
        }
    }
}
