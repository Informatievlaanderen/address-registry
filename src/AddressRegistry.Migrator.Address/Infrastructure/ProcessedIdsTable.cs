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

        private const string ProcessedIdsTableName = "ProcessedIds";
        private const string Table = $"[{Schema.MigrateAddress}].[{ProcessedIdsTableName}]";

        public ProcessedIdsTable(string connectionString, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _logger = loggerFactory.CreateLogger<ProcessedIdsTable>();
        }

        public async Task CreateTableIfNotExists()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(@$"
IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'{Schema.MigrateAddress}')
    EXEC('CREATE SCHEMA [{Schema.MigrateAddress}]');

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{ProcessedIdsTableName}' and xtype='U')
CREATE TABLE {Table}(
[Id] [int] NOT NULL,
[IsPageCompleted] [bit] NOT NULL DEFAULT 0,
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
                await conn.ExecuteAsync(@$"INSERT INTO {Table} VALUES ('{internalId}')");
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to add Id '{internalId}' to ProcessedIds table");
                throw;
            }
        }

        public async Task CompletePageAsync (IEnumerable<int> processedIds)
        {
            string query = $"UPDATE {Table} SET IsPageCompleted = 1 WHERE Id in (@processedIds)";

            await using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(query, new
            {
                processedIds
            });
        }

        public async Task<IEnumerable<(int processedId, bool isPageCompleted)>> GetProcessedIds()
        {
            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.QueryAsync<(int, bool)>($"SELECT Id, IsPageCompleted FROM {Table} ORDER BY Id desc");
            return result;
        }
    }
}
