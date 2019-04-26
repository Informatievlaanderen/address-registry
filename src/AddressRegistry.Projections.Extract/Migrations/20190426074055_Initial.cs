using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistryExtract");

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    AddressOsloId = table.Column<int>(nullable: false),
                    Complete = table.Column<bool>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    MinimumX = table.Column<double>(nullable: false),
                    MaximumX = table.Column<double>(nullable: false),
                    MinimumY = table.Column<double>(nullable: false),
                    MaximumY = table.Column<double>(nullable: false),
                    StreetNameId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "AddressRegistryExtract",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_NisCode",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_Address_StreetNameId",
                schema: "AddressRegistryExtract",
                table: "Address",
                column: "StreetNameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address",
                schema: "AddressRegistryExtract");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "AddressRegistryExtract");
        }
    }
}
