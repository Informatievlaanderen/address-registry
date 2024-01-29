using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    public partial class AddAddressIdAddressPersistentLocalIdRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address_id_address_persistent_local_id",
                schema: "integration_address",
                columns: table => new
                {
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address_id_address_persistent_local_id", x => x.address_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_address_id_address_persistent_local_id_address_id",
                schema: "integration_address",
                table: "address_id_address_persistent_local_id",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_id_address_persistent_local_id_persistent_local_id",
                schema: "integration_address",
                table: "address_id_address_persistent_local_id",
                column: "persistent_local_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_id_address_persistent_local_id",
                schema: "integration_address");
        }
    }
}
