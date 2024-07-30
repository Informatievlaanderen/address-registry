using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddAddressMatchTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RRStreetNames",
                schema: Infrastructure.Schema.Legacy,
                columns: table => new
                {
                    StreetNameId = table.Column<int>(nullable: false),
                    StreetName = table.Column<string>(nullable: true),
                    StreetCode = table.Column<string>(nullable: true, maxLength: 4),
                    PostalCode = table.Column<string>(nullable: true, maxLength: 4)
                });

            migrationBuilder.CreateTable(
                name: "KadStreetNames",
                schema: Infrastructure.Schema.Legacy,
                columns: table => new
                {
                    StreetNameId = table.Column<int>(nullable: false),
                    KadStreetNameCode = table.Column<string>(nullable: true, maxLength: 5),
                    NisCode = table.Column<string>(nullable: true, maxLength: 5)
                });

            migrationBuilder.CreateTable(
                name: "RRAddresses",
                schema: Infrastructure.Schema.Legacy,
                columns: table => new
                {
                    AddressId = table.Column<int>(nullable: false),
                    AddressType = table.Column<string>(nullable: false, maxLength: 1),
                    RRHouseNumber = table.Column<string>(nullable: true, maxLength: 11),
                    RRIndex = table.Column<string>(nullable: true, maxLength: 4),
                    StreetCode = table.Column<string>(nullable: true, maxLength: 4),
                    PostalCode = table.Column<string>(nullable: true, maxLength: 4)
                });

            migrationBuilder.CreateIndex(
                name: "IX_RRStreetName_StreetCode",
                schema: Infrastructure.Schema.Legacy,
                table: "RRStreetNames",
                column: "StreetCode");

            migrationBuilder.CreateIndex(
                name: "IX_RRStreetName_PostalCode",
                schema: Infrastructure.Schema.Legacy,
                table: "RRStreetNames",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_KadStreetNames_KadStreetNameCode",
                schema: Infrastructure.Schema.Legacy,
                table: "KadStreetNames",
                column: "KadStreetNameCode");

            migrationBuilder.CreateIndex(
                name: "IX_KadStreetNames_NisCode",
                schema: Infrastructure.Schema.Legacy,
                table: "KadStreetNames",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_RRHouseNumber",
                schema: Infrastructure.Schema.Legacy,
                table: "RRAddresses",
                column: "RRHouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_RRIndex",
                schema: Infrastructure.Schema.Legacy,
                table: "RRAddresses",
                column: "RRIndex");

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_StreetCode",
                schema: Infrastructure.Schema.Legacy,
                table: "RRAddresses",
                column: "StreetCode");

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_PostalCode",
                schema: Infrastructure.Schema.Legacy,
                table: "RRAddresses",
                column: "PostalCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RRStreetNames",
                schema: Infrastructure.Schema.Legacy);

            migrationBuilder.DropTable(
                name: "KadStreetNames",
                schema: Infrastructure.Schema.Legacy);

            migrationBuilder.DropTable(
                name: "RRAddresses",
                schema: Infrastructure.Schema.Legacy);
        }
    }
}
