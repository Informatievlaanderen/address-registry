namespace AddressRegistry.Api.Extract.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Projections.Extract;
    using Projections.Syndication;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extract")]
    [ApiExplorerSettings(GroupName = "Extract")]
    public class ExtractController : ApiController
    {
        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="municipalityConsumerContext"></param>
        /// <param name="streetNameConsumerContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als adresregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] ExtractContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] MunicipalityConsumerContext municipalityConsumerContext,
            [FromServices] StreetNameConsumerContext streetNameConsumerContext,
            CancellationToken cancellationToken = default)
        {
            return new IsolationExtractArchive(ExtractFileNames.GetAddressZip(), context)
                {
                    AddressRegistryExtractBuilder.CreateAddressFilesV2(context, streetNameConsumerContext, municipalityConsumerContext)
                }
                .CreateFileCallbackResult(cancellationToken);
        }

        /// <summary>
        /// Vraag een dump van alle adreskoppelingen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als adreskoppelingen kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("addresslinks")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> GetAddressLinks(
            [FromServices] IConfiguration configuration,
            [FromServices] SyndicationContext syndicationContext,
            CancellationToken cancellationToken = default)
        {
            var extractBuilder = new LinkedAddressExtractBuilder(syndicationContext, configuration.GetConnectionString("SyndicationProjections"));

            return new ExtractArchive(ExtractFileNames.GetAddressLinksZip())
                {
                    extractBuilder.CreateLinkedBuildingUnitAddressFiles(),
                    await extractBuilder.CreateLinkedParcelAddressFiles(cancellationToken)
                }
                .CreateFileCallbackResult(cancellationToken);
        }

        /// <summary>
        /// Vraag een dump van alle postcode-straatnaam koppelingen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als postcode-straatnaam koppelingen kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("postcode-straatnamen")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> GetPostalCodeStreetNameLinks(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var extractBuilder = new PostalCodeStreetNameExtractBuilder(configuration.GetConnectionString("ExtractProjections"));

            return new ExtractArchive(ExtractFileNames.GetPostalCodeStreetNameLinksZip())
                {
                    await extractBuilder.CreateLinkedPostalCodeStreetNameFile(cancellationToken)
                }
                .CreateFileCallbackResult(cancellationToken);
        }
    }
}
