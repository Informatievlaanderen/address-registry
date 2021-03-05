using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Extract.Migrations
{
    public partial class SetDatabaseIsolationLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @DBName nvarchar(50), @SQLString nvarchar(200)\r\n" +
                                  "SET @DBName = db_name();\r\n" +
                                  "SET @SQLString = \'ALTER DATABASE [\' + @DBName + \'] SET ALLOW_SNAPSHOT_ISOLATION ON\'\r\n" +
                                  "EXEC( @SQLString )", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @DBName nvarchar(50), @SQLString nvarchar(200)\r\n" +
                                 "SET @DBName = db_name();\r\n" +
                                 "SET @SQLString = \'ALTER DATABASE [\' + @DBName + \'] SET ALLOW_SNAPSHOT_ISOLATION OFF\'\r\n" +
                                 "EXEC( @SQLString )", suppressTransaction: true);
        }
    }
}
