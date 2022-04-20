using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    public partial class RecreateIndex_CompleteRemovedStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "wms.address",
                table: "AddressDetails");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete_Status",
                schema: "wms.address",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNameId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressDetails_Removed_Complete_Status",
                schema: "wms.address",
                table: "AddressDetails");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDetails_Removed_Complete",
                schema: "wms.address",
                table: "AddressDetails",
                columns: new[] { "Removed", "Complete" });
        }
    }
}
