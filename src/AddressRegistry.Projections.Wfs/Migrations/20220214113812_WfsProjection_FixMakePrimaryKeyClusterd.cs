using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Wfs.Migrations
{
    public partial class WfsProjection_FixMakePrimaryKeyClusterd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressDetails",
                schema: "wfs.address",
                table: "AddressDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressDetails",
                schema: "wfs.address",
                table: "AddressDetails",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressDetails",
                schema: "wfs.address",
                table: "AddressDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressDetails",
                schema: "wfs.address",
                table: "AddressDetails",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
