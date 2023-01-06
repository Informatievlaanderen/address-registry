using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddIndexOnAddressParcelLinksExtractWithIncludeParcelPersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressParcelLinksExtract_IsParcelRemoved_IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_IsParcelRemoved_IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                columns: new[] { "IsParcelRemoved", "IsAddressLinkRemoved" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "ParcelId", "ParcelPersistentLocalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressParcelLinksExtract_IsParcelRemoved_IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract");

            migrationBuilder.CreateIndex(
                name: "IX_AddressParcelLinksExtract_IsParcelRemoved_IsAddressLinkRemoved",
                schema: "AddressRegistrySyndication",
                table: "AddressParcelLinksExtract",
                columns: new[] { "IsParcelRemoved", "IsAddressLinkRemoved" })
                .Annotation("SqlServer:Include", new[] { "AddressId", "ParcelId" });
        }
    }
}
