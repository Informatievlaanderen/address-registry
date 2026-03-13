using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Feed.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryFeed");

            migrationBuilder.CreateSequence(
                name: "AddressFeedSequence",
                schema: "AddressRegistryFeed");

            migrationBuilder.CreateTable(
                name: "AddressDocuments",
                schema: "AddressRegistryFeed",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDocuments", x => x.PersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressFeed",
                schema: "AddressRegistryFeed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudEvent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressFeed", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressFeedItemAddresses",
                schema: "AddressRegistryFeed",
                columns: table => new
                {
                    FeedItemId = table.Column<long>(type: "bigint", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressFeedItemAddresses", x => new { x.FeedItemId, x.AddressPersistentLocalId });
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryFeed",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressFeed_Page",
                schema: "AddressRegistryFeed",
                table: "AddressFeed",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_AddressFeed_Position",
                schema: "AddressRegistryFeed",
                table: "AddressFeed",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_AddressFeedItemAddresses_AddressPersistentLocalId",
                schema: "AddressRegistryFeed",
                table: "AddressFeedItemAddresses",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressFeedItemAddresses_FeedItemId",
                schema: "AddressRegistryFeed",
                table: "AddressFeedItemAddresses",
                column: "FeedItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDocuments",
                schema: "AddressRegistryFeed");

            migrationBuilder.DropTable(
                name: "AddressFeed",
                schema: "AddressRegistryFeed");

            migrationBuilder.DropTable(
                name: "AddressFeedItemAddresses",
                schema: "AddressRegistryFeed");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryFeed");

            migrationBuilder.DropSequence(
                name: "AddressFeedSequence",
                schema: "AddressRegistryFeed");
        }
    }
}
