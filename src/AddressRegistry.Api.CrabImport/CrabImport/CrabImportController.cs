namespace AddressRegistry.Api.CrabImport.CrabImport
{
    using System;
    using Dasync.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CrabImport;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Converters;
    using Requests;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabimport")]
    [ApiExplorerSettings(GroupName = "CRAB Import")]
    public class CrabImportController : ApiController
    {
        private const string AggregateMessageTemplate = "Preparing to process commands for {AggregateCount} aggregates.";
        private const string CommandMessageTemplate = "Handled {CommandCount} commands in {Elapsed:0.0000} ms";
        private const string BatchMessageTemplate = "Handled {AggregateCount} aggregates ({CommandCount} commands) in {Elapsed:0.0000} ms";
        private static double GetElapsedMilliseconds(long start, long stop) => (stop - start) * 1000 / (double)Stopwatch.Frequency;

        /// <summary>
        /// Import een CRAB item.
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="logger"></param>
        /// <param name="registerCrabImportList"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het verzoek aanvaard is.</response>
        /// <response code="400">Als het verzoek ongeldige data bevat.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(RegisterCrabImportRequest), typeof(RegisterCrabImportRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status202Accepted, typeof(RegisterCrabImportResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Post(
            [FromServices] IdempotentCommandHandlerModule bus,
            [FromServices] ILogger<CrabImportController> logger,
            [FromBody] List<RegisterCrabImportRequest[]> registerCrabImportList,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tags = new ConcurrentBag<long?>();

            var start = Stopwatch.GetTimestamp();
            logger.LogDebug(AggregateMessageTemplate, registerCrabImportList.Count);

            await registerCrabImportList.ParallelForEachAsync(async registerCrabImports =>
            {
                var startCommands = Stopwatch.GetTimestamp();

                try
                {
                    var commandsPerCommandId = registerCrabImports
                        .Select(RegisterCrabImportRequestMapping.Map)
                        .Distinct(new LambdaEqualityComparer<dynamic>(x => (string)x.CreateCommandId().ToString()))
                        .ToDictionary(x => (Guid?)x.CreateCommandId(), x => x);

                    var tag = await bus
                        .IdempotentCommandHandlerDispatchBatch(
                            commandsPerCommandId,
                            GetMetadata(),
                            cancellationToken);

                    tags.Add(tag);
                }
                catch
                {
                    var x = registerCrabImports.Select(RegisterCrabImportRequestMapping.Map).ToList();
                    Console.WriteLine($"Boom, {x}");
                    throw;
                }

                var elapsedCommandsMs = GetElapsedMilliseconds(startCommands, Stopwatch.GetTimestamp());
                logger.LogDebug(CommandMessageTemplate, registerCrabImports.Length, elapsedCommandsMs);
            },
            cancellationToken: cancellationToken,
            maxDegreeOfParallelism: 0);

            logger.LogDebug(
                BatchMessageTemplate,
                registerCrabImportList.Count,
                registerCrabImportList.SelectMany(x => x).Count(),
                GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()));

            return Accepted(tags.Any() ? tags.Max() : null);
        }

        private IDictionary<string, object> GetMetadata()
        {
            string FindClaimValue(string claimName) => User?.FindFirst(claimName)?.Value;

            return new Dictionary<string, object>
            {
                { "FirstName", FindClaimValue(ClaimTypes.GivenName) },
                { "LastName", FindClaimValue(ClaimTypes.Name) },
                { "Ip", FindClaimValue(AddRemoteIpAddressMiddleware.UrnBasisregistersVlaanderenIp) },
                { "UserId", FindClaimValue("urn:be:vlaanderen:addressregistry:acmid") },
                { "CorrelationId", FindClaimValue(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId) }
            };
        }

        [HttpGet("batch/{feed}")]
        public IActionResult GetBatchStatus(
            [FromServices] CrabImportContext context,
            [FromRoute] string feed)
        {
            var status = context.LastBatchFor((ImportFeed)feed);
            return Ok(status);
        }

        [HttpPost("batch")]
        public IActionResult SetBatchStatus(
            [FromServices] CrabImportContext context,
            [FromBody] BatchStatusUpdate batchStatus)
        {
            context.SetCurrent(batchStatus);
            context.SaveChanges();

            return Ok();
        }
    }

    public class RegisterCrabImportResponseExamples : IExamplesProvider
    {
        public object GetExamples() => new { };
    }
}
