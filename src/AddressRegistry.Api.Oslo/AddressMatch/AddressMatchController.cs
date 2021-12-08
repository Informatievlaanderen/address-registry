namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Infrastructure.Options;
    using Matching;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Requests;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adresmatch")]
    [ApiExplorerSettings(GroupName = "AdresMatch")]
    public class AddressMatchController : ApiController
    {
        /// <summary>
        /// Voer een adresmatch vraag uit en krijg de adressen die gematcht worden.
        /// </summary>
        /// <param name="kadRrService"></param>
        /// <param name="latestQueries"></param>
        /// <param name="responseOptions"></param>
        /// <param name="context"></param>
        /// <param name="buildingContext"></param>
        /// <param name="addressMatchRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(AddressMatchOsloCollection), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressMatchOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ValidationErrorResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
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

            const int maxNumberOfResults = 10;

            var warningLogger = new ValidationMessageWarningLogger();
            var addressMatch = new AddressMatchMatchingAlgorithm<AdresMatchScorableItem>(
                kadRrService,
                new ManualAddressMatchConfig(responseOptions.Value.SimilarityThreshold, responseOptions.Value.MaxStreetNamesThreshold),
                latestQueries,
                new GemeenteMapper(responseOptions.Value),
                new StreetNameMapper(responseOptions.Value, latestQueries),
                new AdresMapper(responseOptions.Value, latestQueries),
                maxNumberOfResults,
                warningLogger);

            var result = addressMatch
                .Process(new AddressMatchBuilder(Map(addressMatchRequest)))
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.ScoreableProperty)
                .Take(maxNumberOfResults)
                .Select(x => AdresMatchOsloItem.Create(x, buildingContext, context, responseOptions.Value))
                .ToList();

            return Ok(new AddressMatchOsloCollection
            {
                AdresMatches = result,
                Warnings = warningLogger.Warnings
            });
        }

        private AddressMatchQueryComponents Map(AddressMatchRequest request) =>
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
