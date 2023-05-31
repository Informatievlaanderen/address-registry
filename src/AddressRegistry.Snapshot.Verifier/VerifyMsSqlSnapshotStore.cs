namespace AddressRegistry.Snapshot.Verifier
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Dapper;

    public sealed class MsSqlSnapshotStoreVerifier
    {
        private const string TableName = "Snapshots";

        private readonly string _snapshotStoreConnectionString;
        private readonly string _schema;

        private string SchemaWithTableName => $"{_schema}.{TableName}";

        public MsSqlSnapshotStoreVerifier(string snapshotStoreConnectionString, string schema)
        {
            _snapshotStoreConnectionString = snapshotStoreConnectionString;
            _schema = schema;
        }

        public async Task<bool> DoesTableExist()
        {
            await using var connection = new SqlConnection(_snapshotStoreConnectionString);
            var result = await connection.ExecuteScalarAsync<int>($"IF object_id('{SchemaWithTableName}', 'U') IS NULL SELECT 0 ELSE SELECT 1");

            return result == 1;
        }

        public async Task<IEnumerable<SnapshotIdentifier>?> GetSnapshotIdsToVerify(int? lastProcessedId)
        {
            await using var connection = new SqlConnection(_snapshotStoreConnectionString);
            if (lastProcessedId is null)
            {
                return await connection.QueryAsync<SnapshotIdentifier>($"SELECT Id as SnapshotId, StreamId FROM {SchemaWithTableName}");
            }

            return await connection.QueryAsync<SnapshotIdentifier>($"SELECT Id as SnapshotId, StreamId FROM {SchemaWithTableName} WHERE Id > @Id", new { Id = lastProcessedId });
        }

        public async Task<string?> GetSnapshotBlob(int snapshotId)
        {
            await using var connection = new SqlConnection(_snapshotStoreConnectionString);
            var commandDefinition = new CommandDefinition($"SELECT SnapshotBlob FROM {SchemaWithTableName} WHERE Id = @Id", new { Id = snapshotId });
            var reader = await connection.ExecuteReaderAsync(commandDefinition, CommandBehavior.SequentialAccess);

            if (!reader.HasRows)
                return null;

            await reader.ReadAsync();
            return await reader.GetTextReader(0).ReadToEndAsync();
        }
    }

    public record SnapshotIdentifier(int SnapshotId, string StreamId);
}
