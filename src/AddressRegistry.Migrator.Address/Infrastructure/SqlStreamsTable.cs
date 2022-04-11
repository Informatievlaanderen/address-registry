namespace AddressRegistry.Migrator.Address.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class SqlStreamsTable
    {
        private readonly string _connectionString;
        private readonly int _pageSize;

        public SqlStreamsTable(string connectionString, int pageSize = 500)
        {
            _connectionString = connectionString;
            _pageSize = pageSize;
        }

        public async Task<IEnumerable<(int internalId, string aggregateId)>?> ReadNextAddressStreamPage(int lastCursorPosition)
        {
            await using var conn = new SqlConnection(_connectionString);

            return await conn.QueryAsync<(int, string)>($@"
select top ({_pageSize}) 
	[IdInternal]
    ,[IdOriginal]
from
    [{Schema.Default}].[Streams]
where
    IdOriginal not like 'streetname-%'
    and IdInternal > {lastCursorPosition}
order by
    IdInternal", commandTimeout: 60);
        }
    }
}
