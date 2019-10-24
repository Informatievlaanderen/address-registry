namespace AddressRegistry.Api.Legacy.Address.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class AddressBosaResponse
    {
        /// <summary>
        /// Collection of municipality instances (count lower or equal to Limit)
        /// </summary>
        [DataMember(Name = "Adressen")]
        public List<AddressBosaResponseItem> Adressen { get; set; }

        /// <summary>
        /// The total number of municipalities matching the request
        /// </summary>
        [DataMember(Name = "TotaalAantal")]
        public long TotaalAantal { get; set; }

        public AddressBosaResponse()
        {
            Adressen = new List<AddressBosaResponseItem>();
        }
    }

    [DataContract]
    public class AddressBosaResponseItem
    {
        /// <summary>
        /// The identifier of the address
        /// </summary>
        [DataMember(Name = "Identificator")]
        public AdresIdentificator Identificator { get; set; }

        [DataMember(Name = "AdresStatus")]
        public AdresStatus AdresStatus { get; set; }

        [DataMember(Name = "Huisnummer")]
        public string Huisnummer { get; set; }

        [DataMember(Name = "Busnummer", EmitDefaultValue = false)]
        public string Busnummer { get; set; }

        /// <summary>
        /// The address position (a point with Lambert-72 coordinates)
        /// </summary>
        [DataMember(Name = "AdresPositie")]
        public Point AdresPositie { get; set; }

        /// <summary>
        /// The method used to provide the position
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode")]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        /// <summary>
        /// The specification of the object represented by the position
        /// </summary>
        [DataMember(Name = "PositieSpecificatie")]
        public PositieSpecificatie PositieSpecificatie { get; set; }

        [DataMember(Name = "IsOfficieelToegekend")]
        public bool IsOfficieelToegekend { get; set; }

        [DataMember(Name = "HeeftStraatnaam")]
        public StraatnaamIdentificator HeeftStraatnaam { get; set; }

        [DataMember(Name = "HeeftGemeente")]
        public GemeenteIdentificator HeeftGemeente { get; set; }

        [DataMember(Name = "HeeftPostInfo")]
        public PostinfoIdentificator HeeftPostInfo { get; set; }

        public AddressBosaResponseItem(
            string postinfoNamespace,
            string municipalityNamespace,
            string streetNameNamespace,
            string addressNamespace,
            int persistentLocalId,
            AdresStatus status,
            string houseNumber,
            string boxNumber,
            bool? isOfficial,
            Point addressPosition,
            PositieGeometrieMethode positionGeometryMethod,
            PositieSpecificatie positionSpecification,
            DateTimeOffset version,
            string straatnaamId,
            DateTimeOffset straatnaamVersion,
            string gemeenteId,
            DateTimeOffset gemeenteVersion,
            string postalCode,
            DateTimeOffset postinfoVersion)
        {
            Identificator = new AdresIdentificator(addressNamespace, persistentLocalId.ToString(), version);
            AdresStatus = status;
            Huisnummer = houseNumber;
            Busnummer = boxNumber;
            IsOfficieelToegekend = isOfficial ?? false;
            AdresPositie = addressPosition;
            PositieGeometrieMethode = positionGeometryMethod;
            PositieSpecificatie = positionSpecification;
            HeeftPostInfo = new PostinfoIdentificator(postinfoNamespace, postalCode, postinfoVersion);
            HeeftGemeente = new GemeenteIdentificator(municipalityNamespace, gemeenteId, gemeenteVersion);
            HeeftStraatnaam = new StraatnaamIdentificator(streetNameNamespace, straatnaamId, straatnaamVersion);
        }
    }

    public class AddressBosaResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public AddressBosaResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            var addressBosaResponseItems = new List<AddressBosaResponseItem>
            {
                new AddressBosaResponseItem(
                    _responseOptions.PostInfoNaamruimte,
                    _responseOptions.GemeenteNaamruimte,
                    _responseOptions.StraatNaamNaamruimte,
                    _responseOptions.Naamruimte,
                    123,
                    AdresStatus.InGebruik,
                    "26",
                    "2",
                    true,
                    new Point
                    {
                        JsonPoint = new GeoJSONPoint
                        {
                            Type = "point",
                            Coordinates = new[] { 140252.76, 198794.27 }
                        },
                        XmlPoint = new GmlPoint
                        {
                            Pos = "140252.76 198794.27"
                        }
                    },
                    PositieGeometrieMethode.Geinterpoleerd,
                    PositieSpecificatie.Gebouweenheid,
                    DateTimeOffset.Now,
                    "1",
                    DateTimeOffset.Now,
                    "11002",
                    DateTimeOffset.Now,
                    "8000",
                    DateTimeOffset.Now
                   ),

                new AddressBosaResponseItem(
                    _responseOptions.PostInfoNaamruimte,
                    _responseOptions.GemeenteNaamruimte,
                    _responseOptions.StraatNaamNaamruimte,
                    _responseOptions.Naamruimte,
                    48711,
                    AdresStatus.InGebruik,
                    "13",
                    "a",
                    true,
                    new Point
                    {
                        JsonPoint = new GeoJSONPoint
                        {
                            Type = "point",
                            Coordinates = new[] { 140152.76, 198694.27 }
                        },
                        XmlPoint = new GmlPoint
                        {
                            Pos = "140152.76 198694.27"
                        }
                    },
                    PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie.Straat,
                    DateTimeOffset.Now,
                    "9645",
                    DateTimeOffset.Now,
                    "11001",
                    DateTimeOffset.Now,
                    "9000",
                    DateTimeOffset.Now
                ),
            };

            return new AddressBosaResponse
            {
                Adressen = addressBosaResponseItems,
                TotaalAantal = 3
            };
        }
    }
}
