using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    using Infrastructure;

    /// <inheritdoc />
    public partial class AddFeedPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FeedPosition",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                type: "bigint",
                nullable: false,
                defaultValue: -1L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressSyndication",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.DropIndex(
                name: "CI_AddressSyndication_Position",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.CreateIndex(
                name: "AddressSyndication_Position",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "Position");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressSyndication",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "FeedPosition")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.Sql($"SET IDENTITY_INSERT {Schema.Legacy}.AddressSyndication OFF;");

            migrationBuilder.Sql(
                $"""
                 UPDATE {Schema.Legacy}.AddressSyndication
                 SET FeedPosition = Position
                 WHERE FeedPosition = -1
                 """);

            var maxIdSql = $"SELECT MAX(Position) FROM {Schema.Legacy}.AddressSyndication";
            migrationBuilder.Sql($"DECLARE @maxId INT = ({maxIdSql});");
            migrationBuilder.Sql($"DBCC CHECKIDENT ('{Schema.Legacy}.AddressSyndication', RESEED, @maxId);");

            migrationBuilder.Sql($"SET IDENTITY_INSERT {Schema.Legacy}.AddressSyndication ON;");

            migrationBuilder.CreateIndex(
                name: "CI_AddressSyndication_FeedPosition",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "FeedPosition")
                .Annotation("SqlServer:ColumnStoreIndex", "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressSyndication",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.DropIndex(
                name: "CI_AddressSyndication_FeedPosition",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.DropColumn(
                name: "FeedPosition",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.RenameIndex(
                name: "IX_AddressSyndication_Position",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                newName: "CI_AddressSyndication_Position");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressSyndication",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "Position")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
