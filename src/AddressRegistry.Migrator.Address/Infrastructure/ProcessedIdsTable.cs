namespace AddressRegistry.Migrator.Address.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;

    public class ProcessedIdsTable
    {
        private readonly string _connectionString;
        private readonly ILogger<ProcessedIdsTable> _logger;

        public ProcessedIdsTable(string connectionString, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _logger = loggerFactory.CreateLogger<ProcessedIdsTable>();
        }

        private const string ProcessedIdsTableName = "ProcessedIds";

        public async Task CreateTableIfNotExists()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(@$"
IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'{Schema.MigrateAddress}')
    EXEC('CREATE SCHEMA [{Schema.MigrateAddress}]');

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{ProcessedIdsTableName}' and xtype='U')
CREATE TABLE [{Schema.MigrateAddress}].[{ProcessedIdsTableName}](
[Id] [int] NOT NULL,
CONSTRAINT [PK_ProcessedIds] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))");
        }

        public async Task Add(int internalId)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.ExecuteAsync(@$"INSERT INTO [{Schema.MigrateAddress}].[{ProcessedIdsTableName}] VALUES ('{internalId}')");
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to add Id '{internalId}' to ProcessedIds table");
                throw;
            }
        }

        public async Task<IEnumerable<int>?> GetProcessedIds()
        {
            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.QueryAsync<int>($"SELECT Id FROM [{Schema.MigrateAddress}].[{ProcessedIdsTableName}]");
            return result;
        }
    }
}
