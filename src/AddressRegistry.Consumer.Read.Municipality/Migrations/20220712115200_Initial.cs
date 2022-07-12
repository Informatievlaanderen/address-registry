using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.Municipality.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryConsumerReadMunicipality");

            migrationBuilder.CreateTable(
                name: "LatestItems",
                schema: "AddressRegistryConsumerReadMunicipality",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameDutchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameFrenchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameGermanSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnglishSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExtendedWkbGeometry = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OfficialLanguages = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestItems", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadMunicipality",
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
                name: "IX_LatestItems_NameDutchSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameEnglishSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameFrenchSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameGermanSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NisCode",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LatestItems",
                schema: "AddressRegistryConsumerReadMunicipality");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadMunicipality");
        }
    }
}
