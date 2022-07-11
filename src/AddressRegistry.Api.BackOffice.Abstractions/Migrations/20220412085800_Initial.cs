using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Api.BackOffice.Abstractions.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryBackOffice");

            migrationBuilder.CreateTable(
                name: "AddressPersistentIdStreetNamePersistentId",
                schema: "AddressRegistryBackOffice",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressPersistentIdStreetNamePersistentId", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressPersistentIdStreetNamePersistentId",
                schema: "AddressRegistryBackOffice");
        }
    }
}
