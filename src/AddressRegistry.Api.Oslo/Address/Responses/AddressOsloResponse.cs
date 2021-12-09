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
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(PlainStringJsonConverter))]
        public object Context => @"{
    ""@base"": ""https://basisregisters.vlaanderen.be/ns/adres"",
    ""@vocab"": ""#"",
    ""identificator"": ""@nest"",
    ""id"": ""@id"",
    ""versieId"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""naamruimte"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#naamruimte"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""objectId"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#lokaleIdentificator"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""Adres"": ""https://data.vlaanderen.be/ns/adres#Adres"",
    ""adresStatus"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#Adres.status"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/concept/adresstatus/""
      }
    },
    ""huisnummer"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#huisnummer"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""officieelToegekend"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#officieelToegekend"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#boolean""
    },
    ""postinfo"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#heeftPostinfo"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/postinfo/"",
        ""objectId"": ""@id""
      }
    },
    ""volledigAdres"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#isVerrijktMet"",
      ""@type"": ""@id"",
      ""@context"": {
        ""geografischeNaam"": {
          ""@id"": ""https://data.vlaanderen.be/ns/adres#volledigAdres"",
          ""@context"": {
            ""spelling"": ""@value"",
            ""taal"": ""@language""
          }
        }
      }
    },
    ""gemeente"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#heeftGemeentenaam"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/gemeente/"",
        ""objectId"": ""@id"",
        ""gemeentenaam"": ""@nest"",
        ""geografischeNaam"": {
          ""@id"": ""http://www.w3.org/2000/01/rdf-schema#label"",
          ""@context"": {
            ""spelling"": ""@value"",
            ""taal"": ""@language""
          }
        }
      }
    },
    ""straatnaam"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#heeftStraatnaam"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/straatnaam/"",
        ""objectId"": ""@id"",
        ""straatnaam"": ""@nest"",
        ""geografischeNaam"": {
          ""@id"": ""http://www.w3.org/2000/01/rdf-schema#label"",
          ""@context"": {
            ""spelling"": ""@value"",
            ""taal"": ""@language""
          }
        }
      }
    },
    ""adresPositie"": {
      ""@id"": ""https://data.vlaanderen.be/ns/adres#positie"",
      ""@type"": ""@id"",
      ""@context"": {
        ""Point"": ""http://www.opengis.net/ont/sf#Point"",
        ""type"": ""@type"",
        ""coordinates"": {
          ""@type"": ""@json"",
          ""@id"": ""https://data.europa.eu/m8g/coordinates""
        },
        ""positieGeometrieMethode"": ""https://data.vlaanderen.be/id/conceptscheme/geometriemethode"",
        ""positieSpecificatie"": ""https://data.vlaanderen.be/id/conceptscheme/geometriespecificatie"",
        ""gml"": {
          ""@id"": ""http://www.opengis.net/ont/geosparql#asGML"",
          ""@type"": ""http://www.opengis.net/ont/geosparql#gmlLiteral""
        }
      }
    },
    ""detail"": ""http://www.iana.org/assignments/relation/self""
  }";

        /// <summary>
        /// Het linked-data type van het Adres.
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
        public Point AdresPositie { get; set; }

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
            Point adresPositie,
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
            var point = new Point
            {
                XmlPoint = new GmlPoint { Pos = "140252.76 198794.27" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { 140252.76, 198794.27 },
                //TODO: Uncomment this after installing new nupkg of grar-common
                //Gml = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>",
                //PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                //PositieSpecificatie = PositieSpecificatie.Gebouw,
                }
            };

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
                point,
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
