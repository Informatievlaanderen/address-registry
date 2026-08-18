namespace AddressRegistry.Api.Oslo.Address.V3.Detail
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class AddressDetailOsloV3Response
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de adres envelop.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "AdresEnvelop";

        /// <summary>
        /// De data van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "data", Order = 2, Required = Required.DisallowNull)]
        public AddressDetailOsloV3ResponseData Data { get; set; }

        /// <summary>
        /// De hyperlinks die gerelateerd zijn aan het adres.
        /// </summary>
        [JsonProperty(PropertyName = "_links", Order = 14, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AddressDetailOsloV3ResponseLinks? Links { get; set; }

        [JsonIgnore] public string? LastEventHash { get; }

        public AddressDetailOsloV3Response(
            string contextUrlDetail,
            string objectId,
            string huisnummer,
            AdresIsDeelVan? huisnummerObject,
            string busnummer,
            AdresHeeftGemeentenaam gemeente,
            AdresHeeftStraatnaam straatnaam,
            AdresHeeftPostinfo postInfo,
            string postCode,
            AddressPositionV3 adresPositie,
            AdresStatusValue status,
            bool officieelToegekend,
            DateTimeOffset version,
            string selfDetailUrl,
            string parcelLinkUrl,
            string buildingUnitLinkUrl,
            string? lastEventHash = null)
        {
            Context = contextUrlDetail;
            Data = new AddressDetailOsloV3ResponseData(
                objectId,
                huisnummer,
                huisnummerObject,
                busnummer,
                gemeente,
                straatnaam,
                postInfo,
                postCode,
                adresPositie,
                status,
                officieelToegekend,
                version);

            LastEventHash = lastEventHash;

            Links = new AddressDetailOsloV3ResponseLinks(
                self: new Link
                {
                    Href = new Uri(string.Format(selfDetailUrl, objectId))
                },
                percelen: new Link
                {
                    Href = new Uri(string.Format(parcelLinkUrl, objectId))
                },
                gebouweenheden: new Link
                {
                    Href = new Uri(string.Format(buildingUnitLinkUrl, objectId))
                }
            );
        }
    }

    /// <summary>
    /// De data van het adres.
    /// </summary>
    public class AddressDetailOsloV3ResponseData
    {
        /// <summary>
        /// Het linked-data type van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Adres";

        /// <summary>
        /// De unieke en persistente identificator van het adres (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "identificator", Order = 2, Required = Required.DisallowNull)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De gemeentenaam die deel uitmaakt van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "heeftGemeentenaam", Order = 3, Required = Required.DisallowNull)]
        public AdresHeeftGemeentenaam Gemeente { get; set; }

        /// <summary>
        /// De postinfo die deel uitmaakt van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "heeftPostinfo", Order = 4, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public AdresHeeftPostinfo Postinfo { get; set; }

        /// <summary>
        /// De straatnaam die deel uitmaakt van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "heeftStraatnaam", Order = 5, Required = Required.DisallowNull)]
        public AdresHeeftStraatnaam Straatnaam { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "huisnummer", Order = 7, Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het huisnummer waaraan het busnummer is gekoppeld.
        /// </summary>
        [JsonProperty(PropertyName = "isDeelVan", Order = 8, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public AdresIsDeelVan? HuisnummerObject { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "busnummer", Order = 9, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [JsonProperty(PropertyName = "isVerrijktMet", Order = 10, Required = Required.DisallowNull)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// De geometrie van het object in gml-formaat.
        /// </summary>
        [JsonProperty(PropertyName = "positie", Order = 11, Required = Required.DisallowNull)]
        public AddressPositionV3 AdresPositie { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 12, Required = Required.DisallowNull)]
        public AdresStatus Status { get; set; }

        /// <summary>
        /// False wanneer het bestaan van het adres niet geweten is ten tijde van administratieve procedures, maar pas na observatie op het terrein.
        /// </summary>
        [JsonProperty(PropertyName = "officieelToegekend", Order = 13, Required = Required.DisallowNull)]
        public bool OfficieelToegekend { get; set; }

        public AddressDetailOsloV3ResponseData(
            string objectId,
            string huisnummer,
            AdresIsDeelVan? huisnummerObject,
            string busnummer,
            AdresHeeftGemeentenaam gemeente,
            AdresHeeftStraatnaam straatnaam,
            AdresHeeftPostinfo postInfo,
            string postcode,
            AddressPositionV3 adresPositie,
            AdresStatusValue status,
            bool officieelToegekend,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.Adres.ToPuri(objectId);
            Identificator = new AdresIdentificator(objectId, version);
            Gemeente = gemeente;
            Straatnaam = straatnaam;
            Postinfo = postInfo;
            Busnummer = busnummer;
            Huisnummer = huisnummer;
            HuisnummerObject = huisnummerObject;
            AdresPositie = adresPositie;
            Status = new AdresStatus(status);
            OfficieelToegekend = officieelToegekend;

            VolledigAdres = new VolledigAdres();
            foreach (var straatNaam in straatnaam.Straatnaam)
            {
                var taal = straatNaam.Taal;
                var gemeenteNaam = gemeente.Gemeentenamen.FirstOrDefault(x => x.Taal == taal);

                VolledigAdres.Add(straatNaam.Spelling,
                    huisnummer,
                    busnummer,
                    postcode,
                    gemeenteNaam?.Spelling ?? gemeente.Gemeentenamen.First().Spelling,
                    taal);
            }
        }
    }

    /// <summary>
    /// De hyperlinks die gerelateerd zijn aan het adres.
    /// </summary>
    public class AddressDetailOsloV3ResponseLinks
    {
        [JsonProperty(PropertyName = "self", Required = Required.DisallowNull)]
        public Link Self { get; set; }

        [JsonProperty(PropertyName = "gebouweenheden", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Link? Gebouweenheden { get; set; }

        [JsonProperty(PropertyName = "percelen", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Link? Percelen { get; set; }

        public AddressDetailOsloV3ResponseLinks(
            Link self,
            Link? gebouweenheden = null,
            Link? percelen = null)
        {
            Self = self;
            Gebouweenheden = gebouweenheden;
            Percelen = percelen;
        }
    }

    public class AddressDetailOsloResponseExamples : IExamplesProvider<AddressDetailOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public AddressDetailOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressDetailOsloV3Response GetExamples()
        {
            var gml1972 =
                "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>";

            var gml2008 =
                "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>640249.09 698793.29</gml:pos></gml:Point>";

            var addressPosition = new AddressPositionV3([new PointGeometrie(gml1972), new PointGeometrie(gml2008)],
                PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Gebouw);
            var adresDetailHuisnummer = new AdresIsDeelVan(59, new Uri(string.Format(_responseOptions.DetailUrl, 59)));
            var gemeente = new AdresHeeftGemeentenaam(OsloNamespaces.Gemeente.ToPuri("44021"), new Uri(string.Format(_responseOptions.GemeenteDetailUrl, "44021")),
                [new GeografischeNaam("Gent", Taal.Nl)]);
            var straat = new AdresHeeftStraatnaam(OsloNamespaces.StraatNaam.ToPuri("748"), new Uri(string.Format(_responseOptions.StraatnaamDetailUrl, "748")),
                [new GeografischeNaam("Teststraat", Taal.Nl)], [new GeografischeNaam("UK", Taal.Nl)]);
            var postInfo = new AdresHeeftPostinfo(OsloNamespaces.Postinfo.ToPuri("9000"), new Uri(string.Format(_responseOptions.PostInfoDetailUrl, "9000")));

            return new AddressDetailOsloV3Response(
                _responseOptions.ContextUrlDetail,
                "60",
                "42",
                adresDetailHuisnummer,
                "5B",
                gemeente,
                straat,
                postInfo,
                "9000",
                addressPosition,
                AdresStatusValue.InGebruik,
                true,
                DateTimeOffset.Now.ToExampleOffset(),
                _responseOptions.DetailUrl,
                _responseOptions.AddressDetailParcelsLink,
                _responseOptions.AddressDetailBuildingUnitsLink);
        }
    }

    public class AddressNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public AddressNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples() =>
            new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:address:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaand adres.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }

    public class AddressGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public AddressGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples() =>
            new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:address:gone",
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Verwijderd adres.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }
}
