using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Api.CrabImport.Migrations
{
    using AddressRegistry.Infrastructure;
    using Infrastructure;

    public partial class AddOsloSequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
              CREATE SEQUENCE {Schema.Sequence}.{SequenceContext.AddressOsloIdSequenceName}
                AS int
                START WITH 30000000
                INCREMENT BY 1
	            MINVALUE 30000000
                MAXVALUE 999999999
                NO CYCLE
                NO CACHE
            ;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP SEQUENCE {Schema.Sequence}.{SequenceContext.AddressOsloIdSequenceName};");
        }
    }
}
