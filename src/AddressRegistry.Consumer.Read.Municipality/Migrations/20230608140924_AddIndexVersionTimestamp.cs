using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.Municipality.Migrations
{
    public partial class AddIndexVersionTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems",
                column: "VersionTimestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LatestItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadMunicipality",
                table: "LatestItems");
        }
    }
}
