using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Syndication.Migrations
{
    public partial class AddPostalInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostalInfoLatestSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    PostalCode = table.Column<string>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInfoLatestSyndication", x => x.PostalCode)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PostalInfoPostalNamesLatestSyndication",
                schema: "AddressRegistrySyndication",
                columns: table => new
                {
                    PostalCode = table.Column<string>(nullable: false),
                    PostalName = table.Column<string>(nullable: false),
                    Language = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInfoPostalNamesLatestSyndication", x => new { x.PostalCode, x.PostalName })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PostalInfoPostalNamesLatestSyndication_PostalInfoLatestSyndication_PostalCode",
                        column: x => x.PostalCode,
                        principalSchema: "AddressRegistrySyndication",
                        principalTable: "PostalInfoLatestSyndication",
                        principalColumn: "PostalCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostalInfoLatestSyndication_NisCode",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoLatestSyndication",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_PostalInfoPostalNamesLatestSyndication_PostalName",
                schema: "AddressRegistrySyndication",
                table: "PostalInfoPostalNamesLatestSyndication",
                column: "PostalName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalInfoPostalNamesLatestSyndication",
                schema: "AddressRegistrySyndication");

            migrationBuilder.DropTable(
                name: "PostalInfoLatestSyndication",
                schema: "AddressRegistrySyndication");
        }
    }
}
