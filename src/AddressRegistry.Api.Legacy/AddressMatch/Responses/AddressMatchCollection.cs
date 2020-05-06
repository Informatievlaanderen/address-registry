namespace AddressRegistry.Api.Legacy.AddressMatch.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Matching;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Microsoft.EntityFrameworkCore;

    [DataContract(Name = "AdresMatchCollectie", Namespace = "")]
    public class AddressMatchCollection
    {
        /// <summary>
        /// the first 10 found address matches
        /// </summary>
        [XmlArray(ElementName = "AdresMatches")]
        [XmlArrayItem(ElementName = "AdresMatch")]
        [JsonProperty(PropertyName = "AdresMatches")]
        [DataMember(Name = "AdresMatches", Order = 1)]
        public List<AdresMatchItem> AdresMatches { get; set; }

        /// <summary>
        /// contains warnings concerning conflicting information in the input
        /// </summary>
        [XmlArray(ElementName = "Warnings")]
        [XmlArrayItem(ElementName = "Warning")]
        [JsonProperty(PropertyName = "Warnings")]
        [DataMember(Name = "Warnings", Order = 2)]
        public List<ValidationMessage> Warnings { get; set; }
    }

    [DataContract(Name = "AdresMatch", Namespace = "")]
    public class AdresMatchItem
    {
        /// <summary>
        /// the identifier of the address
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// URL returning the details of the latest version of the address
        /// </summary>
        [DataMember(Name = "Detail", Order = 2, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Detail { get; set; }

        /// <summary>
        /// municipality that is part of the address
        /// </summary>
        [DataMember(Name = "Gemeente", Order = 3, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchItemGemeente Gemeente { get; set; }

        /// <summary>
        /// postal information object that is part of the address
        /// </summary>
        [DataMember(Name = "Postinfo", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchItemPostinfo Postinfo { get; set; }

        /// <summary>
        /// street name that is part of the address
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 5, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchItemStraatnaam Straatnaam { get; set; }

        /// <summary>
        /// homonym addition to the street name
        /// </summary>
        [DataMember(Name = "HomoniemToevoeging", Order = 6, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public HomoniemToevoeging HomoniemToevoeging { get; set; }

        /// <summary>
        /// the house number
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 7, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// the mailbox number
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 8, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public string Busnummer { get; set; }

        /// <summary>
        /// the representation of an address in dutch
        /// </summary>
        [DataMember(Name = "VolledigAdres", Order = 9, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public VolledigAdres VolledigAdres { get; set; }

        /// <summary>
        /// the address position
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 10, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public Point AdresPositie { get; set; }

        /// <summary>
        /// the specification of the object represented by the position
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 11, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public PositieSpecificatie? PositieSpecificatie { get; set; }

        /// <summary>
        /// the method used to provide the position
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 12, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

        /// <summary>
        /// the current phase in the lifecycle of the address
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 13, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresStatus? AdresStatus { get; set; }

        /// <summary>
        /// true if the existence of the address was not known within administrative procedures but only after observation on site
        /// </summary>
        [DataMember(Name = "OfficieelToegekend", Order = 14, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public bool? OfficieelToegekend { get; set; }

        /// <summary>
        /// resources that are coupled to the address
        /// </summary>
        [XmlArray(ElementName = "AdresseerbareObjecten", Order = 15)]
        [XmlArrayItem(ElementName = "AdresseerbaarObject")]
        [DataMember(Name = "AdresseerbareObjecten", Order = 15, EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "AdresseerbareObjecten", Order = 15, DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default)]
        public List<AdresseerbaarObject> AdresseerbareObjecten { get; set; }

        /// <summary>
        /// the grade of similarity between the found address and the input address components
        /// </summary>
        [Range(0.0, 100.0)]
        [DataMember(Name = "Score", Order = 20, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public double Score { get; set; }

        public static AdresMatchItem Create(
            AdresMatchScorableItem scorableItem,
            BuildingContext buildingContext,
            AddressMatchContext addressMatchContext,
            ResponseOptions responseOptions)
        {
            return new AdresMatchItem
            {
                Identificator = scorableItem.Identificator,
                Detail = scorableItem.Detail,
                Gemeente = scorableItem.Gemeente,
                Straatnaam = scorableItem.Straatnaam,
                AdresStatus = scorableItem.AdresStatus,
                Postinfo = scorableItem.Postinfo,
                HomoniemToevoeging = scorableItem.HomoniemToevoeging,
                Huisnummer = scorableItem.Huisnummer,
                Busnummer = scorableItem.Busnummer,
                PositieGeometrieMethode = scorableItem.PositieGeometrieMethode,
                AdresPositie = scorableItem.AdresPositie,
                PositieSpecificatie = scorableItem.PositieSpecificatie,
                VolledigAdres = scorableItem.VolledigAdres,
                OfficieelToegekend = scorableItem.OfficieelToegekend,
                Score = scorableItem.Score,
                AdresseerbareObjecten = buildingContext.BuildingUnits
                        .Include(x => x.Addresses)
                        .Where(x => x.Addresses.Any(y => y.AddressId == scorableItem.AddressId) && !x.IsRemoved && x.IsBuildingComplete && x.IsComplete && x.PersistentLocalId.HasValue)
                        .Select(x => new { x.PersistentLocalId })
                        .ToList()
                        .Select(matchLatestItem => new AdresseerbaarObject
                        {
                            ObjectId = matchLatestItem.PersistentLocalId.ToString(),
                            ObjectType = ObjectType.Gebouweenheid,
                            Detail = string.Format(responseOptions.GebouweenheidDetailUrl, matchLatestItem.PersistentLocalId.ToString()),
                        })
                        .ToList()
                        .Concat(
                            addressMatchContext.ParcelAddressMatchLatestItems
                            .Where(x => x.AddressId == scorableItem.AddressId && !x.IsRemoved)
                            .ToList()
                            .Select(matchLatestItem => new AdresseerbaarObject
                            {
                                ObjectId = matchLatestItem.ParcelPersistentLocalId,
                                ObjectType = ObjectType.Perceel,
                                Detail = string.Format(responseOptions.PerceelDetailUrl, matchLatestItem.ParcelPersistentLocalId),
                            })
                            .ToList())
                        .ToList()
            };
        }
    }

    [DataContract(Name = "AdresMatchItemGemeente", Namespace = "")]
    public class AdresMatchItemGemeente
    {
        /// <summary>
        /// the object identifier of the coupled municipality
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// URL returning the details of the latest version of the coupled municipality
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        /// <summary>
        /// the municipality name in Dutch
        /// </summary>
        [DataMember(Name = "Gemeentenaam", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Gemeentenaam Gemeentenaam { get; set; }
    }

    [DataContract(Name = "AdresMatchItemStraatnaam", Namespace = "")]
    public class AdresMatchItemStraatnaam
    {
        /// <summary>
        /// the object identifier of the coupled street name
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// URL returning the details of the latest version of the coupled street name
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        /// <summary>
        /// the street name in Dutch
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Straatnaam Straatnaam { get; set; }

        public static AdresMatchItemStraatnaam Create(string objectId, string detail, GeografischeNaam straatnaam) =>
            new AdresMatchItemStraatnaam
            {
                ObjectId = objectId,
                Detail = detail,
                Straatnaam = new Straatnaam(straatnaam)
            };
    }

    [DataContract(Name = "AdresMatchItemPostinfo", Namespace = "")]
    public class AdresMatchItemPostinfo
    {
        /// <summary>
        /// the object identifier of the coupled postal information object
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// URL returning the details of the latest version of the coupled postal information object
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        public static AdresMatchItemPostinfo Create(string objectId, string detail) =>
            new AdresMatchItemPostinfo
            {
                ObjectId = objectId,
                Detail = detail,
            };
    }

    public enum ObjectType
    {
        Gebouweenheid,
        Perceel,
        Standplaats,
        Ligplaats
    }

    [DataContract(Name = "AdresseerbaarObject", Namespace = "")]
    public class AdresseerbaarObject
    {
        /// <summary>
        /// the object type of the coupled resource
        /// </summary>
        [DataMember(Name = "ObjectType", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public ObjectType ObjectType { get; set; }

        /// <summary>
        /// the object identifier of the coupled resource
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// URL returning the details of the latest version of the coupled resource
        /// </summary>
        [DataMember(Name = "Detail", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }
    }

    public class AddressMatchResponseExamples : IExamplesProvider<AddressMatchCollection>
    {
        private readonly ResponseOptions _options;

        public AddressMatchResponseExamples(IOptions<ResponseOptions> options)
        {
            _options = options.Value;
        }

        public AddressMatchCollection GetExamples()
        {
            return new AddressMatchCollection
            {
                AdresMatches = new List<AdresMatchItem>
                {
                    new AdresMatchItem
                    {
                        Identificator = new AdresIdentificator(_options.Naamruimte, "36416228", DateTimeOffset.Now),
                        Detail = string.Format(_options.DetailUrl, "36416228"),
                        Gemeente = new AdresMatchItemGemeente
                        {
                            ObjectId = "44021",
                            Detail = string.Format(_options.GemeenteDetailUrl, "44021"),
                            Gemeentenaam =  new Gemeentenaam(new GeografischeNaam("Gent", Taal.NL))
                        },
                        Straatnaam = AdresMatchItemStraatnaam.Create("69499", string.Format(_options.StraatnaamDetailUrl, "69499"), new GeografischeNaam("Aalbessenlaan", Taal.NL)),
                        Postinfo = AdresMatchItemPostinfo.Create("9032", string.Format(_options.PostInfoDetailUrl, "9032")),
                        Huisnummer = "14",
                        Busnummer = null,
                        AdresPositie = new Point
                        {
                            JsonPoint = new GeoJSONPoint
                            {
                                Type = "point",
                                Coordinates = new double[] { 103024.22, 197113.18 }
                            },
                            XmlPoint = new GmlPoint
                            {
                                Pos = "103024.22 197113.18"
                            }
                        },
                        PositieSpecificatie = PositieSpecificatie.Perceel,
                        PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                        VolledigAdres = new VolledigAdres(new GeografischeNaam("Aalbessenlaan 14, 9032 Gent", Taal.NL)),
                        AdresStatus = AdresStatus.InGebruik,
                        AdresseerbareObjecten = new List<AdresseerbaarObject>
                        {
                            new AdresseerbaarObject
                            {
                                ObjectId = "3466",
                                ObjectType =ObjectType.Gebouweenheid,
                                Detail = string.Format(_options.GebouweenheidDetailUrl, "3466")
                            }
                        },
                        OfficieelToegekend = false,
                        Score = 89.3
                    }
                },
                Warnings = new List<ValidationMessage>
                {
                    new ValidationMessage { Code="4", Message = "Onbekende 'Postcode'." }
                }
            };
        }
    }
}
