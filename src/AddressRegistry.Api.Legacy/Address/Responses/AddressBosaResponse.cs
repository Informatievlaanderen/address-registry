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
    using System.ComponentModel.DataAnnotations;

    public class AddressBosaResponse
    {
        /// <summary>
        /// collection of municipality instances (count lower or equal to Limit)
        /// </summary>
        [Required]
        public List<AddressBosaResponseItem> Adressen { get; set; }

        /// <summary>
        /// the total number of municipalities matching the request
        /// </summary>
        [Required]
        public long TotaalAantal { get; set; }
    }

    public class AddressBosaResponseItem
    {
        /// <summary>
        /// the identifier of the street name
        /// </summary>
        [Required]
        public Identificator Identificator { get; set; }

        [Required]
        public AdresStatus AdresStatus { get; set; }

        [Required]
        public string Huisnummer { get; set; }

        public string Busnummer { get; set; }

        /// <summary>
        /// the address position (a point with Lambert-72 coordinates)
        /// </summary>
        [Required]
        public Point AdresPositie { get; set; }

        /// <summary>
        /// the method used to provide the position
        /// </summary>
        [Required]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        /// <summary>
        /// the specification of the object represented by the position
        /// </summary>
        [Required]
        public PositieSpecificatie PositieSpecificatie { get; set; }

        public bool IsOfficieelToegekend { get; set; }

        [Required]
        public Identificator HeeftStraatnaam { get; set; }

        [Required]
        public Identificator HeeftGemeente { get; set; }

        [Required]
        public Identificator HeeftPostInfo { get; set; }

        public AddressBosaResponseItem(
            string postinfoNamespace,
            string municipalityNamespace,
            string streetNameNamespace,
            string addressNamespace,
            int osloId,
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
            Identificator = new Identificator(addressNamespace, osloId.ToString(), version);
            AdresStatus = status;
            Huisnummer = houseNumber;
            Busnummer = boxNumber;
            IsOfficieelToegekend = isOfficial ?? false;
            AdresPositie = addressPosition;
            PositieGeometrieMethode = positionGeometryMethod;
            PositieSpecificatie = positionSpecification;
            HeeftPostInfo = new Identificator(postinfoNamespace, postalCode, postinfoVersion);
            HeeftGemeente = new Identificator(municipalityNamespace, gemeenteId, gemeenteVersion);
            HeeftStraatnaam = new Identificator(streetNameNamespace, straatnaamId, straatnaamVersion);
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
                            Coordinates = new double[] { 140252.76, 198794.27 }
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
                            Coordinates = new double[] { 140152.76, 198694.27 }
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

            return new AddressBosaResponse()
            {
                Adressen = addressBosaResponseItems,
                TotaalAantal = 3
            };
        }
    }
}
