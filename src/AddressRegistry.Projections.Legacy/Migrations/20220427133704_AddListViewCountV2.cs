using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddListViewCountV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[vw_AddressListCountV2]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[AddressListV2]
            WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_vw_AddressListCountV2 ON [{Infrastructure.Schema.Legacy}].[vw_AddressListCountV2] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_vw_AddressListCountV2] ON [{Infrastructure.Schema.Legacy}].[vw_AddressListCountV2]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[vw_AddressListCountV2]");
        }
    }
}
