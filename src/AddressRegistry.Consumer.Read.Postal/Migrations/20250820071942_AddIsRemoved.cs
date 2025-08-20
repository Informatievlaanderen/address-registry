using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Consumer.Read.Postal.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "AddressRegistryConsumerReadPostal",
                table: "LatestItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "AddressRegistryConsumerReadPostal",
                table: "LatestItems");
        }
    }
}
