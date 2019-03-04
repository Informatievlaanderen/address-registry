using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryExtract");

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    AddressOsloId = table.Column<int>(nullable: false),
                    Complete = table.Column<bool>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    MinimumX = table.Column<double>(nullable: false),
                    MaximumX = table.Column<double>(nullable: false),
                    MinimumY = table.Column<double>(nullable: false),
                    MaximumY = table.Column<double>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressExtractMunicipalities",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    PrimaryLanguage = table.Column<int>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressExtractMunicipalities", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressExtractStreetNames",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressExtractStreetNames", x => x.StreetNameId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryExtract",
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

            migrationBuilder.CreateIndex(
                name: "IX_Address_NisCode",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_Address_StreetNameId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "StreetNameId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressExtractMunicipalities_NisCode",
                schema: "AddressRegistryExtract",
                table: "AddressExtractMunicipalities",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressExtractMunicipalities_Position",
                schema: "AddressRegistryExtract",
                table: "AddressExtractMunicipalities",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_AddressExtractStreetNames_NisCode",
                schema: "AddressRegistryExtract",
                table: "AddressExtractStreetNames",
                column: "NisCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "AddressExtractMunicipalities",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "AddressExtractStreetNames",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryExtract");
        }
    }
}
