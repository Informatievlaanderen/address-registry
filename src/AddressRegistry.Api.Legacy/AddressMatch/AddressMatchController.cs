namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Infrastructure.Options;
    using Matching;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Requests;
    using Responses;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Converters;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adresmatch")]
    [ApiExplorerSettings(GroupName = "AdresMatch")]
    public class AddressMatchController : ApiController
    {
        private const double SimilarityThreshold = 75.0;
        private const int MaxStreetNamesThreshold = 100;

        //TODO: Add Examples
        //TODO: StatusCodes?
        [HttpGet]
        [ProducesResponseType(typeof(AdresMatchCollectie), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] IKadRrService kadRrService,
            [FromServices] ILatestQueries latestQueries,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromQuery] AddressMatchRequest addressMatchRequest,
            CancellationToken cancellationToken = default)
        {
            //TODO: Implement validator

            var warningLogger = new ValidationMessageWarningLogger();
            var addressMatch = new AddressMatchMatchingAlgorithm<AdresMatchItem>(
                kadRrService,
                new ManualAddressMatchConfig(SimilarityThreshold, MaxStreetNamesThreshold),
                latestQueries,
                new GemeenteMapper(responseOptions.Value),
                new StreetNameMapper(responseOptions.Value, latestQueries),
                new AdresMapper(responseOptions.Value, latestQueries),
                warningLogger);

            var result = addressMatch.Process(new AddressMatchBuilder(Map(addressMatchRequest)));
            return Ok(new AdresMatchCollectie
            {
                AdresMatches = result.OrderByDescending(i => i.Score).Take(10).ToList(),
                Warnings = warningLogger.Warnings
            });
        }

        public AddressMatchQueryComponents Map(AddressMatchRequest request)
        {
            return new AddressMatchQueryComponents
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
}
