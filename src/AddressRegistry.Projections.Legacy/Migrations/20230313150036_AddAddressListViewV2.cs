using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    using AddressListV2;

    public partial class AddAddressListViewV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryLegacy TO address");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryConsumerReadStreetName TO address");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryConsumerReadMunicipality TO address");

            migrationBuilder.Sql($@"
CREATE VIEW [AddressRegistryLegacy].{AddressListViewItemV2Configuration.ViewName}
WITH SCHEMABINDING
AS
  SELECT al.AddressPersistentLocalId
  ,al.[StreetNamePersistentLocalId]
  ,al.[PostalCode]
  ,al.[HouseNumber]
  ,al.[BoxNumber]
  ,al.[Status]
  ,al.[VersionTimestamp]
  ,s.NameDutch as StreetNameDutch
  ,s.NameDutchSearch as StreetNameDutchSearch
  ,s.NameEnglish as StreetNameEnglish
  ,s.NameEnglishSearch as StreetNameEnglishSearch
  ,s.NameFrench as StreetNameFrench
  ,s.NameFrenchSearch as StreetNameFrenchSearch
  ,s.NameGerman as StreetNameGerman
  ,s.NameGermanSearch as StreetNameGermanSearch
  ,s.HomonymAdditionDutch
  ,s.HomonymAdditionEnglish
  ,s.HomonymAdditionFrench
  ,s.HomonymAdditionGerman
  ,m.NisCode
  ,m.OfficialLanguages
  ,m.NameDutch as MunicipalityNameDutch
  ,m.NameDutchSearch as MunicipalityNameDutchSearch
  ,m.NameEnglish as MunicipalityNameEnglish
  ,m.NameEnglishSearch as MunicipalityNameEnglishSearch
  ,m.NameFrench as MunicipalityNameFrench
  ,m.NameFrenchSearch as MunicipalityNameFrenchSearch
  ,m.NameGerman as MunicipalityNameGerman
  ,m.NameGermanSearch as MunicipalityNameGermanSearch
FROM [AddressRegistryLegacy].[AddressListV2] al
INNER JOIN [AddressRegistryConsumerReadStreetName].LatestItems s
	ON al.StreetNamePersistentLocalId = s.PersistentLocalId AND s.IsRemoved = 0
INNER JOIN [AddressRegistryConsumerReadMunicipality].LatestItems m
	ON s.NisCode = m.NisCode
WHERE al.Removed = 0");

            migrationBuilder.Sql($"CREATE UNIQUE CLUSTERED INDEX IX_{AddressListViewItemV2Configuration.ViewName}_AddressPersistentLocalId ON [AddressRegistryLegacy].{AddressListViewItemV2Configuration.ViewName} (AddressPersistentLocalId);");

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_HouseNumber",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_BoxNumber",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_PostalCode",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_Status",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                column: "Status");

            migrationBuilder.CreateIndex(
                    name: $"IX_{AddressListViewItemV2Configuration.ViewName}_MunicipalityNameSearch",
                    table: AddressListViewItemV2Configuration.ViewName,
                    schema: "AddressRegistryLegacy",
                    columns: new[]
                    {
                        "MunicipalityNameDutchSearch",
                        "MunicipalityNameEnglishSearch",
                        "MunicipalityNameFrenchSearch",
                        "MunicipalityNameGermanSearch"
                    });

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_NisCode",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_StreetNameDutchSearch",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameDutchSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_StreetNameEnglishSearch",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameEnglishSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_StreetNameFrenchSearch",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameFrenchSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_StreetNameGermanSearch",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameGermanSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_StreetNamePersistentLocalId",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                name: $"IX_{AddressListViewItemV2Configuration.ViewName}_HomonymAdditions",
                table: AddressListViewItemV2Configuration.ViewName,
                schema: "AddressRegistryLegacy",
                columns: new[]
                {
                    "HomonymAdditionDutch",
                    "HomonymAdditionEnglish",
                    "HomonymAdditionFrench",
                    "HomonymAdditionGerman"
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP VIEW [AddressRegistryLegacy].{AddressListViewItemV2Configuration.ViewName}");
        }
    }
}
