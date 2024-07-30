using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    using Infrastructure;

    public partial class AddressListView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistryLegacy TO address");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::AddressRegistrySyndication TO address");

            migrationBuilder.Sql(@$"
CREATE VIEW [AddressRegistryLegacy].vw_AddressList
WITH SCHEMABINDING
AS
  SELECT al.[PersistentLocalId]
  ,al.[StreetNameId]
  ,al.[PostalCode]
  ,al.[HouseNumber]
  ,al.[BoxNumber]
  ,al.[Status]
  ,al.[VersionTimestamp]
  ,s.PersistentLocalId as StreetNamePersistentLocalId
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
  ,m.PrimaryLanguage
  ,m.NameDutch as MunicipalityNameDutch
  ,m.NameDutchSearch as MunicipalityNameDutchSearch
  ,m.NameEnglish as MunicipalityNameEnglish
  ,m.NameEnglishSearch as MunicipalityNameEnglishSearch
  ,m.NameFrench as MunicipalityNameFrench
  ,m.NameFrenchSearch as MunicipalityNameFrenchSearch
  ,m.NameGerman as MunicipalityNameGerman
  ,m.NameGermanSearch as MunicipalityNameGermanSearch
FROM [AddressRegistryLegacy].[AddressList] al
INNER JOIN AddressRegistrySyndication.StreetNameLatestSyndication s
	ON al.StreetNameId = s.StreetNameId AND s.IsComplete = 1 AND s.IsRemoved = 0
INNER JOIN AddressRegistrySyndication.MunicipalityLatestSyndication m
	ON s.NisCode = m.NisCode
WHERE al.Complete = 1 AND al.Removed = 0 AND al.PersistentLocalId <> 0 AND al.PersistentLocalId IS NOT NULL");

            migrationBuilder.Sql($"CREATE UNIQUE CLUSTERED INDEX IX_vw_AddressList_PersistentLocalId ON [AddressRegistryLegacy].vw_AddressList (PersistentLocalId);");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressList_HouseNumber",
                table: "vw_AddressList",
                schema: "AddressRegistryLegacy",
                column: "HouseNumber");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressList_BoxNumber",
                table: "vw_AddressList",
                schema: "AddressRegistryLegacy",
                column: "BoxNumber");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressList_PostalCode",
                table: "vw_AddressList",
                schema: "AddressRegistryLegacy",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressList_Status",
                table: "vw_AddressList",
                schema: "AddressRegistryLegacy",
                column: "Status");

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressList_MunicipalityNameSearch",
                    table: "vw_AddressList",
                    schema: "AddressRegistryLegacy",
                    columns: new[]
                    {
                        "MunicipalityNameDutchSearch",
                        "MunicipalityNameEnglishSearch",
                        "MunicipalityNameFrenchSearch",
                        "MunicipalityNameGermanSearch"
                    })
                .Annotation("SqlServer:Include", new[] { "NisCode" });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressList_NisCode",
                table: "vw_AddressList",
                schema: "AddressRegistryLegacy",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressList_StreetNameDutchSearch",
                    table: "vw_AddressList",
                    schema: "AddressRegistryLegacy",
                    columns: new[] { "StreetNameDutchSearch" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressList_StreetNameEnglishSearch",
                    table: "vw_AddressList",
                    schema: "AddressRegistryLegacy",
                    columns: new[] { "StreetNameEnglishSearch" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressList_StreetNameFrenchSearch",
                    table: "vw_AddressList",
                    schema: "AddressRegistryLegacy",
                    columns: new[] { "StreetNameFrenchSearch" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressList_StreetNameGermanSearch",
                    table: "vw_AddressList",
                    schema: "AddressRegistryLegacy",
                    columns: new[] { "StreetNameGermanSearch" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: $"IX_vw_AddressList_StreetNamePersistentLocalId",
                table: "vw_AddressList",
                schema: "AddressRegistryLegacy",
                column: "StreetNamePersistentLocalId");

            migrationBuilder.CreateIndex(
                    name: $"IX_vw_AddressList_HomonymAdditions",
                    table: "vw_AddressList",
                    schema: "AddressRegistryLegacy",
                    columns: new[]
                    {
                        "HomonymAdditionDutch",
                        "HomonymAdditionEnglish",
                        "HomonymAdditionFrench",
                        "HomonymAdditionGerman"
                    })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP VIEW [AddressRegistryLegacy].vw_AddressList");
        }
    }
}
