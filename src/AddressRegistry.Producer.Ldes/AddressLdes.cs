namespace AddressRegistry.Producer.Ldes
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class AddressLdes
    {
        private static readonly JObject Context = JObject.Parse(@"
{
  ""@version"": 1.1,
  ""@base"": ""https://basisregisters.vlaanderen.be/implementatiemodel/adressenregister"",
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
  ""busnummer"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#Adresvoorstelling.busnummer"",
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
  ""gemeente"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#heeftGemeentenaam"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/id/gemeente/"",
      ""objectId"": ""@id""
    }
  },
  ""straatnaam"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#heeftStraatnaam"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/id/straatnaam/"",
      ""objectId"": ""@id""
    }
  },
  ""huisnummerObject"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#huisnummerObject"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/id/adres/"",
      ""objectId"": ""@id""
    }
  },
  ""adresPositie"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#positie"",
    ""@type"": ""@id"",
    ""@context"": {
      ""positieGeometrieMethode"": {
        ""@id"": ""https://data.vlaanderen.be/id/conceptscheme/geometriemethode"",
        ""@type"": ""@id"",
        ""@context"": {
          ""@base"": ""https://data.vlaanderen.be/doc/concept/geometriemethode/""
        }
      },
      ""positieSpecificatie"": {
        ""@id"": ""https://data.vlaanderen.be/id/conceptscheme/geometriespecificatie"",
        ""@type"": ""@id"",
        ""@context"": {
          ""@base"": ""https://data.vlaanderen.be/doc/concept/geometriespecificatie/""
        }
      },
      ""geometrie"": {
        ""@id"": ""https://www.w3.org/ns/locn#geometry"",
        ""@context"": {
          ""gml"": {
            ""@id"": ""http://www.opengis.net/ont/geosparql#asGML"",
            ""@type"": ""http://www.opengis.net/ont/geosparql#gmlLiteral""
          },
          ""type"": ""@type"",
          ""@vocab"": ""http://www.opengis.net/ont/sf#""
        }
      }
    }
  }
}");

        [JsonProperty("@context", Order = 0)]
        public JObject LdContext => Context;

        [JsonProperty("@type", Order = 1)]
        public string Type => "Adres";

        [JsonProperty("Identificator", Order = 2)]
        public AdresIdentificator Identificator { get; private set; }

        [JsonProperty("Gemeente", Order = 3)]
        public ObjectIdObject Gemeente { get; private set; }

        [JsonProperty("Postinfo", Order = 4)]
        public ObjectIdObject? Postinfo { get; private set; }

        [JsonProperty("Straatnaam", Order = 5)]
        public ObjectIdObject Straatnaam { get; private set; }

        [JsonProperty("Huisnummer", Order = 6)]
        public string Huisnummer { get; private set; }

        [JsonProperty("HuisnummerObject", Order = 7)]
        public ObjectIdObject? HuisnummerObject { get; private set; }

        [JsonProperty("Busnummer", Order = 8)]
        public string? Busnummer { get; private set; }

        [JsonProperty("AdresPositie", Order = 9)]
        public AdresPositie AdresPositie { get; private set; }

        [JsonProperty("AdresStatus", Order = 10)]
        public AdresStatus AdresStatus { get; private set; }

        [JsonProperty("OfficieelToegekend", Order = 11)]
        public bool OfficieelToegekend { get; private set; }

        [JsonProperty("IsVerwijderd", Order = 12)]
        public bool IsRemoved { get; private set; }

        public AddressLdes(AddressDetail address, string osloNamespace)
        {
            Identificator = new AdresIdentificator(osloNamespace, address.AddressPersistentLocalId.ToString(), address.VersionTimestamp.ToBelgianDateTimeOffset());
            Gemeente = new ObjectIdObject(address.NisCode);
            Postinfo = address.PostalCode is not null
                ? new ObjectIdObject(address.PostalCode)
                : null;
            Straatnaam = new ObjectIdObject(address.StreetNamePersistentLocalId.ToString());
            Huisnummer = address.HouseNumber;
            HuisnummerObject = address.ParentAddressPersistentLocalId is not null
                ? new ObjectIdObject(address.ParentAddressPersistentLocalId.Value.ToString())
                : null;
            Busnummer = address.BoxNumber;
            AdresPositie = AddressMapper.GetAdresPositie(address.Position, address.PositionMethod, address.PositionSpecification);
            AdresStatus = AddressMapper.ConvertToAdresStatus(address.Status);
            OfficieelToegekend = address.OfficiallyAssigned;
            IsRemoved = address.Removed;
        }
    }

    public sealed class ObjectIdObject
    {
        [JsonProperty("ObjectId")]
        public string ObjectId { get; private set; }

        public ObjectIdObject(string objectId)
        {
            ObjectId = objectId;
        }
    }

    public class AdresPositie
    {
        [JsonProperty("Geometrie", Order = 0)]
        public GmlJsonPoint Geometry { get; }

        [JsonProperty("PositieGeometrieMethode", Order = 1)]
        public PositieGeometrieMethode PositieGeometrieMethode { get; }

        [JsonProperty("PositieSpecificatie", Order = 2)]
        public PositieSpecificatie PositieSpecificatie { get; }

        public AdresPositie(GmlJsonPoint geometry,
            PositieGeometrieMethode positieGeometrieMethode,
            PositieSpecificatie positieSpecificatie)
        {
            Geometry = geometry;
            PositieGeometrieMethode = positieGeometrieMethode;
            PositieSpecificatie = positieSpecificatie;
        }
    }
}
