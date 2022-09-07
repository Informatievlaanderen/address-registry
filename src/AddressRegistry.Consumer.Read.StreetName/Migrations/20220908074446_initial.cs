using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.StreetName.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryConsumerReadStreetName");

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

            migrationBuilder.CreateTable(
                name: "StreetNameBosaItem",
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
                    table.PrimaryKey("PK_StreetNameBosaItem", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "StreetNameLatestItem",
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
                    table.PrimaryKey("PK_StreetNameLatestItem", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_IsRemoved",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_NameDutchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_NameEnglishSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_NameFrenchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_NameGermanSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_NisCode",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosaItem_Status",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameBosaItem",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_HomonymAdditionDutch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "HomonymAdditionDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_HomonymAdditionEnglish",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "HomonymAdditionEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_HomonymAdditionFrench",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "HomonymAdditionFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_HomonymAdditionGerman",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "HomonymAdditionGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_IsRemoved",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameDutch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameDutch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameDutchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameEnglish",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameEnglish");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameEnglishSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameFrench",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameFrench");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameFrenchSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameGerman",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameGerman");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NameGermanSearch",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_NisCode",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameLatestItem_Status",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "StreetNameLatestItem",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryConsumerReadStreetName");

            migrationBuilder.DropTable(
                name: "StreetNameBosaItem",
                schema: "AddressRegistryConsumerReadStreetName");

            migrationBuilder.DropTable(
                name: "StreetNameLatestItem",
                schema: "AddressRegistryConsumerReadStreetName");
        }
    }
}
