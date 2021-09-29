namespace AddressRegistry.Api.Legacy.Address.Responses
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
    using Newtonsoft.Json;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "AdresDetail", Namespace = "")]
    public class AddressResponse
    {
        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De gemeente die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Gemeente", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresDetailGemeente Gemeente { get; set; }

        /// <summary>
        /// Een PostInfo object dat deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Postinfo", Order = 3)]
        [JsonProperty(Required = Required.Default)]
        public AdresDetailPostinfo Postinfo { get; set; }

        /// <summary>
        /// Een straatnaam die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public AdresDetailStraatnaam Straatnaam { get; set; }

        /// <summary>
        /// Homoniem toevoeging aan de straatnaam.
        /// </summary>
        [DataMember(Name = "HomoniemToevoeging", Order = 5, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HomoniemToevoeging HomoniemToevoeging { get; set; }

        /// <summary>
        /// Het huisnummer.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 6)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het nummer van de bus.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 7, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 8)]
        [JsonProperty(Required = Required.DisallowNull)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// De positie van het adres.
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 9)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Point AdresPositie { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 10)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van het object, voorgesteld door de positie.
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 11)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PositieSpecificatie PositieSpecificatie { get; set; }

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

        public AddressResponse(
            string naamruimte,
            string objectId,
            string huisnummer,
            string busnummer,
            AdresDetailGemeente gemeente,
            AdresDetailStraatnaam straatnaam,
            HomoniemToevoeging homoniemToevoeging,
            AdresDetailPostinfo postInfo,
            Point adresPositie,
            PositieGeometrieMethode positieGeometrieMethode,
            PositieSpecificatie positieSpecificatie,
            AdresStatus status,
            Taal taal,
            bool? officieelToegekend,
            DateTimeOffset version)
        {
            Identificator = new AdresIdentificator(naamruimte, objectId, version);
            Huisnummer = huisnummer;
            Busnummer = busnummer;
            PositieGeometrieMethode = positieGeometrieMethode;
            PositieSpecificatie = positieSpecificatie;
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

    public class AddressResponseExamples : IExamplesProvider<AddressResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public AddressResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
         => _responseOptions = responseOptionsProvider.Value;

        public AddressResponse GetExamples()
        {
            var point = new Point
            {
                XmlPoint = new GmlPoint { Pos = "140252.76 198794.27" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { 140252.76, 198794.27 } }
            };

            var gemeente = new AdresDetailGemeente("9000", string.Format(_responseOptions.GemeenteDetailUrl, "9000"), new GeografischeNaam("Gent", Taal.NL));
            var straat = new AdresDetailStraatnaam("748", string.Format(_responseOptions.StraatnaamDetailUrl, "748"), new GeografischeNaam("Teststraat", Taal.NL));
            var postInfo = new AdresDetailPostinfo("9000", string.Format(_responseOptions.PostInfoDetailUrl, "9000"));
            var homoniem = new HomoniemToevoeging(new GeografischeNaam("UK", Taal.NL));

            return new AddressResponse(
                _responseOptions.Naamruimte,
                "60",
                "42",
                "5B",
                gemeente,
                straat,
                homoniem,
                postInfo,
                point,
                PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie.Gebouw,
                AdresStatus.InGebruik,
                Taal.NL,
                true,
                DateTimeOffset.Now);
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
