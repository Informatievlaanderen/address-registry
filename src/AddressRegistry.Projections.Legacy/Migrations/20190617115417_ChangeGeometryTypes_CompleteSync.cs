using System;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class ChangeGeometryTypes_CompleteSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<IPoint>(
                name: "Position",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                type: "sys.geometry",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOfficiallyAssigned",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<IPoint>(
                name: "PointPosition",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                type: "sys.geometry",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionMethod",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionSpecification",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication",
                nullable: true);

            migrationBuilder.AlterColumn<IPoint>(
                name: "Position",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                type: "sys.geometry",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOfficiallyAssigned",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.DropColumn(
                name: "PointPosition",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.DropColumn(
                name: "PositionMethod",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.DropColumn(
                name: "PositionSpecification",
                schema: "AddressRegistryLegacy",
                table: "AddressSyndication");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Position",
                schema: "AddressRegistryLegacy",
                table: "AddressVersions",
                nullable: true,
                oldClrType: typeof(IPoint),
                oldType: "sys.geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Position",
                schema: "AddressRegistryLegacy",
                table: "AddressDetails",
                nullable: true,
                oldClrType: typeof(IPoint),
                oldType: "sys.geometry",
                oldNullable: true);
        }
    }
}
