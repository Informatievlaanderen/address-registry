namespace AddressRegistry.Projector.BackOffice
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using SqlStreamStore;

    [ApiVersion("1.0")]
    [ApiRoute("backoffice")]
    public class BackOfficeController : ApiController
    {
        private const string? BackOfficeConnectionStringKey = "Events";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            [FromServices] IStreamStore streamStore,
            CancellationToken cancellationToken)
        {
            var streamPosition = await streamStore.ReadHeadPosition(cancellationToken);

            List<BackOfficeStatusResponse> response;
            await using (var sqlConnection = new SqlConnection(configuration.GetConnectionString(BackOfficeConnectionStringKey)))
            {
                var result =
                    await sqlConnection.QueryAsync<ProjectionStateItem>(
                        "SELECT * FROM [AddressRegistryBackOfficeProjections].[ProjectionStates] WHERE Name='AddressRegistry.Projections.BackOffice.BackOfficeProjections'", cancellationToken);

                response = result.Select(x => new BackOfficeStatusResponse(x, streamPosition)).ToList();
            }

            return Ok(response);
        }
    }

    public class BackOfficeStatusResponse
    {
        public long MaxPosition { get; set; }
        public string ProjectionName { get; set; }
        public long Position { get; set; }

        public BackOfficeStatusResponse(ProjectionStateItem projectionStateItem, long maxPosition)
        {
            MaxPosition = maxPosition;
            ProjectionName = projectionStateItem.Name;
            Position = projectionStateItem.Position;
        }
    }
}
