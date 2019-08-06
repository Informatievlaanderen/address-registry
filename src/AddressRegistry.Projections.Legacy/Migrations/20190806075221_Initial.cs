using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "AddressDetails",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    OfficiallyAssigned = table.Column<bool>(nullable: true),
                    Position = table.Column<byte[]>(nullable: true),
                    PositionMethod = table.Column<int>(nullable: true),
                    PositionSpecification = table.Column<int>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressList",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressList", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "AddressSyndication",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    ChangeType = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    PointPosition = table.Column<byte[]>(nullable: true),
                    PositionMethod = table.Column<int>(nullable: true),
                    PositionSpecification = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsOfficiallyAssigned = table.Column<bool>(nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(nullable: false),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    EventDataAsXml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "AddressVersions",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    StreamPosition = table.Column<long>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    OfficiallyAssigned = table.Column<bool>(nullable: true),
                    Position = table.Column<byte[]>(nullable: true),
                    PositionMethod = table.Column<int>(nullable: true),
                    PositionSpecification = table.Column<int>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: true),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressVersions", x => new { x.AddressId, x.StreamPosition })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "CrabIdToPersistentLocalIds",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    HouseNumberId = table.Column<int>(nullable: true),
                    SubaddressId = table.Column<int>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    BoxNumber = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrabIdToPersistentLocalIds", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryLegacy",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressList_Complete_Removed",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                columns: new[] { "Complete", "Removed" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_AddressId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressSyndication_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressVersions_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_HouseNumberId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "HouseNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_IsRemoved",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_PersistentLocalId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "PersistentLocalId",
                unique: true,
                filter: "[PersistentLocalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CrabIdToPersistentLocalIds_SubaddressId",
                schema: "AddressRegistryLegacy",
                table: "CrabIdToPersistentLocalIds",
                column: "SubaddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetails",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressList",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressSyndication",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "AddressVersions",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "CrabIdToPersistentLocalIds",
                schema: "AddressRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryLegacy");
        }
    }
}
