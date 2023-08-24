namespace AddressRegistry.Snapshot.Verifier
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class SnapshotVerificationRepository
    {
        private readonly string _connectionString;
        private readonly Scripts _scripts;

        public SnapshotVerificationRepository(
            string connectionString,
            string schema,
            string tableName = "SnapshotVerificationStates")
        {
            _connectionString = connectionString;
            _scripts = new Scripts(schema, tableName);
        }

        public void EnsureCreated()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute(_scripts.GetInitial());
        }

        public async Task<int?> MaxSnapshotId(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int?>($"SELECT MAX(SnapshotId) FROM {_scripts.SchemaWithTableName}");
        }

        public async Task AddVerificationState(SnapshotVerificationState verificationState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                _scripts.InsertVerificationState(),
                new
                {
                    SnapshotId = verificationState.SnapshotId,
                    Status = verificationState.Status.ToString(),
                    Differences = verificationState.Differences
                });
        }
    }
}
