using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Producer.Migrations
{
    /// <inheritdoc />
    public partial class AddOffsetOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "AddressRegistryProducer",
                table: "ProjectionStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "AddressRegistryProducer",
                table: "ProjectionStates",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "AddressRegistryProducer",
                table: "ProjectionStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectionStates",
                schema: "AddressRegistryProducer",
                table: "ProjectionStates",
                column: "Name")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
