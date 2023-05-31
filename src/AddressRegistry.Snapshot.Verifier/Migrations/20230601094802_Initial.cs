using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Snapshot.Verifier.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressRegistry");

            migrationBuilder.CreateTable(
                name: "SnapshotVerificationStates",
                schema: "AddressRegistry",
                columns: table => new
                {
                    SnapshotId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Differences = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotVerificationStates", x => x.SnapshotId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotVerificationStates_Status",
                schema: "AddressRegistry",
                table: "SnapshotVerificationStates",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SnapshotVerificationStates",
                schema: "AddressRegistry");
        }
    }
}
