namespace AddressRegistry.Api.Legacy.AddressMatch.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.EntityFrameworkCore;
    using V1;
    using V1.Matching;

    [DataContract(Name = "AdresMatchCollectie", Namespace = "")]
    public class AddressMatchCollection
    {
        /// <summary>
        /// De 10 adresmatches met de hoogste score (gesorteerd van hoog naar laag).
        /// </summary>
        [XmlArray(ElementName = "AdresMatches")]
        [XmlArrayItem(ElementName = "AdresMatch")]
        [JsonProperty(PropertyName = "AdresMatches")]
        [DataMember(Name = "AdresMatches", Order = 1)]
        public List<AdresMatchItem> AdresMatches { get; set; }

        /// <summary>
        /// Bevat waarschuwingen met betrekking tot conflicterende input.
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
        public AdresMatchItemGemeente Gemeente { get; set; }

        /// <summary>
        /// De postinfo die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Postinfo", Order = 4, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchItemPostinfo Postinfo { get; set; }

        /// <summary>
        /// De straatnaam die deel uitmaakt van het adres.
        /// </summary>
        [DataMember(Name = "Straatnaam", Order = 5, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public AdresMatchItemStraatnaam Straatnaam { get; set; }

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
        /// Een GML3 punt of een GeoJSON punt, afhankelijk van het Content-Type.
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 10, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public Point AdresPositie { get; set; }

        /// <summary>
        /// De specificatie van het object, voorgesteld door de positie.
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 11, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public PositieSpecificatie? PositieSpecificatie { get; set; }

        /// <summary>
        /// De geometrie methode van de positie.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 12, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

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
        /// De aan het adres gekoppelde objecten.
        /// </summary>
        [XmlArray(ElementName = "AdresseerbareObjecten", Order = 15)]
        [XmlArrayItem(ElementName = "AdresseerbaarObject")]
        [DataMember(Name = "AdresseerbareObjecten", Order = 15, EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "AdresseerbareObjecten", Order = 15, DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default)]
        public List<AdresseerbaarObject> AdresseerbareObjecten { get; set; }

        /// <summary>
        /// De graad van gelijkenis tussen het gevonden adres en de invoer.
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
                AdresseerbareObjecten = buildingContext
                    .BuildingUnits
                    .Include(x => x.Addresses)
                    .Where(x => x.Addresses.Any(y => y.AddressId == scorableItem.AddressId) &&
                                !x.IsRemoved &&
                                x.IsBuildingComplete &&
                                x.IsComplete &&
                                x.PersistentLocalId.HasValue)
                    .Select(x => new { x.PersistentLocalId })
                    .ToList()
                    .Select(matchLatestItem => new AdresseerbaarObject
                    {
                        ObjectId = matchLatestItem.PersistentLocalId.ToString(),
                        ObjectType = ObjectType.Gebouweenheid,
                        Detail = string.Format(
                            responseOptions.GebouweenheidDetailUrl,
                            matchLatestItem.PersistentLocalId.ToString()),
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
                                Detail = string.Format(
                                    responseOptions.PerceelDetailUrl,
                                    matchLatestItem.ParcelPersistentLocalId),
                            })
                            .ToList())
                    .ToList()
            };
        }
    }

    /// <summary>
    /// De gemeente die deel uitmaakt van het adres.
    /// </summary>
    [DataContract(Name = "AdresMatchItemGemeente", Namespace = "")]
    public class AdresMatchItemGemeente
    {
        /// <summary>
        /// De objectidentificator van de gekoppelde gemeente.
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
    public class AdresMatchItemStraatnaam
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

        public static AdresMatchItemStraatnaam Create(string objectId, string detail, GeografischeNaam straatnaam) =>
            new AdresMatchItemStraatnaam
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
    public class AdresMatchItemPostinfo
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
        /// Het object type van het gerelateerde object.
        /// </summary>
        [DataMember(Name = "ObjectType", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public ObjectType ObjectType { get; set; }

        /// <summary>
        /// De identificator van het gekoppelde object.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string ObjectId { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van het gekoppelde object weergeeft.
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
                        Identificator = new AdresIdentificator(_options.Naamruimte, "36416228", DateTimeOffset.Now.ToExampleOffset()),
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
                                Coordinates = new[] { 103024.22, 197113.18 }
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
                                ObjectType = ObjectType.Gebouweenheid,
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
