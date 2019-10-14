using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddAddressDetailIndexesForBosa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_VersionTimestamp",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                column: "VersionTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_Status",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_VersionTimestamp",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HouseNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BoxNumber",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
