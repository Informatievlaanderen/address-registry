namespace AddressRegistry.Api.CrabImport.Migrations
{
    using AddressRegistry.Infrastructure;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddSequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
              CREATE SEQUENCE {Schema.Sequence}.{SequenceContext.AddressPersistentLocalIdSequenceName}
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
            migrationBuilder.Sql($"DROP SEQUENCE {Schema.Sequence}.{SequenceContext.AddressPersistentLocalIdSequenceName};");
        }
    }
}
