namespace AddressRegistry.Api.Oslo.Address.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.JsonConverters;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Newtonsoft.Json;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "AdresDetail", Namespace = "")]
    public class AddressOsloResponse
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(PlainStringJsonConverter))]
        public object Context => "[\"https://raw.githubusercontent.com/Informatievlaanderen/OSLOthema-gebouwEnAdres/d44fbba69aeb9f02d10d4e372449c404f3ebd06c/site-skeleton/adressenregister/context/adressen_detail.jsonld\"]";

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
        [DataMember(Name = "Postinfo", Order = 4)]
        [JsonProperty(Required = Required.Default)]
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
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 8, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 9)]
        [JsonProperty(Required = Required.DisallowNull)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// De positie van het adres.
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 10)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AddressPosition AdresPositie { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 11)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresStatus AdresStatus { get; set; }

        /// <summary>
        /// False wanneer het bestaan van het adres niet geweten is ten tijde van administratieve procedures, maar pas na observatie op het terrein.
        /// </summary>
        [DataMember(Name = "OfficieelToegekend", Order = 12)]
        [JsonProperty(Required = Required.DisallowNull)]
        public bool OfficieelToegekend { get; set; }

        public AddressOsloResponse(
            string naamruimte,
            string objectId,
            string huisnummer,
            string busnummer,
            AdresDetailGemeente gemeente,
            AdresDetailStraatnaam straatnaam,
            HomoniemToevoeging homoniemToevoeging,
            AdresDetailPostinfo postInfo,
            AddressPosition adresPositie,
            AdresStatus status,
            Taal taal,
            bool? officieelToegekend,
            DateTimeOffset version)
        {
            Identificator = new AdresIdentificator(naamruimte, objectId, version);
            Huisnummer = huisnummer;
            Busnummer = busnummer;
            AdresStatus = status;
            OfficieelToegekend = officieelToegekend ?? false;
            Postinfo = postInfo;
            Gemeente = gemeente;
            Straatnaam = straatnaam;
            HomoniemToevoeging = homoniemToevoeging;
            AdresPositie = adresPositie;

            VolledigAdres = new VolledigAdres(
                straatnaam?.Straatnaam?.GeografischeNaam?.Spelling,
                huisnummer,
                busnummer,
                postInfo?.ObjectId,
                gemeente?.Gemeentenaam?.GeografischeNaam?.Spelling,
                taal);
        }
    }

    public class AddressOsloResponseExamples : IExamplesProvider<AddressOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public AddressOsloResponse GetExamples()
        {
            var gml = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>";
            var addressPosition = new AddressPosition(new GmlJsonPoint(gml),
                PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Gebouw);
            var gemeente = new AdresDetailGemeente("9000", string.Format(_responseOptions.GemeenteDetailUrl, "9000"),
                new GeografischeNaam("Gent", Taal.NL));
            var straat = new AdresDetailStraatnaam("748", string.Format(_responseOptions.StraatnaamDetailUrl, "748"),
                new GeografischeNaam("Teststraat", Taal.NL));
            var postInfo = new AdresDetailPostinfo("9000", string.Format(_responseOptions.PostInfoDetailUrl, "9000"));
            var homoniem = new HomoniemToevoeging(new GeografischeNaam("UK", Taal.NL));

            return new AddressOsloResponse(
                _responseOptions.Naamruimte,
                "60",
                "42",
                "5B",
                gemeente,
                straat,
                homoniem,
                postInfo,
                addressPosition,
                AdresStatus.InGebruik,
                Taal.NL,
                true,
                DateTimeOffset.Now.ToExampleOffset());
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
            };
    }
}
