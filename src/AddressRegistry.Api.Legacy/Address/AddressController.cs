namespace AddressRegistry.Api.Legacy.Address
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Newtonsoft.Json.Converters;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Requests;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "Adressen")]
    public class AddressController : ApiController
    {
        /// <summary>
        /// Vraag een adres op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="persistentLocalId">Identificator van het adres.</param>
        /// <param name="taal">De taal in dewelke het adres wordt teruggegeven.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het adres gevonden is.</response>
        /// <response code="404">Als het adres niet gevonden kan worden.</response>
        /// <response code="410">Als het adres verwijderd is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}")]
        [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(AddressNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(AddressGoneResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] int persistentLocalId,
            [FromRoute] Taal? taal,
            CancellationToken cancellationToken = default)
        {
            var address = await context
                .AddressDetail
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == persistentLocalId, cancellationToken);

            if (address == null || !address.Complete)
                throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);

            if (address.Removed)
                throw new ApiException("Adres werd verwijderd.", StatusCodes.Status410Gone);

            var streetName = await syndicationContext.StreetNameLatestItems.FindAsync(new object[] { address.StreetNameId }, cancellationToken);
            var municipality = await syndicationContext.MunicipalityLatestItems.FirstAsync(m => m.NisCode == streetName.NisCode, cancellationToken);
            var defaultMunicipalityName = AddressMapper.GetDefaultMunicipalityName(municipality);
            var defaultStreetName = AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);

            var gemeente = new AdresDetailGemeente(
                municipality.NisCode,
                string.Format(responseOptions.Value.GemeenteDetailUrl, municipality.NisCode),
                new GeografischeNaam(defaultMunicipalityName.Value, defaultMunicipalityName.Key));

            var straat = new AdresDetailStraatnaam(
                streetName.PersistentLocalId,
                string.Format(responseOptions.Value.StraatnaamDetailUrl, streetName.PersistentLocalId),
                new GeografischeNaam(defaultStreetName.Value, defaultStreetName.Key));

            var postInfo = new AdresDetailPostinfo(
                address.PostalCode,
                string.Format(responseOptions.Value.PostInfoDetailUrl, address.PostalCode));

            return Ok(
                new AddressResponse(
                    responseOptions.Value.Naamruimte,
                    address.PersistentLocalId.ToString(),
                    address.HouseNumber,
                    address.BoxNumber,
                    gemeente,
                    straat,
                    postInfo,
                    AddressMapper.GetAddressPoint(address.Position),
                    AddressMapper.ConvertFromGeometryMethod(address.PositionMethod),
                    AddressMapper.ConvertFromGeometrySpecification(address.PositionSpecification),
                    AddressMapper.ConvertFromAddressStatus(address.Status),
                    defaultStreetName.Key,
                    address.OfficiallyAssigned,
                    address.VersionTimestamp.ToBelgianDateTimeOffset()));
        }

        /// <summary>
        /// Vraag een lijst met adressen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="taal">Gewenste taal van de respons.</param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met adressen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(AddressListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            Taal? taal,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedAddresses = new AddressListQuery(context, syndicationContext).Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedAddresses.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            var addresses = await pagedAddresses.Items
                .Select(a =>
                new
                {
                    a.PersistentLocalId,
                    a.StreetNameId,
                    a.HouseNumber,
                    a.BoxNumber,
                    a.PostalCode,
                    a.VersionTimestamp
                })
                .ToListAsync(cancellationToken);

            var streetNameIds = addresses
                .Select(x => x.StreetNameId)
                .Distinct()
                .ToList();

            var streetNames = await syndicationContext
                .StreetNameLatestItems
                .Where(x => streetNameIds.Contains(x.StreetNameId))
                .ToListAsync(cancellationToken);

            var nisCodes = streetNames
                .Select(x => x.NisCode)
                .Distinct()
                .ToList();

            var municipalities = await syndicationContext
                .MunicipalityLatestItems
                .Where(x => nisCodes.Contains(x.NisCode))
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(a =>
                {
                    var streetName = streetNames.Single(x => x.StreetNameId == a.StreetNameId);
                    var municipality = municipalities.Single(x => x.NisCode == streetName.NisCode);
                    return new AddressListItemResponse(
                        a.PersistentLocalId,
                        responseOptions.Value.Naamruimte,
                        responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        a.BoxNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                        a.VersionTimestamp.ToBelgianDateTimeOffset());
                })
                .ToList();

            return Ok(new AddressListResponse
            {
                Adressen = addressListItemResponses,
                TotaalAantal = pagedAddresses.PaginationInfo.TotalItems,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(responseOptions.Value.VolgendeUrl)
            });
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van adressen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedAddresses = new AddressSyndicationQuery(
                context,
                filtering.Filter?.ContainsEvent ?? false,
                filtering.Filter?.ContainsObject ?? false)
                .Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedAddresses.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            return new ContentResult
            {
                Content = await BuildAtomFeed(pagedAddresses, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Vraag een adres op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="addressRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het adres gevonden is.</response>
        /// <response code="400">Als het adres niet gevonden kan worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpPost("bosa")]
        [ProducesResponseType(typeof(AddressBosaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(BosaAddressRequest), typeof(BosaAddressRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressBosaResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Post(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromBody] BosaAddressRequest addressRequest,
            CancellationToken cancellationToken = default)
        {
            if (Request.ContentLength.HasValue && Request.ContentLength > 0 && addressRequest == null)
                return Ok(new AddressBosaResponse());

            var query = new AddressBosaQuery(context, syndicationContext, responseOptions.Value);

            return Ok(await query.Filter(addressRequest));
        }

        /// <summary>
        /// Vraag een adres op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het adres gevonden is.</response>
        /// <response code="400">Als het adres niet gevonden kan worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpPost("bosa/adresvoorstellingen")]
        [ProducesResponseType(typeof(AddressRepresentationBosaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(BosaAddressRepresentationRequest), typeof(BosaAddressRepresentationRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRepresentationBosaResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(AddressNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> PostBosaAddressRepresentations(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromBody] BosaAddressRepresentationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (Request.ContentLength.HasValue && Request.ContentLength > 0 && request == null)
                return Ok(new AddressRepresentationBosaResponse());

            if (string.IsNullOrEmpty(request?.AdresCode?.ObjectId) || !int.TryParse(request.AdresCode.ObjectId, out var addressId))
                return BadRequest("Valid objectId is required");

            var address = await context.AddressDetail.FirstOrDefaultAsync(x => x.PersistentLocalId == addressId, cancellationToken);
            if (address == null)
                return NotFound();

            var streetName = await syndicationContext
                .StreetNameBosaItems
                .FirstOrDefaultAsync(x => x.StreetNameId == address.StreetNameId, cancellationToken);

            var municipality = await syndicationContext
                .MunicipalityBosaItems
                .FirstOrDefaultAsync(x => x.NisCode == streetName.NisCode, cancellationToken);

            var response = new AddressRepresentationBosaResponse
            {
                Identificator = new Identificator(responseOptions.Value.Naamruimte, address.PersistentLocalId.ToString(), address.VersionTimestamp.ToBelgianDateTimeOffset())
            };

            if (!request.Taal.HasValue || request.Taal.Value == municipality.PrimaryLanguage)
            {
                response.AdresVoorstellingen = new List<BosaAddressRepresentation>
                {
                    new BosaAddressRepresentation(
                        municipality.PrimaryLanguage.Value,
                        address.HouseNumber,
                        address.BoxNumber,
                        AddressMapper.GetVolledigAdres(address.HouseNumber, address.BoxNumber, address.PostalCode, streetName, municipality).GeografischeNaam.Spelling,
                        AddressMapper.GetDefaultMunicipalityName(municipality).Value,
                        AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage).Value,
                        address.PostalCode)
                };
            }

            return Ok(response);
        }

        private static async Task<string> BuildAtomFeed(
            PagedQueryable<AddressSyndicationQueryResult> pagedAddresses,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");

                await writer.WriteDefaultMetadata(
                    syndicationConfiguration["Id"],
                    syndicationConfiguration["Title"],
                    Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    new Uri(syndicationConfiguration["Self"]),
                    syndicationConfiguration.GetSection("Related").GetChildren().Select(c => c.Value).ToArray());

                var nextUri = pagedAddresses.PaginationInfo.BuildNextUri(syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, "next"));

                foreach (var address in pagedAddresses.Items)
                    await writer.WriteAddress(responseOptions, formatter, syndicationConfiguration["Category"], address);

                xmlWriter.Flush();
            }

            return sw.ToString();
        }
    }
}
