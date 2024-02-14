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
    using SqlStreamStore;

    [ApiVersion("1.0")]
    [ApiRoute("caches")]
    public class CachesController : ApiController
    {
        private static readonly Dictionary<string, string> ProjectionNameMapper = new()
        {
            {"AddressRegistry.Projections.LastChangedList.LastChangedListProjections", LastChangedListProjections.ProjectionName}
        };

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            [FromServices] LastChangedListContext lastChangedListContext,
            [FromServices] IReadonlyStreamStore streamStore,
            CancellationToken cancellationToken)
        {
            var maxErrorTimeInSeconds = configuration.GetValue<int?>("Caches:LastChangedList:MaxErrorTimeInSeconds") ?? 60;

            var maxErrorTime = DateTimeOffset.UtcNow.AddSeconds(-1 * maxErrorTimeInSeconds);
            var numberOfRecords = await lastChangedListContext
                .LastChangedList
                .OrderBy(x => x.Id)
                .Where(r => r.ToBeIndexed && (r.LastError == null || r.LastError < maxErrorTime))
                .CountAsync(cancellationToken);

            var positions = await lastChangedListContext.ProjectionStates.ToListAsync(cancellationToken);
            var streamPosition = await streamStore.ReadHeadPosition(cancellationToken);

            var response = new List<dynamic>
            {
                new
                {
                    name = "Cache detail adressen",
                    numberOfRecordsToProcess = numberOfRecords
                }
            };

            foreach (var position in positions)
            {
                response.Add(new
                {
                    name = ProjectionNameMapper.ContainsKey(position.Name) ? ProjectionNameMapper[position.Name] : position.Name,
                    numberOfRecordsToProcess = streamPosition - position.Position
                });
            }

            return Ok(response);
        }
    }
}
