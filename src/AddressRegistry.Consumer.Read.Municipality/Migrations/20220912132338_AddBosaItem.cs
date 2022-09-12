using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.Municipality.Migrations
{
    public partial class AddBosaItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BosaItems",
                schema: "AddressRegistryConsumerReadMunicipality",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    IsFlemishRegion = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OfficialLanguages = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BosaItems", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_IsFlemishRegion",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "IsFlemishRegion");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameDutchSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameEnglishSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameFrenchSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameGermanSearch",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NisCode",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "BosaItems",
                column: "VersionTimestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BosaItems",
                schema: "AddressRegistryConsumerReadMunicipality");
        }
    }
}
