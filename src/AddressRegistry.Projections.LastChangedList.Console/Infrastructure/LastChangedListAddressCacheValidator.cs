namespace AddressRegistry.Projections.LastChangedList.Console.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public sealed class LastChangedListAddressCacheValidator : ICacheValidator
    {
        private readonly string _connectionString;
        private readonly string _projectionName;

        public LastChangedListAddressCacheValidator(string connectionString, string projectionName)
        {
            _connectionString = connectionString;
            _projectionName = projectionName;
        }

        public async Task<bool> CanCache(long position, CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT [Position]
                          FROM [address-registry].[AddressRegistryLegacy].[ProjectionStates]
                          WHERE [Name] = @Name
                        ";

            var projectionPosition = await connection.ExecuteScalarAsync<int>(sql, new { Name = _projectionName });

            return projectionPosition >= position;
        }
    }
}
