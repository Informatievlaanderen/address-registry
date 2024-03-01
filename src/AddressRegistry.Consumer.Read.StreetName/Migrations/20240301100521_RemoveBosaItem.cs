using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.StreetName.Migrations
{
    public partial class RemoveBosaItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BosaItems",
                schema: "AddressRegistryConsumerReadStreetName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BosaItems",
                schema: "AddressRegistryConsumerReadStreetName",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    HomonymAdditionDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAdditionGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    NameDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameDutchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEnglishSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameFrenchSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NameGerman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameGermanSearch = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BosaItems", x => x.PersistentLocalId)
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
        }
    }
}
