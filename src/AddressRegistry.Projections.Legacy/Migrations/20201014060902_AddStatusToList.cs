using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Legacy.Migrations
{
    using AddressDetail;
    using AddressList;
    using Infrastructure;

    public partial class AddStatusToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "AddressRegistryLegacy",
                table: "AddressList",
                nullable: true);

            migrationBuilder.Sql(@$"MERGE INTO [{Schema.Legacy}].[{AddressListItemConfiguration.TableName}] list
                                        USING [{Schema.Legacy}].[{AddressDetailItemConfiguration.TableName}] detail
                                            ON list.[AddressId] = detail.[AddressId]
                                    WHEN MATCHED THEN
                                        UPDATE
                                            SET list.[Status] = detail.[Status];");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "AddressRegistryLegacy",
                table: "AddressList");
        }
    }
}
