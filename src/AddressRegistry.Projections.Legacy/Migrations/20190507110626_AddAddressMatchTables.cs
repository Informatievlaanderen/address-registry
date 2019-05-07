using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    using AddressMatch;

    public partial class AddAddressMatchTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: RRStreetName.TableName,
                schema: Infrastructure.Schema.Legacy,
                columns: table => new
                {
                    StreetNameId = table.Column<int>(nullable: false),
                    StreetName = table.Column<string>(nullable: true),
                    StreetCode = table.Column<string>(nullable: true, maxLength: 4),
                    PostalCode = table.Column<string>(nullable: true, maxLength: 4)
                });

            migrationBuilder.CreateTable(
                name: KadStreetName.TableName,
                schema: Infrastructure.Schema.Legacy,
                columns: table => new
                {
                    StreetNameId = table.Column<int>(nullable: false),
                    KadStreetNameCode = table.Column<string>(nullable: true, maxLength: 5),
                    NisCode = table.Column<string>(nullable: true, maxLength: 5)
                });

            migrationBuilder.CreateTable(
                name: RRAddress.TableName,
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
                table: RRStreetName.TableName,
                column: nameof(RRStreetName.StreetCode));

            migrationBuilder.CreateIndex(
                name: "IX_RRStreetName_PostalCode",
                schema: Infrastructure.Schema.Legacy,
                table: RRStreetName.TableName,
                column: nameof(RRStreetName.PostalCode));

            migrationBuilder.CreateIndex(
                name: "IX_KadStreetNames_KadStreetNameCode",
                schema: Infrastructure.Schema.Legacy,
                table: KadStreetName.TableName,
                column: nameof(KadStreetName.KadStreetNameCode));

            migrationBuilder.CreateIndex(
                name: "IX_KadStreetNames_NisCode",
                schema: Infrastructure.Schema.Legacy,
                table: KadStreetName.TableName,
                column: nameof(KadStreetName.NisCode));

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_RRHouseNumber",
                schema: Infrastructure.Schema.Legacy,
                table: RRAddress.TableName,
                column: nameof(RRAddress.RRHouseNumber));

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_RRIndex",
                schema: Infrastructure.Schema.Legacy,
                table: RRAddress.TableName,
                column: nameof(RRAddress.RRIndex));

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_StreetCode",
                schema: Infrastructure.Schema.Legacy,
                table: RRAddress.TableName,
                column: nameof(RRAddress.StreetCode));

            migrationBuilder.CreateIndex(
                name: "IX_RRAddresses_PostalCode",
                schema: Infrastructure.Schema.Legacy,
                table: RRAddress.TableName,
                column: nameof(RRAddress.PostalCode));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: RRStreetName.TableName,
                schema: Infrastructure.Schema.Legacy);

            migrationBuilder.DropTable(
                name: KadStreetName.TableName,
                schema: Infrastructure.Schema.Legacy);

            migrationBuilder.DropTable(
                name: RRAddress.TableName,
                schema: Infrastructure.Schema.Legacy);
        }
    }
}
