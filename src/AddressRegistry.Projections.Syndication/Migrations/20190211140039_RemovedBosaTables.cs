using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class RemovedBosaTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityBosaSyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "StreetNameBosaSyndication",
                schema: "AddressRegistrySyndication");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MunicipalityBosaSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    IsFlemishRegion = table.Column<bool>(nullable: false),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NisCode = table.Column<string>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    PrimaryLanguage = table.Column<int>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityBosaSyndication", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameBosaSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(nullable: false),
                    HomonymAdditionDutch = table.Column<string>(nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(nullable: true),
                    HomonymAdditionFrench = table.Column<string>(nullable: true),
                    HomonymAdditionGerman = table.Column<string>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NisCode = table.Column<string>(nullable: true),
                    OsloId = table.Column<string>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    Version = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameBosaSyndication", x => x.StreetNameId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosaSyndication_NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosaSyndication",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosaSyndication_NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosaSyndication",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosaSyndication_NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosaSyndication",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosaSyndication_NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosaSyndication",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosaSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosaSyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosaSyndication_Position",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosaSyndication",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaSyndication_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosaSyndication",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaSyndication_NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosaSyndication",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaSyndication_NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosaSyndication",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaSyndication_NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosaSyndication",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaSyndication_NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosaSyndication",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosaSyndication",
                column: "NisCode");
        }
    }
}
