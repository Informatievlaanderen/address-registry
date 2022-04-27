using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddressRegistry.Projections.Legacy.Migrations
{
    public partial class AddListViewCountV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountNameV2}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{AddressListV2.AddressListItemV2Configuration.TableName}]
            WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.AddressListViewCountNameV2} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountNameV2}] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{LegacyContext.AddressListViewCountNameV2}] ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountNameV2}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.AddressListViewCountNameV2}]");
        }
    }
}
