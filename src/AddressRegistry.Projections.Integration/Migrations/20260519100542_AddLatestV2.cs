using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddLatestV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address_latest_items_v2",
                schema: "integration_address",
                columns: table => new
                {
                    persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    street_name_persistent_local_id = table.Column<int>(type: "integer", nullable: true),
                    parent_persistent_local_id = table.Column<int>(type: "integer", nullable: true),
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
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address_latest_items_v2", x => x.persistent_local_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_box_number",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "box_number");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_geometry",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_house_number",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "house_number");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_oslo_status",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_parent_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "parent_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_postal_code",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "postal_code");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_removed",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "removed");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_removed_status",
                schema: "integration_address",
                table: "address_latest_items_v2",
                columns: new[] { "removed", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_status",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_address_latest_items_v2_street_name_persistent_local_id",
                schema: "integration_address",
                table: "address_latest_items_v2",
                column: "street_name_persistent_local_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_latest_items_v2",
                schema: "integration_address");
        }
    }
}
