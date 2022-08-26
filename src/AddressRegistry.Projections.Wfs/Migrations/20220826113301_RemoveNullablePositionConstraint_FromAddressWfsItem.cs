using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    public partial class RemoveNullablePositionConstraint_FromAddressWfsItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PositionSpecification",
                schema: "wfs.address",
                table: "AddressWfs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PositionMethod",
                schema: "wfs.address",
                table: "AddressWfs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Point>(
                name: "Position",
                schema: "wfs.address",
                table: "AddressWfs",
                type: "sys.geometry",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "sys.geometry",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PositionSpecification",
                schema: "wfs.address",
                table: "AddressWfs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PositionMethod",
                schema: "wfs.address",
                table: "AddressWfs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Point>(
                name: "Position",
                schema: "wfs.address",
                table: "AddressWfs",
                type: "sys.geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "sys.geometry");
        }
    }
}
