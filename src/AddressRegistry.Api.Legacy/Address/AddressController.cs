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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Consumer.Read.Municipality;
    using Convertors;
    using Infrastructure;
    using Infrastructure.FeatureToggles;
    using Projections.Syndication.Municipality;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "Adressen")]
    public class AddressController : ApiController
    {
        private readonly UseProjectionsV2Toggle _useProjectionsV2Toggle;

        public AddressController(UseProjectionsV2Toggle useProjectionsV2Toggle)
        {
            _useProjectionsV2Toggle = useProjectionsV2Toggle;
        }

        /// <summary>
        /// Vraag een adres op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="municipalityConsumerContext"></param>
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(AddressNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(AddressGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] MunicipalityConsumerContext municipalityConsumerContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] int persistentLocalId,
            [FromRoute] Taal? taal,
            CancellationToken cancellationToken = default)
        {
            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                var addressV2 = await context
                   .AddressDetailV2
                   .AsNoTracking()
                   .SingleOrDefaultAsync(item => item.AddressPersistentLocalId == persistentLocalId, cancellationToken);

                if (addressV2 != null && addressV2.Removed)
                    throw new ApiException("Adres werd verwijderd.", StatusCodes.Status410Gone);

                if (addressV2 == null)
                    throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);

                var streetNameV2 = await syndicationContext.StreetNameLatestItems.FirstOrDefaultAsync(x =>
                    x.PersistentLocalId == addressV2.StreetNamePersistentLocalId.ToString(), cancellationToken);

                var municipalityV2 =
                    await municipalityConsumerContext.MunicipalityLatestItems.FirstAsync(m => m.NisCode == streetNameV2.NisCode,
                        cancellationToken);
                var defaultMunicipalityNameV2 = AddressMapper.GetDefaultMunicipalityName(municipalityV2);
                var defaultStreetNameV2 =
                    AddressMapper.GetDefaultStreetNameName(streetNameV2, municipalityV2.PrimaryLanguage);
                var defaultHomonymAdditionV2 =
                    AddressMapper.GetDefaultHomonymAddition(streetNameV2, municipalityV2.PrimaryLanguage);

                var gemeenteV2 = new AdresDetailGemeente(
                    municipalityV2.NisCode,
                    string.Format(responseOptions.Value.GemeenteDetailUrl, municipalityV2.NisCode),
                    new GeografischeNaam(defaultMunicipalityNameV2.Value, defaultMunicipalityNameV2.Key));

                var straatV2 = new AdresDetailStraatnaam(
                    streetNameV2.PersistentLocalId,
                    string.Format(responseOptions.Value.StraatnaamDetailUrl, streetNameV2.PersistentLocalId),
                    new GeografischeNaam(defaultStreetNameV2.Value, defaultStreetNameV2.Key));

                var postInfoV2 = string.IsNullOrEmpty(addressV2.PostalCode)
                    ? null
                    : new AdresDetailPostinfo(
                        addressV2.PostalCode,
                        string.Format(responseOptions.Value.PostInfoDetailUrl, addressV2.PostalCode));

                var homoniemToevoegingV2 = defaultHomonymAdditionV2 == null
                    ? null
                    : new HomoniemToevoeging(new GeografischeNaam(defaultHomonymAdditionV2.Value.Value,
                        defaultHomonymAdditionV2.Value.Key));

                return Ok(
                    new AddressResponse(
                        responseOptions.Value.Naamruimte,
                        addressV2.AddressPersistentLocalId.ToString(),
                        addressV2.HouseNumber,
                        addressV2.BoxNumber,
                        gemeenteV2,
                        straatV2,
                        homoniemToevoegingV2,
                        postInfoV2,
                        AddressMapper.GetAddressPoint(addressV2.Position),
                        AddressMapper.ConvertFromGeometryMethod(addressV2.PositionMethod),
                        AddressMapper.ConvertFromGeometrySpecification(addressV2.PositionSpecification),
                        addressV2.Status.ConvertFromAddressStatus(),
                        defaultStreetNameV2.Key,
                        addressV2.OfficiallyAssigned,
                        addressV2.VersionTimestamp.ToBelgianDateTimeOffset()));
            }

            var address = await context
                   .AddressDetail
                   .AsNoTracking()
                   .SingleOrDefaultAsync(item => item.PersistentLocalId == persistentLocalId, cancellationToken);

            if (address != null && address.Removed)
                throw new ApiException("Adres werd verwijderd.", StatusCodes.Status410Gone);

            if (address == null || !address.Complete)
                throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);

            var streetName =
                await syndicationContext.StreetNameLatestItems.FindAsync(new object[] { address.StreetNameId },
                    cancellationToken);
            var municipality =
                await syndicationContext.MunicipalityLatestItems.FirstAsync(m => m.NisCode == streetName.NisCode,
                    cancellationToken);
            var defaultMunicipalityName = AddressMapper.GetDefaultMunicipalityName(municipality);
            var defaultStreetName =
                AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage);
            var defaultHomonymAddition =
                AddressMapper.GetDefaultHomonymAddition(streetName, municipality.PrimaryLanguage);

            var gemeente = new AdresDetailGemeente(
                municipality.NisCode,
                string.Format(responseOptions.Value.GemeenteDetailUrl, municipality.NisCode),
                new GeografischeNaam(defaultMunicipalityName.Value, defaultMunicipalityName.Key));

            var straat = new AdresDetailStraatnaam(
                streetName.PersistentLocalId,
                string.Format(responseOptions.Value.StraatnaamDetailUrl, streetName.PersistentLocalId),
                new GeografischeNaam(defaultStreetName.Value, defaultStreetName.Key));

            var postInfo = string.IsNullOrEmpty(address.PostalCode)
                ? null
                : new AdresDetailPostinfo(
                    address.PostalCode,
                    string.Format(responseOptions.Value.PostInfoDetailUrl, address.PostalCode));

            var homoniemToevoeging = defaultHomonymAddition == null
                ? null
                : new HomoniemToevoeging(new GeografischeNaam(defaultHomonymAddition.Value.Value,
                    defaultHomonymAddition.Value.Key));

            return Ok(
                new AddressResponse(
                    responseOptions.Value.Naamruimte,
                    address.PersistentLocalId.ToString(),
                    address.HouseNumber,
                    address.BoxNumber,
                    gemeente,
                    straat,
                    homoniemToevoeging,
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
        /// <param name="taal">Gewenste taal van de respons.</param>
        /// <param name="queryContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de adresmatch gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(AddressListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressListResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            [FromServices] AddressQueryContext queryContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            Taal? taal,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                var pagedAddressesV2 = new AddressListQueryV2(queryContext)
                    .Fetch(filtering, sorting, pagination);

                Response.AddPagedQueryResultHeaders(pagedAddressesV2);

                var addressesV2 = await pagedAddressesV2.Items
                    .Select(a =>
                    new
                    {
                        a.AddressPersistentLocalId,
                        a.StreetNamePersistentLocalId,
                        a.HouseNumber,
                        a.BoxNumber,
                        a.PostalCode,
                        a.Status,
                        a.VersionTimestamp
                    })
                    .ToListAsync(cancellationToken);

                var streetNameIdsV2 = addressesV2
                    .Select(x => x.StreetNamePersistentLocalId.ToString())
                    .Distinct()
                    .ToList();

                var streetNamesV2 = await queryContext
                    .StreetNameLatestItems
                    .Where(x => streetNameIdsV2.Contains(x.PersistentLocalId))
                    .ToListAsync(cancellationToken);

                var nisCodesV2 = streetNamesV2
                    .Select(x => x.NisCode)
                    .Distinct()
                    .ToList();

                var municipalitiesV2 = await queryContext
                    .MunicipalityConsumerLatestItems
                    .Where(x => nisCodesV2.Contains(x.NisCode))
                    .ToListAsync(cancellationToken);

                var addressListItemResponsesV2 = addressesV2
                    .Select(a =>
                    {
                        var streetName = streetNamesV2.SingleOrDefault(x => x.PersistentLocalId == a.StreetNamePersistentLocalId.ToString());
                        Consumer.Read.Municipality.Projections.MunicipalityLatestItem municipality = null;
                        if (streetName != null)
                            municipality = municipalitiesV2.SingleOrDefault(x => x.NisCode == streetName.NisCode);
                        return new AddressListItemResponse(
                            a.AddressPersistentLocalId,
                            responseOptions.Value.Naamruimte,
                            responseOptions.Value.DetailUrl,
                            a.HouseNumber,
                            a.BoxNumber,
                            AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                            a.Status.ConvertFromAddressStatus(),
                            a.VersionTimestamp.ToBelgianDateTimeOffset());
                    })
                    .ToList();

                return Ok(new AddressListResponse
                {
                    Adressen = addressListItemResponsesV2,
                    Volgende = pagedAddressesV2.PaginationInfo.BuildNextUri(responseOptions.Value.VolgendeUrl)
                });
            }

            var pagedAddresses = new AddressListQuery(queryContext)
                .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedAddresses);

            var addresses = await pagedAddresses.Items
                .Select(a =>
                new
                {
                    a.PersistentLocalId,
                    a.StreetNameId,
                    a.HouseNumber,
                    a.BoxNumber,
                    a.PostalCode,
                    a.Status,
                    a.VersionTimestamp
                })
                .ToListAsync(cancellationToken);

            var streetNameIds = addresses
                .Select(x => x.StreetNameId)
                .Distinct()
                .ToList();

            var streetNames = await queryContext
                .StreetNameLatestItems
                .Where(x => streetNameIds.Contains(x.StreetNameId))
                .ToListAsync(cancellationToken);

            var nisCodes = streetNames
                .Select(x => x.NisCode)
                .Distinct()
                .ToList();

            var municipalities = await queryContext
                .MunicipalityLatestItems
                .Where(x => nisCodes.Contains(x.NisCode))
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(a =>
                {
                    var streetName = streetNames.SingleOrDefault(x => x.StreetNameId == a.StreetNameId);
                    MunicipalityLatestItem municipality = null;
                    if (streetName != null)
                        municipality = municipalities.SingleOrDefault(x => x.NisCode == streetName.NisCode);
                    return new AddressListItemResponse(
                        a.PersistentLocalId,
                        responseOptions.Value.Naamruimte,
                        responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        a.BoxNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                        AddressMapper.ConvertFromAddressStatus(a.Status),
                        a.VersionTimestamp.ToBelgianDateTimeOffset());
                })
                .ToList();

            return Ok(new AddressListResponse
            {
                Adressen = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(responseOptions.Value.VolgendeUrl)
            });
        }

        /// <summary>
        /// Vraag het totaal aantal adressen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            [FromServices] LegacyContext context,
            [FromServices] AddressQueryContext queryContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();


            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                return Ok(
                    new TotaalAantalResponse
                    {
                        Aantal = filtering.ShouldFilter
                            ? await new AddressListQueryV2(queryContext)
                                .Fetch(filtering, sorting, pagination)
                                .Items
                                .CountAsync(cancellationToken)
                            : Convert.ToInt32(context
                                .AddressListViewCountV2
                                .First()
                                .Count)
                    });
            }

            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new AddressListQuery(queryContext)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32(context
                            .AddressListViewCount
                            .First()
                            .Count)
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
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var lastFeedUpdate = await context
                .AddressSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var pagedAddresses =
                new AddressSyndicationQuery(
                    context,
                    filtering.Filter?.Embed)
                .Fetch(filtering, sorting, pagination);

            return new ContentResult
            {
                Content = await BuildAtomFeed(lastFeedUpdate, pagedAddresses, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Vraag een adres op.
        /// </summary>
        /// <param name="context"></param>
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressBosaResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Post(
            [FromServices] AddressBosaContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromBody] BosaAddressRequest addressRequest,
            CancellationToken cancellationToken = default)
        {
            if (Request.ContentLength.HasValue && Request.ContentLength > 0 && addressRequest == null)
                return Ok(new AddressBosaResponse());

            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                var queryV2 = new AddressBosaQueryV2(context, responseOptions.Value);
                return Ok(await queryV2.Filter(addressRequest));
            }

            var query = new AddressBosaQuery(context, responseOptions.Value);
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRepresentationBosaResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(AddressNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> PostBosaAddressRepresentations(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] MunicipalityConsumerContext municipalityConsumerContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromBody] BosaAddressRepresentationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (Request.ContentLength.HasValue && Request.ContentLength > 0 && request == null)
                return Ok(new AddressRepresentationBosaResponse());

            if (string.IsNullOrEmpty(request?.AdresCode?.ObjectId) || !int.TryParse(request.AdresCode.ObjectId, out var addressId))
                return BadRequest("Valid objectId is required");

            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                var addressV2 = await context.AddressDetailV2
                    .FirstOrDefaultAsync(x => x.AddressPersistentLocalId == addressId, cancellationToken);
                if (addressV2 == null)
                    return NotFound();

                var streetNameV2 = await syndicationContext
                    .StreetNameBosaItems
                    .FirstOrDefaultAsync(x => x.PersistentLocalId == addressV2.StreetNamePersistentLocalId.ToString(), cancellationToken);

                var municipalityV2 = await municipalityConsumerContext
                    .MunicipalityLatestItems
                    .FirstOrDefaultAsync(x => x.NisCode == streetNameV2.NisCode, cancellationToken);

                var responseV2 = new AddressRepresentationBosaResponse
                {
                    Identificator = new AdresIdentificator(responseOptions.Value.Naamruimte, addressV2.AddressPersistentLocalId.ToString(), addressV2.VersionTimestamp.ToBelgianDateTimeOffset())
                };

                if (!request.Taal.HasValue || request.Taal.Value == municipalityV2.PrimaryLanguage.ToTaal())
                {
                    responseV2.AdresVoorstellingen = new List<BosaAddressRepresentation>
                    {
                        new BosaAddressRepresentation(
                            municipalityV2.PrimaryLanguage.ToTaal(),
                            addressV2.HouseNumber,
                            addressV2.BoxNumber,
                            AddressMapper.GetVolledigAdres(addressV2.HouseNumber, addressV2.BoxNumber, addressV2.PostalCode, streetNameV2, municipalityV2).GeografischeNaam.Spelling,
                            AddressMapper.GetDefaultMunicipalityName(municipalityV2).Value,
                            AddressMapper.GetDefaultStreetNameName(streetNameV2, municipalityV2.PrimaryLanguage).Value,
                            addressV2.PostalCode)
                    };
                }

                return Ok(responseV2);
            }

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
                Identificator = new AdresIdentificator(responseOptions.Value.Naamruimte, address.PersistentLocalId.ToString(), address.VersionTimestamp.ToBelgianDateTimeOffset())
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
            DateTimeOffset lastFeedUpdate,
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
                var atomFeedConfig = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastFeedUpdate);

                await writer.WriteDefaultMetadata(atomFeedConfig);

                var addresses = pagedAddresses.Items.ToList();

                var nextFrom = addresses.Any()
                    ? addresses.Max(x => x.Position) + 1
                    : (long?)null;

                var nextUri = BuildNextSyncUri(pagedAddresses.PaginationInfo.Limit, nextFrom, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, "next"));

                foreach (var address in addresses)
                    await writer.WriteAddress(responseOptions, formatter, syndicationConfiguration["Category"], address);

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri BuildNextSyncUri(int limit, long? from, string nextUrlBase)
        {
            return from.HasValue
                ? new Uri(string.Format(nextUrlBase, from, limit))
                : null;
        }
    }
}
