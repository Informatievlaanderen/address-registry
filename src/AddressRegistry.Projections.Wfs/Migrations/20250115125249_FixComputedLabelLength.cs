using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class FixComputedLabelLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HouseNumberLabelLength",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "int",
                nullable: false,
                computedColumnSql: "CAST(LEN(ISNULL(HouseNumberLabel, '')) AS INT)",
                stored: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "LEN(HouseNumberLabel)",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HouseNumberLabelLength",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "int",
                nullable: false,
                computedColumnSql: "LEN(HouseNumberLabel)",
                stored: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "CAST(LEN(ISNULL(HouseNumberLabel, '')) AS INT)",
                oldStored: true);
        }
    }
}
