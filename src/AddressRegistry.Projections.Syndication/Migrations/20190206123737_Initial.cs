using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "MunicipalityBosaSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    IsFlemishRegion = table.Column<bool>(nullable: false),
                    PrimaryLanguage = table.Column<int>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityBosaSyndication", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "MunicipalityLatestSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    PrimaryLanguage = table.Column<int>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityLatestSyndication", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "MunicipalitySyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    OfficialLanguages = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalitySyndication", x => new { x.MunicipalityId, x.Position })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameBosaSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    HomonymAdditionDutch = table.Column<string>(nullable: true),
                    HomonymAdditionFrench = table.Column<string>(nullable: true),
                    HomonymAdditionGerman = table.Column<string>(nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameBosaSyndication", x => x.StreetNameId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameLatestSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    HomonymAdditionDutch = table.Column<string>(nullable: true),
                    HomonymAdditionFrench = table.Column<string>(nullable: true),
                    HomonymAdditionGerman = table.Column<string>(nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameLatestSyndication", x => x.StreetNameId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    HomonymAdditionDutch = table.Column<string>(nullable: true),
                    HomonymAdditionFrench = table.Column<string>(nullable: true),
                    HomonymAdditionGerman = table.Column<string>(nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetNameSyndication", x => new { x.StreetNameId, x.Position })
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
                name: "IX_MunicipalityLatestSyndication_NameDutchSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameEnglishSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameFrenchSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameGermanSearch",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_Position",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Position",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Version");

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

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_HomonymAdditionDutch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "HomonymAdditionDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_HomonymAdditionEnglish",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "HomonymAdditionEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_HomonymAdditionFrench",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "HomonymAdditionFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_HomonymAdditionGerman",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "HomonymAdditionGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_IsComplete",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameDutch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameEnglish",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameFrench",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NameGerman",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NameGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_HomonymAdditionDutch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "HomonymAdditionDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_HomonymAdditionEnglish",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "HomonymAdditionEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_HomonymAdditionFrench",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "HomonymAdditionFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_HomonymAdditionGerman",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "HomonymAdditionGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_NameDutch",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "NameDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_NameEnglish",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_NameFrench",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "NameFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameSyndication_NameGerman",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                column: "NameGerman");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityBosaSyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "MunicipalityLatestSyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "MunicipalitySyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "StreetNameBosaSyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "StreetNameLatestSyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "StreetNameSyndication",
                schema: "AddressRegistrySyndication");
        }
    }
}
