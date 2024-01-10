using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_address");

            migrationBuilder.CreateTable(
                name: "address_latest_items",
                schema: "integration_address",
                columns: table => new
                {
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    street_name_persistent_local_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: true),
                    house_number = table.Column<string>(type: "text", nullable: true),
                    box_number = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: true),
                    position_method = table.Column<int>(type: "integer", nullable: false),
                    oslo_position_method = table.Column<string>(type: "text", nullable: true),
                    position_specification = table.Column<int>(type: "integer", nullable: false),
                    oslo_position_specification = table.Column<string>(type: "text", nullable: true),
                    officially_assigned = table.Column<bool>(type: "boolean", nullable: true),
                    removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: true),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: true),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    idempotence_key = table.Column<long>(type: "bigint", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_as_string = table.Column<string>(type: "text", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address_latest_items", x => x.persistent_local_id);
                });

            migrationBuilder.CreateTable(
                name: "address_versions",
                schema: "integration_address",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    street_name_persistent_local_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    oslo_status = table.Column<string>(type: "text", nullable: true),
                    house_number = table.Column<string>(type: "text", nullable: true),
                    box_number = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: true),
                    position_method = table.Column<int>(type: "integer", nullable: true),
                    oslo_position_method = table.Column<string>(type: "text", nullable: true),
                    position_specification = table.Column<int>(type: "integer", nullable: true),
                    oslo_position_specification = table.Column<string>(type: "text", nullable: true),
                    officially_assigned = table.Column<bool>(type: "boolean", nullable: true),
                    removed = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: true),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: true),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_as_string = table.Column<string>(type: "text", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address_versions", x => new { x.position, x.persistent_local_id });
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_address",
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
                name: "IX_address_latest_items_box_number",
                schema: "integration_address",
                table: "address_latest_items",
                column: "box_number");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_geometry",
                schema: "integration_address",
                table: "address_latest_items",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_house_number",
                schema: "integration_address",
                table: "address_latest_items",
                column: "house_number");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_oslo_status",
                schema: "integration_address",
                table: "address_latest_items",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_postal_code",
                schema: "integration_address",
                table: "address_latest_items",
                column: "postal_code");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_removed",
                schema: "integration_address",
                table: "address_latest_items",
                column: "removed");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_status",
                schema: "integration_address",
                table: "address_latest_items",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_street_name_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items",
                column: "street_name_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_address_id",
                schema: "integration_address",
                table: "address_versions",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_box_number",
                schema: "integration_address",
                table: "address_versions",
                column: "box_number");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_geometry",
                schema: "integration_address",
                table: "address_versions",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_house_number",
                schema: "integration_address",
                table: "address_versions",
                column: "house_number");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_oslo_status",
                schema: "integration_address",
                table: "address_versions",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_persistent_local_id",
                schema: "integration_address",
                table: "address_versions",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_postal_code",
                schema: "integration_address",
                table: "address_versions",
                column: "postal_code");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_removed",
                schema: "integration_address",
                table: "address_versions",
                column: "removed");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_status",
                schema: "integration_address",
                table: "address_versions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_address_versions_street_name_persistent_local_id",
                schema: "integration_address",
                table: "address_versions",
                column: "street_name_persistent_local_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_latest_items",
                schema: "integration_address");

            migrationBuilder.DropTable(
                name: "address_versions",
                schema: "integration_address");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_address");
        }
    }
}
