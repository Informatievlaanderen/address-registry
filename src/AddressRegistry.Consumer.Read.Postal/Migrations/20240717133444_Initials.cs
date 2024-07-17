using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.Postal.Migrations
{
    /// <inheritdoc />
    public partial class Initials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryConsumerReadPostal");

            migrationBuilder.CreateTable(
                name: "LatestItems",
                schema: "AddressRegistryConsumerReadPostal",
                columns: table => new
                {
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestItems", x => x.PostalCode)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadPostal",
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

            migrationBuilder.CreateTable(
                name: "LatestItemPostalNames",
                schema: "AddressRegistryConsumerReadPostal",
                columns: table => new
                {
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PostalName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestItemPostalNames", x => new { x.PostalCode, x.PostalName })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_LatestItemPostalNames_LatestItems_PostalCode",
                        column: x => x.PostalCode,
                        principalSchema: "AddressRegistryConsumerReadPostal",
                        principalTable: "LatestItems",
                        principalColumn: "PostalCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LatestItemPostalNames_PostalName",
                schema: "AddressRegistryConsumerReadPostal",
                table: "LatestItemPostalNames",
                column: "PostalName");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NisCode",
                schema: "AddressRegistryConsumerReadPostal",
                table: "LatestItems",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadPostal",
                table: "LatestItems",
                column: "VersionTimestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LatestItemPostalNames",
                schema: "AddressRegistryConsumerReadPostal");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadPostal");

            migrationBuilder.DropTable(
                name: "LatestItems",
                schema: "AddressRegistryConsumerReadPostal");
        }
    }
}
