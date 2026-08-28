namespace AddressRegistry.Migrator.Lambert2008.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Reads the street name streams to convert, paged on the streams table's own internal id so the
    /// conversion can be resumed where it left off.
    /// </summary>
    internal sealed class SqlStreamsTable
    {
        private readonly string _connectionString;
        private readonly int _pageSize;

        public SqlStreamsTable(string connectionString, int pageSize = 500)
        {
            _connectionString = connectionString;
            _pageSize = pageSize;
        }

        public int PageSize => _pageSize;

        /// <summary>
        /// Every address lives in a street name stream since AddressRegistry.Migrator.Address moved them
        /// there, so the street name streams are the complete set of streams holding positions that are
        /// still projected. The legacy address streams are left untouched.
        /// </summary>
        public async Task<IEnumerable<(int internalId, string streamId)>> ReadNextStreetNameStreamPage(int lastCursorPosition)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<(int, string)>($"""
                                                               select top (@PageSize)
                                                                   [IdInternal]
                                                                   ,[IdOriginal]
                                                               from
                                                                   [{Schema.Default}].[Streams]
                                                               where
                                                                   IdOriginal like 'streetname-%'
                                                                   and IdInternal > @LastCursorPosition
                                                               order by
                                                                   IdInternal
                                                               """, new { PageSize = _pageSize, LastCursorPosition = lastCursorPosition }, commandTimeout: 60);
        }

        /// <summary>
        /// The total up front, so progress can be reported as a percentage with an estimate of the time
        /// left instead of an ever-growing count. One scan at startup, not per page.
        /// </summary>
        public async Task<int> CountStreetNameStreams(CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(new CommandDefinition($@"
select
    count(*)
from
    [{Schema.Default}].[Streams]
where
    IdOriginal like 'streetname-%'", commandTimeout: 300, cancellationToken: ct));
        }
    }
}
