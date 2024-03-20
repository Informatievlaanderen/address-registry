namespace AddressRegistry.Projector.Syndication
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("syndication")]
    public class SyndicationStatusController : ApiController
    {
        private const string? SyndicationConnectionStringKey = "Syndication";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            List<SyndicationStatusResponse> response;
            await using (var sqlConnection = new SqlConnection(configuration.GetConnectionString(SyndicationConnectionStringKey)))
            {
                var result =
                    await sqlConnection.QueryAsync<ProjectionStateItem>(
                        "SELECT * FROM [AddressRegistrySyndication].[ProjectionStates] WHERE [DesiredState] IS NULL", cancellationToken);

                response = result.Select(x => new SyndicationStatusResponse(x)).ToList();
            }

            return Ok(response);
        }
    }

    public class SyndicationStatusResponse
    {
        public string ProjectionName { get; set; }
        public long Position { get; set; }

        public SyndicationStatusResponse(ProjectionStateItem projectionStateItem)
        {
            ProjectionName = projectionStateItem.Name;
            Position = projectionStateItem.Position;
        }
    }
}
