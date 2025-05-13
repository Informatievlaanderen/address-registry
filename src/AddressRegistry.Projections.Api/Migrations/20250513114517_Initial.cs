using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AddressRegistry.Projections.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryApi");

            migrationBuilder.CreateTable(
                name: "AddressBoxNumberSyndicationHelper",
                schema: "AddressRegistryApi",
                columns: table => new
                {
                    PersistentLocalId = table.Column<int>(type: "integer", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    HouseNumber = table.Column<string>(type: "text", nullable: true),
                    BoxNumber = table.Column<string>(type: "text", nullable: true),
                    PointPosition = table.Column<byte[]>(type: "bytea", nullable: true),
                    PositionMethod = table.Column<int>(type: "integer", nullable: true),
                    PositionSpecification = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    IsOfficiallyAssigned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressBoxNumberSyndicationHelper", x => x.PersistentLocalId);
                });

            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "AddressRegistryApi",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "integer", nullable: false),
                    StreetNamePersistentLocalId = table.Column<int>(type: "integer", nullable: false),
                    ParentAddressPersistentLocalId = table.Column<int>(type: "integer", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    HouseNumber = table.Column<string>(type: "text", nullable: false),
                    BoxNumber = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<byte[]>(type: "bytea", nullable: false),
                    PositionMethod = table.Column<int>(type: "integer", nullable: false),
                    PositionSpecification = table.Column<int>(type: "integer", nullable: false),
                    Removed = table.Column<bool>(type: "boolean", nullable: false),
                    LastEventHash = table.Column<string>(type: "text", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressPersistentLocalId);
                });

            migrationBuilder.CreateTable(
                name: "AddressSyndication",
                schema: "AddressRegistryApi",
                columns: table => new
                {
                    FeedPosition = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: true),
                    StreetNameId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "integer", nullable: true),
                    StreetNamePersistentLocalId = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    HouseNumber = table.Column<string>(type: "text", nullable: true),
                    BoxNumber = table.Column<string>(type: "text", nullable: true),
                    PointPosition = table.Column<byte[]>(type: "bytea", nullable: true),
                    PositionMethod = table.Column<int>(type: "integer", nullable: true),
                    PositionSpecification = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    IsOfficiallyAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Application = table.Column<int>(type: "integer", nullable: true),
                    Modification = table.Column<int>(type: "integer", nullable: true),
                    Operator = table.Column<string>(type: "text", nullable: true),
                    Organisation = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    EventDataAsXml = table.Column<string>(type: "text", nullable: true),
                    SyndicationItemCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressSyndication", x => x.FeedPosition);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryApi",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "text", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_BoxNumber",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_HouseNumber",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PostalCode",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Status",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_StreetNamePersistentLocalId",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_VersionTimestamp",
                schema: "AddressRegistryApi",
                table: "AddressDetails",
                column: "VersionTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_AddressId",
                schema: "AddressRegistryApi",
                table: "AddressSyndication",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_FeedPosition",
                schema: "AddressRegistryApi",
                table: "AddressSyndication",
                column: "FeedPosition")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_PersistentLocalId",
                schema: "AddressRegistryApi",
                table: "AddressSyndication",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_Position",
                schema: "AddressRegistryApi",
                table: "AddressSyndication",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressBoxNumberSyndicationHelper",
                schema: "AddressRegistryApi");

            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "AddressRegistryApi");

            migrationBuilder.DropTable(
                name: "AddressSyndication",
                schema: "AddressRegistryApi");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryApi");
        }
    }
}
