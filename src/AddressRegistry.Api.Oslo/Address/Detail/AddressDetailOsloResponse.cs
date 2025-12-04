namespace AddressRegistry.Api.Oslo.Address.Detail
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "AdresDetail", Namespace = "")]
    public class AddressDetailOsloResponse
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van het adres.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Adres";

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De gemeente die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Gemeente", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresDetailGemeente Gemeente { get; set; }

        /// <summary>
        /// De postinfo die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Postinfo", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AdresDetailPostinfo Postinfo { get; set; }

        /// <summary>
        /// De straatnaam die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresDetailStraatnaam Straatnaam { get; set; }

        /// <summary>
        /// Homoniem toevoeging aan de straatnaam.
        /// </summary>
        [DataMember(Name = "HomoniemToevoeging", Order = 6, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HomoniemToevoeging HomoniemToevoeging { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 7)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het huisnummer waaraan het busnummer is gekoppeld.
        /// </summary>
        [DataMember(Name = "HuisnummerObject", Order = 8, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AdresDetailHuisnummerObject? HuisnummerObject { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 9, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 10)]
        [JsonProperty(Required = Required.DisallowNull)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// De geometrie van het object in gml-formaat.
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 11)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AddressPosition AdresPositie { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 12)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresStatus AdresStatus { get; set; }

        /// <summary>
        /// False wanneer het bestaan van het adres niet geweten is ten tijde van administratieve procedures, maar pas na observatie op het terrein.
        /// </summary>
        [DataMember(Name = "OfficieelToegekend", Order = 13)]
        [JsonProperty(Required = Required.DisallowNull)]
        public bool OfficieelToegekend { get; set; }

        /// <summary>
        /// De hyperlinks die gerelateerd zijn aan het adres.
        /// </summary>
        [DataMember(Name = "_links", Order = 14)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AddressDetailOsloResponseLinks? Links { get; set; }

        [IgnoreDataMember] [JsonIgnore] public string? LastEventHash { get; }

        public AddressDetailOsloResponse(
            string naamruimte,
            string contextUrlDetail,
            string objectId,
            string huisnummer,
            AdresDetailHuisnummerObject? huisnummerObject,
            string busnummer,
            AdresDetailGemeente gemeente,
            AdresDetailStraatnaam straatnaam,
            HomoniemToevoeging homoniemToevoeging,
            AdresDetailPostinfo postInfo,
            AddressPosition adresPositie,
            AdresStatus status,
            Taal taal,
            bool? officieelToegekend,
            DateTimeOffset version,
            string selfDetailUrl,
            string parcelLinkUrl,
            string buildingUnitLinkUrl,
            string? lastEventHash = null)
        {
            Context = contextUrlDetail;
            Identificator = new AdresIdentificator(naamruimte, objectId, version);
            Huisnummer = huisnummer;
            HuisnummerObject = huisnummerObject;
            Busnummer = busnummer;
            AdresStatus = status;
            OfficieelToegekend = officieelToegekend ?? false;
            Postinfo = postInfo;
            Gemeente = gemeente;
            Straatnaam = straatnaam;
            HomoniemToevoeging = homoniemToevoeging;
            AdresPositie = adresPositie;
            LastEventHash = lastEventHash;

            VolledigAdres = new VolledigAdres(
                straatnaam?.Straatnaam?.GeografischeNaam?.Spelling,
                huisnummer,
                busnummer,
                postInfo?.ObjectId,
                gemeente?.Gemeentenaam?.GeografischeNaam?.Spelling,
                taal);

            Links = new AddressDetailOsloResponseLinks(
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
    /// De hyperlinks die gerelateerd zijn aan het adres.
    /// </summary>
    [DataContract(Name = "_links", Namespace = "")]
    public class AddressDetailOsloResponseLinks
    {
        [DataMember(Name = "self")]
        [JsonProperty(Required = Required.DisallowNull)]
        public Link Self { get; set; }

        [DataMember(Name = "gebouweenheden", EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Link? Gebouweenheden { get; set; }

        [DataMember(Name = "percelen", EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Link? Percelen { get; set; }

        public AddressDetailOsloResponseLinks(
            Link self,
            Link? gebouweenheden = null,
            Link? percelen = null)
        {
            Self = self;
            Gebouweenheden = gebouweenheden;
            Percelen = percelen;
        }
    }

    public class AddressDetailOsloResponseExamples : IExamplesProvider<AddressDetailOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressDetailOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressDetailOsloResponse GetExamples()
        {
            var gml =
                "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>";
            var addressPosition = new AddressPosition(new GmlJsonPoint(gml),
                PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Gebouw);
            var adresDetailHuisnummer = new AdresDetailHuisnummerObject(59, string.Format(_responseOptions.DetailUrl, 59));
            var gemeente = new AdresDetailGemeente("9000", string.Format(_responseOptions.GemeenteDetailUrl, "9000"),
                new GeografischeNaam("Gent", Taal.NL));
            var straat = new AdresDetailStraatnaam("748", string.Format(_responseOptions.StraatnaamDetailUrl, "748"),
                new GeografischeNaam("Teststraat", Taal.NL));
            var postInfo = new AdresDetailPostinfo("9000", string.Format(_responseOptions.PostInfoDetailUrl, "9000"));
            var homoniem = new HomoniemToevoeging(new GeografischeNaam("UK", Taal.NL));

            return new AddressDetailOsloResponse(
                _responseOptions.Naamruimte,
                _responseOptions.ContextUrlDetail,
                "60",
                "42",
                adresDetailHuisnummer,
                "5B",
                gemeente,
                straat,
                homoniem,
                postInfo,
                addressPosition,
                AdresStatus.InGebruik,
                Taal.NL,
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v2")
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v2")
            };
    }
}
