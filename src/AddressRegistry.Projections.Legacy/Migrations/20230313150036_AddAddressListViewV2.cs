using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddAddressListViewV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryLegacy TO address");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryConsumerReadStreetName TO address");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryConsumerReadMunicipality TO address");

            migrationBuilder.Sql($@"
CREATE VIEW [AddressRegistryLegacy].[vw_AddressListCountV2]
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

            migrationBuilder.Sql($"CREATE UNIQUE CLUSTERED INDEX IX_vw_AddressListCountV2_AddressPersistentLocalId ON [AddressRegistryLegacy].vw_AddressListCountV2 (AddressPersistentLocalId);");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_HouseNumber",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_BoxNumber",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_PostalCode",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_Status",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                column: "Status");

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressListCountV2_MunicipalityNameSearch",
                    table: "vw_AddressListCountV2",
                    schema: "AddressRegistryLegacy",
                    columns: new[]
                    {
                        "MunicipalityNameDutchSearch",
                        "MunicipalityNameEnglishSearch",
                        "MunicipalityNameFrenchSearch",
                        "MunicipalityNameGermanSearch"
                    });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_NisCode",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_StreetNameDutchSearch",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameDutchSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_StreetNameEnglishSearch",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameEnglishSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_StreetNameFrenchSearch",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameFrenchSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_StreetNameGermanSearch",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                columns: new[] { "StreetNameGermanSearch" });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_StreetNamePersistentLocalId",
                table: "vw_AddressListCountV2",
                schema: "AddressRegistryLegacy",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressListCountV2_HomonymAdditions",
                table: "vw_AddressListCountV2",
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
            migrationBuilder.Sql($"DROP VIEW [AddressRegistryLegacy].vw_AddressListCountV2");
        }
    }
}
