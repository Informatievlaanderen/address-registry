namespace AddressRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Bookkeeping of the street name streams that have been converted, so a restart resumes instead of
    /// starting over. Lives in its own schema next to the event store, in the same database, so a stream
    /// and its bookkeeping cannot drift apart across databases.
    /// </summary>
    /// <remarks>
    /// A dry run records its own rows and reads only its own: it dispatches nothing, so letting it advance
    /// the watermark a real run resumes from would make that run skip every stream the dry run measured and
    /// transform nothing. Both modes still keep their timings, which is what the table is for.
    /// </remarks>
    internal sealed class ProcessedStreamsTable
    {
        private const string TableName = "ProcessedStreams";
        private const string Table = $"[{Schema.MigrateLambert2008}].[{TableName}]";

        private readonly string _connectionString;
        private readonly bool _dryRun;
        private readonly ILogger<ProcessedStreamsTable> _logger;

        public ProcessedStreamsTable(string connectionString, bool dryRun, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _dryRun = dryRun;
            _logger = loggerFactory.CreateLogger<ProcessedStreamsTable>();
        }

        public async Task CreateTableIfNotExists(CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(new CommandDefinition($@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{Schema.MigrateLambert2008}')
    EXEC('CREATE SCHEMA [{Schema.MigrateLambert2008}]');

IF OBJECT_ID(N'{Table}', N'U') IS NULL
CREATE TABLE {Table}(
    [Id] [int] NOT NULL,
    [IsDryRun] [bit] NOT NULL,
    [StreetNamePersistentLocalId] [int] NOT NULL,
    [AddressCount] [int] NOT NULL,
    [ConvertedAddresses] [int] NOT NULL,
    [LoadMilliseconds] [int] NOT NULL,
    [DispatchMilliseconds] [int] NOT NULL,
    [IsPageCompleted] [bit] NOT NULL DEFAULT 0,
    [ProcessedAt] [datetimeoffset](7) NOT NULL,
    CONSTRAINT [PK_Lambert2008ProcessedStreams] PRIMARY KEY CLUSTERED ([Id] ASC, [IsDryRun] ASC)
)", cancellationToken: ct));
        }

        /// <summary>
        /// The per-stream timings are written here rather than only logged, so a run can be queried
        /// afterwards — cost against address count, the slowest streams, percentiles — instead of having to be
        /// reconstructed from log lines.
        /// </summary>
        /// <remarks>
        /// Deliberately not cancellable: this records work the event store has already accepted, and losing
        /// the row on a Ctrl-C would leave a converted stream looking unconverted.
        /// </remarks>
        public async Task Add(
            int internalId,
            int streetNamePersistentLocalId,
            StreamConversionResult result)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    $@"INSERT INTO {Table} (Id, IsDryRun, StreetNamePersistentLocalId, AddressCount, ConvertedAddresses, LoadMilliseconds, DispatchMilliseconds, IsPageCompleted, ProcessedAt)
                       VALUES (@Id, @IsDryRun, @StreetNamePersistentLocalId, @AddressCount, @ConvertedAddresses, @LoadMilliseconds, @DispatchMilliseconds, 0, SYSDATETIMEOFFSET())",
                    new
                    {
                        Id = internalId,
                        IsDryRun = _dryRun,
                        StreetNamePersistentLocalId = streetNamePersistentLocalId,
                        result.AddressCount,
                        result.ConvertedAddresses,
                        LoadMilliseconds = (int)result.LoadDuration.TotalMilliseconds,
                        DispatchMilliseconds = (int)result.DispatchDuration.TotalMilliseconds
                    });
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Failed to add id '{InternalId}' to {Table}.", internalId, Table);
                throw;
            }
        }

        /// <summary>
        /// Marks a page done once every stream in it is recorded, which is what makes <see cref="GetResumeCursor"/>
        /// a watermark rather than a guess: streams within a page are processed in parallel, so a recorded
        /// high id says nothing about the ids below it, but a completed page does.
        /// </summary>
        public async Task CompletePage(IEnumerable<int> internalIds)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                $"UPDATE {Table} SET IsPageCompleted = 1 WHERE Id IN @internalIds AND IsDryRun = @IsDryRun",
                new { internalIds = internalIds.ToArray(), IsDryRun = _dryRun });
        }

        /// <summary>How many streams earlier runs in this mode already recorded, so progress is reported over the whole job.</summary>
        public async Task<int> GetProcessedCount(CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {Table} WHERE IsDryRun = @IsDryRun",
                new { IsDryRun = _dryRun },
                cancellationToken: ct));
        }

        /// <summary>The highest internal id below which every stream is known to be done in this mode.</summary>
        public async Task<int> GetResumeCursor(CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int?>(new CommandDefinition(
                $"SELECT MAX(Id) FROM {Table} WHERE IsPageCompleted = 1 AND IsDryRun = @IsDryRun",
                new { IsDryRun = _dryRun },
                cancellationToken: ct)) ?? 0;
        }

        /// <summary>
        /// The streams recorded past the resume cursor — the tail of a page that was interrupted — so they are
        /// skipped rather than converted twice.
        /// </summary>
        public async Task<IEnumerable<int>> GetProcessedIdsAfter(int cursor, CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<int>(new CommandDefinition(
                $"SELECT Id FROM {Table} WHERE Id > @cursor AND IsDryRun = @IsDryRun",
                new { cursor, IsDryRun = _dryRun },
                cancellationToken: ct));
        }
    }
}
