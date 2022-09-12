using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.StreetName.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryConsumerReadStreetName");

            migrationBuilder.CreateTable(
                name: "BosaItems",
                schema: "AddressRegistryConsumerReadStreetName",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    NameDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameDutchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameFrenchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameGermanSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnglishSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HomonymAdditionDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BosaItems", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "LatestItems",
                schema: "AddressRegistryConsumerReadStreetName",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    NameDutch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameDutchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrenchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGermanSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglishSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HomonymAdditionDutch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HomonymAdditionFrench = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HomonymAdditionGerman = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestItems", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadStreetName",
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
                name: "IX_BosaItems_IsRemoved",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameDutchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameEnglishSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameFrenchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NameGermanSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_NisCode",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_Status",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BosaItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "BosaItems",
                column: "VersionTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_HomonymAdditionDutch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "HomonymAdditionDutch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_HomonymAdditionEnglish",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "HomonymAdditionEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_HomonymAdditionFrench",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "HomonymAdditionFrench");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_HomonymAdditionGerman",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "HomonymAdditionGerman");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_IsRemoved",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameDutch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameDutch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameDutchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameEnglish",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameEnglishSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameFrench",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameFrench");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameFrenchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameGerman",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameGerman");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NameGermanSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_NisCode",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_Status",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BosaItems",
                schema: "AddressRegistryConsumerReadStreetName");

            migrationBuilder.DropTable(
                name: "LatestItems",
                schema: "AddressRegistryConsumerReadStreetName");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadStreetName");
        }
    }
}
