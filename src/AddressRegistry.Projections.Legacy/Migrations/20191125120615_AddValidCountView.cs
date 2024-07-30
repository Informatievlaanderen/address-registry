using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddValidCountView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[vw_AddressListCount]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[AddressList]
            WHERE [Complete] = 1 AND [Removed] = 0 AND [PersistentLocalId] <> 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_vw_AddressListCount ON [{Infrastructure.Schema.Legacy}].[vw_AddressListCount] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_vw_AddressListCount] ON [{Infrastructure.Schema.Legacy}].[vw_AddressListCount]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[vw_AddressListCount]");
        }
    }
}
