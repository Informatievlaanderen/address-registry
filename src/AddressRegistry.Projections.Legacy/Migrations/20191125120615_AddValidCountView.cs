using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddValidCountView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{AddressList.AddressListItemConfiguration.TableName}]
            WHERE [Complete] = 1 AND [Removed] = 0 AND [PersistentLocalId] <> 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.AddressListViewCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountName}] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{LegacyContext.AddressListViewCountName}] ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountName}]");
        }
    }
}
