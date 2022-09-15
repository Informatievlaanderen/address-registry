using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wfs.Migrations
{
    public partial class AddIndexWithIncludes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressWfs_Removed_StreetNamePersistentLocalId",
                schema: "wfs.address",
                table: "AddressWfs",
                columns: new[] { "Removed", "StreetNamePersistentLocalId" })
                .Annotation("SqlServer:Include", new[] { "BoxNumber", "HouseNumber", "OfficiallyAssigned", "AddressPersistentLocalId", "Position", "PositionMethod", "PositionSpecification", "PostalCode", "Status", "VersionAsString" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete_StreetNameId",
                schema: "wfs.address",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete", "StreetNameId" })
                .Annotation("SqlServer:Include", new[] { "BoxNumber", "HouseNumber", "OfficiallyAssigned", "PersistentLocalId", "Position", "PositionMethod", "PositionSpecification", "PostalCode", "Status", "VersionAsString" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressWfs_Removed_StreetNamePersistentLocalId",
                schema: "wfs.address",
                table: "AddressWfs");

            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_Removed_Complete_StreetNameId",
                schema: "wfs.address",
                table: "AddressDetails");
        }
    }
}
