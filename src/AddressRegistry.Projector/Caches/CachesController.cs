namespace AddressRegistry.Projector.Caches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Projections.LastChangedList;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    [ApiVersion("1.0")]
    [ApiRoute("caches")]
    public class CachesController : ApiController
    {
        private static Dictionary<string, string> _projectionNameMapper = new Dictionary<string, string>()
        {
            {"AddressRegistry.Projections.LastChangedList.LastChangedListProjections", LastChangedListProjections.ProjectionName}
        };

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            [FromServices] LastChangedListContext lastChangedListContext,
            CancellationToken cancellationToken)
        {
            var maxErrorTimeInSeconds = configuration.GetValue<int?>("Caches:LastChangedList:MaxErrorTimeInSeconds") ?? 60;

            var maxErrorTime = DateTimeOffset.UtcNow.AddSeconds(-1 * maxErrorTimeInSeconds);
            var numberOfRecords = await lastChangedListContext
                .LastChangedList
                .OrderBy(x => x.Id)
                .Where(r => r.ToBeIndexed && (r.LastError == null || r.LastError < maxErrorTime))
                .CountAsync(cancellationToken);

            return Ok(new[]
            {
                new {
                    name = "Cache detail adressen",
                    numberOfRecordsToProcess = numberOfRecords
                }
            });
        }
    }
}
