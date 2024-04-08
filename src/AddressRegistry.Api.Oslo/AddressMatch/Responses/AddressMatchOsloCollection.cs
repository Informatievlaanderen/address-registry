namespace AddressRegistry.Api.Oslo.AddressMatch.Responses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using V2.Matching;

    [DataContract(Name = "AdresMatchCollectie", Namespace = "")]
    public class AddressMatchOsloCollection
    {
        /// <summary>
        /// De linked-data context van het adres.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// De 10 adresmatches met de hoogste score (gesorteerd van hoog naar laag).
        /// </summary>
        [XmlArray(ElementName = "AdresMatches")]
        [XmlArrayItem(ElementName = "AdresMatch")]
        [JsonProperty(PropertyName = "AdresMatches")]
        [DataMember(Name = "AdresMatches", Order = 1)]
        public List<AdresMatchOsloItem> AdresMatches { get; set; }

        /// <summary>
        /// Bevat waarschuwingen met betrekking tot conflicterende input.
        /// </summary>
        [XmlArray(ElementName = "Warnings")]
        [XmlArrayItem(ElementName = "Warning")]
        [JsonProperty(PropertyName = "Warnings")]
        [DataMember(Name = "Warnings", Order = 2)]
        public List<ValidationOsloMessage> Warnings { get; set; }
    }

    [DataContract(Name = "AdresMatch", Namespace = "")]
    public class AdresMatchOsloItem
    {
        /// <summary>
        /// Het linked-data type van het adres.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Adres";

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// URL waarop de details van de laatste versie van het adres gevonden kunnen worden.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Detail { get; set; }

        /// <summary>
        /// De gemeente die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Gemeente", Order = 3, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchOsloItemGemeente Gemeente { get; set; }

        /// <summary>
        /// De postinfo die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Postinfo", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchOsloItemPostinfo? Postinfo { get; set; }

        /// <summary>
        /// De straatnaam die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 5, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchOsloItemStraatnaam Straatnaam { get; set; }

        /// <summary>
        /// De homoniemtoevoeging in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "HomoniemToevoeging", Order = 6, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public HomoniemToevoeging HomoniemToevoeging { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 7, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 8, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Busnummer { get; set; }

        /// <summary>
        /// Adresvoorstelling in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 9, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// De geometrie van het object in gml-formaat.
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 10, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AddressPosition AdresPositie { get; set; }

        /// <summary>
        /// De status van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 13, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresStatus? AdresStatus { get; set; }

        /// <summary>
        /// False wanneer het bestaan van het adres niet geweten is ten tijde van administratieve procedures, maar pas na observatie op het terrein.
        /// </summary>
        [DataMember(Name = "OfficieelToegekend", Order = 14, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public bool? OfficieelToegekend { get; set; }

        /// <summary>
        /// De graad van gelijkenis tussen het gevonden adres en de invoer.
        /// </summary>
        [Range(0.0, 100.0)]
        [DataMember(Name = "Score", Order = 20, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public double Score { get; set; }

        /// <summary>
        /// Een lijst van gerelateerde resources om te achterhalen wat de gelinkte objecten zijn aan het adres.
        /// </summary>
        [DataMember(Name = "Links", Order = 21, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<HateoasLink> Links { get; set; }

        [DataContract(Name = "HateoasLink", Namespace = "")]
        public class HateoasLink
        {
            /// <summary>
            /// De URL van de link.
            /// </summary>
            [DataMember(Name = "href", Order = 1, EmitDefaultValue = false)]
            [JsonProperty(Required = Required.Default)]
            public Uri HRef { get; set; }

            /// <summary>
            /// Welke relatie de link tot het adres heeft.
            /// </summary>
            [DataMember(Name = "rel", Order = 2, EmitDefaultValue = false)]
            [JsonProperty(Required = Required.Default)]
            public string Rel { get; set; }

            /// <summary>
            /// Welke soort HttpMethode het is.
            /// </summary>
            [DataMember(Name = "type", Order = 3, EmitDefaultValue = false)]
            [JsonProperty(Required = Required.Default)]
            public string Type { get; set; }

            public HateoasLink(Uri hRef, string rel, string type)
            {
                HRef = hRef;
                Rel = rel;
                Type = type;
            }
        }

        public static AdresMatchOsloItem Create(AddressMatchScoreableItemV2 scoreableItem,
            ResponseOptions responseOptions)
        {
            return new AdresMatchOsloItem
            {
                Identificator = scoreableItem.Identificator,
                Detail = scoreableItem.Detail,
                Gemeente = scoreableItem.Gemeente,
                Straatnaam = scoreableItem.Straatnaam,
                AdresStatus = scoreableItem.AdresStatus,
                Postinfo = scoreableItem.Postinfo,
                HomoniemToevoeging = scoreableItem.HomoniemToevoeging,
                Huisnummer = scoreableItem.Huisnummer,
                Busnummer = scoreableItem.Busnummer,
                AdresPositie = scoreableItem.AdresPositie,
                VolledigAdres = scoreableItem.VolledigAdres,
                OfficieelToegekend = scoreableItem.OfficieelToegekend,
                Score = scoreableItem.Score,

                Links = scoreableItem.Identificator?.ObjectId is not null
                    ? new List<HateoasLink>
                    {
                        new HateoasLink(
                            new Uri(string.Format(responseOptions.AddressMatchParcelLink,
                                scoreableItem.Identificator.ObjectId)), "percelen", HttpMethods.Get),
                        new HateoasLink(
                            new Uri(string.Format(responseOptions.AddressMatchBuildingUnitLink,
                                scoreableItem.Identificator.ObjectId)), "gebouweenheden", HttpMethods.Get),
                    }
                    : new List<HateoasLink>()
            };
        }
    }

    /// <summary>
    /// De gemeente die deel uitmaakt van het adres.
    /// </summary>
    [DataContract(Name = "AdresMatchItemGemeente", Namespace = "")]
    public class AdresMatchOsloItemGemeente
    {
        /// <summary>
        /// De objectidentificator van de gekoppelde gemeente.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van de gekoppelde gemeente weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        /// <summary>
        /// De gemeentenaam in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "Gemeentenaam", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Gemeentenaam Gemeentenaam { get; set; }
    }

    /// <summary>
    /// De straatnaam die deel uitmaakt van het adres.
    /// </summary>
    [DataContract(Name = "AdresMatchItemStraatnaam", Namespace = "")]
    public class AdresMatchOsloItemStraatnaam
    {
        /// <summary>
        /// De objectidentificator van de gekoppelde straatnaam.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van de gekoppelde straatnaam weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        /// <summary>
        /// De straatnaam in de eerste officiële taal van de gemeente.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Straatnaam Straatnaam { get; set; }

        public static AdresMatchOsloItemStraatnaam
            Create(string objectId, string detail, GeografischeNaam straatnaam) =>
            new AdresMatchOsloItemStraatnaam
            {
                ObjectId = objectId,
                Detail = detail,
                Straatnaam = new Straatnaam(straatnaam)
            };
    }

    /// <summary>
    /// De postinfo die deel uitmaakt van het adres.
    /// </summary>
    [DataContract(Name = "AdresMatchItemPostinfo", Namespace = "")]
    public class AdresMatchOsloItemPostinfo
    {
        /// <summary>
        /// De objectidentificator van de gekoppelde postinfo.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van de gekoppelde postinfo weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        public static AdresMatchOsloItemPostinfo Create(string objectId, string detail) =>
            new AdresMatchOsloItemPostinfo
            {
                ObjectId = objectId,
                Detail = detail,
            };
    }

    public class AddressMatchResponseExamples : IExamplesProvider<AddressMatchOsloCollection>
    {
        private readonly ResponseOptions _options;

        public AddressMatchResponseExamples(IOptions<ResponseOptions> options)
        {
            _options = options.Value;
        }

        public AddressMatchOsloCollection GetExamples()
        {
            return new AddressMatchOsloCollection
            {
                Context = _options.ContextUrlAddressMatch,
                AdresMatches = new List<AdresMatchOsloItem>
                {
                    new AdresMatchOsloItem
                    {
                        Identificator = new AdresIdentificator(_options.Naamruimte, "36416228",
                            DateTimeOffset.Now.ToExampleOffset()),
                        Detail = string.Format(_options.DetailUrl, "36416228"),
                        Gemeente = new AdresMatchOsloItemGemeente
                        {
                            ObjectId = "44021",
                            Detail = string.Format(_options.GemeenteDetailUrl, "44021"),
                            Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Gent", Taal.NL))
                        },
                        Straatnaam = AdresMatchOsloItemStraatnaam.Create("69499",
                            string.Format(_options.StraatnaamDetailUrl, "69499"),
                            new GeografischeNaam("Aalbessenlaan", Taal.NL)),
                        Postinfo = AdresMatchOsloItemPostinfo.Create("9032",
                            string.Format(_options.PostInfoDetailUrl, "9032")),
                        Huisnummer = "14",
                        Busnummer = null,

                        AdresPositie = new AddressPosition(
                            new GmlJsonPoint(
                                "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>"),
                            PositieGeometrieMethode.AangeduidDoorBeheerder,
                            PositieSpecificatie.Gebouw),

                        VolledigAdres = new VolledigAdres(new GeografischeNaam("Aalbessenlaan 14, 9032 Gent", Taal.NL)),
                        AdresStatus = AdresStatus.InGebruik,
                        OfficieelToegekend = false,
                        Score = 89.3,
                        Links = new List<AdresMatchOsloItem.HateoasLink>
                        {
                            new AdresMatchOsloItem.HateoasLink(
                                new Uri(string.Format(_options.AddressMatchParcelLink, "36416228")), "percelen",
                                HttpMethods.Get),
                            new AdresMatchOsloItem.HateoasLink(
                                new Uri(string.Format(_options.AddressMatchBuildingUnitLink, "36416228")),
                                "gebouweenheden", HttpMethods.Get),
                        }
                    }
                },
                Warnings = new List<ValidationOsloMessage>
                {
                    new ValidationOsloMessage { Code = "4", Message = "Onbekende 'Postcode'." }
                }
            };
        }
    }
}
