using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    OfficiallyAssigned = table.Column<bool>(nullable: true),
                    Position = table.Column<byte[]>(nullable: true),
                    PositionMethod = table.Column<int>(nullable: true),
                    PositionSpecification = table.Column<int>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressList",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressList", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressSyndication",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    ChangeType = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(nullable: false),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Plan = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressVersions",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    StreamPosition = table.Column<long>(nullable: false),
                    OsloId = table.Column<int>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    OfficiallyAssigned = table.Column<bool>(nullable: true),
                    Position = table.Column<byte[]>(nullable: true),
                    PositionMethod = table.Column<int>(nullable: true),
                    PositionSpecification = table.Column<int>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: true),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Plan = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressVersions", x => new { x.AddressId, x.StreamPosition })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "CrabIdToOsloIds",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<int>(nullable: true),
                    HouseNumberId = table.Column<int>(nullable: true),
                    SubaddressId = table.Column<int>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrabIdToOsloIds", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "MunicipalityBosa",
                schema: "AddressRegistryLegacy",
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
                    table.PrimaryKey("PK_MunicipalityBosa", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryLegacy",
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
                name: "StreetNameBosa",
                schema: "AddressRegistryLegacy",
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
                    table.PrimaryKey("PK_StreetNameBosa", x => x.StreetNameId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "OsloId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_Complete_Removed",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                columns: new[] { "Complete", "Removed" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_AddressId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "OsloId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressVersions_OsloId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                column: "OsloId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "HouseNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_IsRemoved",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_OsloId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "OsloId",
                unique: true,
                filter: "[OsloId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToOsloIds_SubaddressId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToOsloIds",
                column: "SubaddressId");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_NameDutchSearch",
                schema: "AddressRegistryLegacy",
                table: "MunicipalityBosa",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_NameEnglishSearch",
                schema: "AddressRegistryLegacy",
                table: "MunicipalityBosa",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_NameFrenchSearch",
                schema: "AddressRegistryLegacy",
                table: "MunicipalityBosa",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_NameGermanSearch",
                schema: "AddressRegistryLegacy",
                table: "MunicipalityBosa",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_NisCode",
                schema: "AddressRegistryLegacy",
                table: "MunicipalityBosa",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_Position",
                schema: "AddressRegistryLegacy",
                table: "MunicipalityBosa",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_IsComplete",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_NameDutchSearch",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_NameEnglishSearch",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_NameFrenchSearch",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_NameGermanSearch",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_NisCode",
                schema: "AddressRegistryLegacy",
                table: "StreetNameBosa",
                column: "NisCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressList",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressSyndication",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressVersions",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "CrabIdToOsloIds",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "MunicipalityBosa",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "StreetNameBosa",
                schema: "AddressRegistryLegacy");
        }
    }
}
