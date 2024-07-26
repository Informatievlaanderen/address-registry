namespace AddressRegistry.Projector.Snapshots
{
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("snapshots")]
    public class SnapshotsController : ApiController
    {
        private const string SnapshotConnectionStringKey = "Events";
        private const string SnapshotVerificationsTableName = "[AddressRegistry].[SnapshotVerificationStates]";
        private const string SnaphotsTableName = "[AddressRegistry].[Snapshots]";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            await using var sqlConnection = new SqlConnection(configuration.GetConnectionString(SnapshotConnectionStringKey));
            var failedSnapshotCount =
                await sqlConnection.QuerySingleAsync<int>(
                    $"SELECT Count(*) FROM {SnapshotVerificationsTableName} WHERE Status = 'Failed'",
                    cancellationToken);

            var differenceInDaysOfLastVerification =
                await sqlConnection.QuerySingleAsync<int>(
                    $"SELECT DATEDIFF(DAY," +
                        $"(SELECT Created FROM {SnaphotsTableName} WHERE Id = (SELECT MAX(SnapshotId) FROM {SnapshotVerificationsTableName}))," +
                        $"(SELECT MAX(Created) FROM [AddressRegistry].[Snapshots])) As DaysDiff",
                    cancellationToken);

            return Ok(new SnapshotStatusResponse(failedSnapshotCount, differenceInDaysOfLastVerification));
        }
    }

    public sealed class SnapshotStatusResponse
    {
        public int FailedSnapshotsCount { get; set; }
        public int DifferenceInDaysOfLastVerification { get; set; }

        public SnapshotStatusResponse(int failedSnapshotsCount, int differenceInDaysOfLastVerification)
        {
            FailedSnapshotsCount = failedSnapshotsCount;
            DifferenceInDaysOfLastVerification = differenceInDaysOfLastVerification;
        }
    }
}
