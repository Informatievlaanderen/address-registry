namespace AddressRegistry.Projections.Feed.Contract
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public static class AddressEventTypes
    {
        public const string CreateV1 = "basisregisters.address.create.v1";
        public const string UpdateV1 = "basisregisters.address.update.v1";
        public const string DeleteV1 = "basisregisters.address.delete.v1";
        public const string TransformV1 = "basisregisters.address.transform.v1";
    }

    public static class AddressAttributeNames
    {
        public const string StreetNameId = "straatnaam.id";
        public const string StatusName = "adresStatus";
        public const string HouseNumber = "huisnummer";
        public const string BoxNumber = "busnummer";
        public const string PostalCode = "postcode";
        public const string Position = "adresPositie";
        public const string PositionGeometryMethod = "positieGeometrieMethode";
        public const string PositionSpecification = "positieSpecificatie";
        public const string OfficiallyAssigned = "officieelToegekend";
    }

    public sealed class AddressCloudTransformEvent
    {
        [JsonProperty("vanIds", Order = 0)]
        public required List<string> From { get; set; }

        [JsonProperty("naarIds", Order = 1)]
        public required List<string> To { get; set; }
    }

    public sealed class AddressPositionCloudEventValue
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Point";

        [JsonProperty("projectie")]
        public string Projection { get; set; } = "http://www.opengis.net/def/crs/EPSG/0/31370";

        [JsonProperty("gml")]
        public string Gml { get; set; } = string.Empty;

        public AddressPositionCloudEventValue(string gml)
        {
            Gml = gml;
        }
    }
}
