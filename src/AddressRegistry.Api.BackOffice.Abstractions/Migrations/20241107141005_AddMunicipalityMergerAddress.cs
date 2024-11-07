using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Api.BackOffice.Abstractions.Migrations
{
    /// <inheritdoc />
    public partial class AddMunicipalityMergerAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MunicipalityMergerAddresses",
                schema: "AddressRegistryBackOffice",
                columns: table => new
                {
                    OldAddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    OldStreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    NewStreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    NewAddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityMergerAddresses", x => x.OldAddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityMergerAddresses_OldStreetNamePersistentLocalId",
                schema: "AddressRegistryBackOffice",
                table: "MunicipalityMergerAddresses",
                column: "OldStreetNamePersistentLocalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityMergerAddresses",
                schema: "AddressRegistryBackOffice");
        }
    }
}
