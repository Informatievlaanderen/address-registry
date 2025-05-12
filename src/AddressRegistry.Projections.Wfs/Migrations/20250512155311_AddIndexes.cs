using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresView', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresView]
");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HouseNumber",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BoxNumber",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV2_BoxNumber",
                schema: "wfs.address",
                table: "AddressWfsV2",
                column: "BoxNumber",
                filter: "[BoxNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV2_HouseNumber",
                schema: "wfs.address",
                table: "AddressWfsV2",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV2_PostalCode",
                schema: "wfs.address",
                table: "AddressWfsV2",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWfsV2_Status",
                schema: "wfs.address",
                table: "AddressWfsV2",
                column: "Status");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresView] WITH SCHEMABINDING AS
SELECT a.[AddressPersistentLocalId] as ObjectId
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[AddressPersistentLocalId]) as 'Id'
      ,a.[VersionAsString] as 'VersieId'
      ,s.PersistentLocalId as 'StraatnaamObjectId'
      ,s.NameDutch as 'Straatnaam'
      ,s.[NisCode] as 'GemeenteObjectId'
      ,m.NameDutch as 'Gemeentenaam'
      ,a.[PostalCode] as 'PostinfoObjectId'
      ,CASE
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NULL THEN CONCAT(s.NameDutch, ' ',  a.[HouseNumber], ', ', a.[PostalCode], ' ', m.NameDutch)
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NOT NULL THEN CONCAT(s.NameDutch, ' ',  a.[HouseNumber], ' bus ', a.[BoxNumber] ,', ', a.[PostalCode], ' ', m.NameDutch)
        ELSE NULL
      END AS 'VolledigAdres'
      ,a.[HouseNumber] as 'Huisnummer'
      ,a.[BoxNumber] as 'Busnummer'
      ,a.[HouseNumberLabel] as 'HuisnummerLabel'
      ,a.[HouseNumberLabelLength] as 'HuisnummerLabelLengte'
      ,a.[LabelType] as 'LabelType'
      ,a.[Status] as 'AdresStatus'
      ,a.[Position] as 'AdresPositie'
      ,a.[PositionMethod]  as 'PositieGeometrieMethode'
      ,a.[PositionSpecification] as 'PositieSpecificatie'
      ,a.[OfficiallyAssigned] as 'OfficieelToegekend'
FROM [wfs.address].[AddressWfsV2] as a
JOIN [wfs.streetname].[StreetNameHelperV2] s ON a.StreetNamePersistentLocalId = s.PersistentLocalId AND a.[Removed] = 0
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0;
");

            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresView_ObjectId ON [wfs].[AdresView] ([ObjectId])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('wfs.AdresView', 'V') IS NOT NULL
    DROP VIEW [wfs].[AdresView]
");

            migrationBuilder.DropIndex(
                name: "IX_AddressWfsV2_BoxNumber",
                schema: "wfs.address",
                table: "AddressWfsV2");

            migrationBuilder.DropIndex(
                name: "IX_AddressWfsV2_HouseNumber",
                schema: "wfs.address",
                table: "AddressWfsV2");

            migrationBuilder.DropIndex(
                name: "IX_AddressWfsV2_PostalCode",
                schema: "wfs.address",
                table: "AddressWfsV2");

            migrationBuilder.DropIndex(
                name: "IX_AddressWfsV2_Status",
                schema: "wfs.address",
                table: "AddressWfsV2");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HouseNumber",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "BoxNumber",
                schema: "wfs.address",
                table: "AddressWfsV2",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[AdresView] WITH SCHEMABINDING AS
SELECT a.[AddressPersistentLocalId] as ObjectId
      ,CONCAT('https://data.vlaanderen.be/id/adres/',a.[AddressPersistentLocalId]) as 'Id'
      ,a.[VersionAsString] as 'VersieId'
      ,s.PersistentLocalId as 'StraatnaamObjectId'
      ,s.NameDutch as 'Straatnaam'
      ,s.[NisCode] as 'GemeenteObjectId'
      ,m.NameDutch as 'Gemeentenaam'
      ,a.[PostalCode] as 'PostinfoObjectId'
      ,CASE
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NULL THEN CONCAT(s.NameDutch, ' ',  a.[HouseNumber], ', ', a.[PostalCode], ' ', m.NameDutch)
        WHEN s.NameDutch IS NOT NULL AND a.[BoxNumber] IS NOT NULL THEN CONCAT(s.NameDutch, ' ',  a.[HouseNumber], ' bus ', a.[BoxNumber] ,', ', a.[PostalCode], ' ', m.NameDutch)
        ELSE NULL
      END AS 'VolledigAdres'
      ,a.[HouseNumber] as 'Huisnummer'
      ,a.[BoxNumber] as 'Busnummer'
      ,a.[HouseNumberLabel] as 'HuisnummerLabel'
      ,a.[HouseNumberLabelLength] as 'HuisnummerLabelLengte'
      ,a.[LabelType] as 'LabelType'
      ,a.[Status] as 'AdresStatus'
      ,a.[Position] as 'AdresPositie'
      ,a.[PositionMethod]  as 'PositieGeometrieMethode'
      ,a.[PositionSpecification] as 'PositieSpecificatie'
      ,a.[OfficiallyAssigned] as 'OfficieelToegekend'
FROM [wfs.address].[AddressWfsV2] as a
JOIN [wfs.streetname].[StreetNameHelperV2] s ON a.StreetNamePersistentLocalId = s.PersistentLocalId AND a.[Removed] = 0
JOIN [wfs.municipality].[MunicipalityHelper] m ON s.[NisCode] = m.[NisCode] AND s.[Removed] = 0;
");

            migrationBuilder.Sql(@"CREATE UNIQUE CLUSTERED INDEX IX_AdresView_ObjectId ON [wfs].[AdresView] ([ObjectId])");
        }
    }
}
