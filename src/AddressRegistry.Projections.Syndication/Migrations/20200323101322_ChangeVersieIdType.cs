using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class ChangeVersieIdType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication");

            migrationBuilder.DropIndex(
                name: "IX_MunicipalityBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa");

            migrationBuilder.DropIndex(
                name: "IX_StreetNameBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosa");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosa",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalitySyndication_Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_StreetNameBosa_Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosa",
                column: "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameSyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameLatestSyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "StreetNameBosa",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalitySyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "AddressRegistrySyndication",
                table: "MunicipalityBosa",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
