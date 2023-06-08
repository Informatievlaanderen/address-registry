using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.StreetName.Migrations
{
    public partial class AddIndexVersionTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "ProcessedMessages",
                column: "DateProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_LatestItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems",
                column: "VersionTimestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "ProcessedMessages");

            migrationBuilder.DropIndex(
                name: "IX_LatestItems_VersionTimestamp",
                schema: "AddressRegistryConsumerReadStreetName",
                table: "LatestItems");
        }
    }
}
