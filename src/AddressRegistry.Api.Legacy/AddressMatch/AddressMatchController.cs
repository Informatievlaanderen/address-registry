namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Infrastructure.Options;
    using Matching;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Converters;
    using Requests;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Projections.Syndication;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adresmatch")]
    [ApiExplorerSettings(GroupName = "AdresMatch")]
    public class AddressMatchController : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(AddressMatchCollection), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressMatchResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ValidationErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] IKadRrService kadRrService,
            [FromServices] ILatestQueries latestQueries,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromServices] AddressMatchContext context,
            [FromServices] BuildingContext buildingContext,
            [FromQuery] AddressMatchRequest addressMatchRequest,
            CancellationToken cancellationToken = default)
        {
            await new AddressMatchRequestValidator()
                .ValidateAndThrowAsync(addressMatchRequest, cancellationToken: cancellationToken);

            var warningLogger = new ValidationMessageWarningLogger();
            var addressMatch = new AddressMatchMatchingAlgorithm<AdresMatchItem>(
                kadRrService,
                new ManualAddressMatchConfig(responseOptions.Value.SimilarityThreshold, responseOptions.Value.MaxStreetNamesThreshold),
                latestQueries,
                new GemeenteMapper(responseOptions.Value),
                new StreetNameMapper(responseOptions.Value, latestQueries),
                new AdresMapper(responseOptions.Value, latestQueries, context, buildingContext),
                warningLogger);

            var result = addressMatch.Process(new AddressMatchBuilder(Map(addressMatchRequest)));
            return Ok(new AddressMatchCollection
            {
                AdresMatches = result.ToList().OrderByDescending(i => i.Score).Take(10).ToList(),
                Warnings = warningLogger.Warnings
            });
        }

        public AddressMatchQueryComponents Map(AddressMatchRequest request) =>
            new AddressMatchQueryComponents
            {
                MunicipalityName = request.Gemeentenaam,
                HouseNumber = request.Huisnummer,
                BoxNumber = request.Busnummer,
                Index = request.Index,
                KadStreetNameCode = request.KadStraatcode,
                NisCode = request.Niscode,
                PostalCode = request.Postcode,
                RrStreetCode = request.RrStraatcode,
                StreetName = request.Straatnaam
            };
    }
}
