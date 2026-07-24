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
        public const string StreetNameId = "heeftStraatnaam";
        public const string StatusName = "status";
        public const string HouseNumber = "huisnummer";
        public const string BoxNumber = "busnummer";
        public const string PostalCode = "heeftPostinfo";
        public const string Position = "positie.geometrie";
        public const string PositionGeometryMethod = "positie.methode";
        public const string PositionSpecification = "positie.specificatie";
        public const string OfficiallyAssigned = "officieelToegekend";
    }

    public sealed class AddressCloudTransformEvent
    {
        [JsonProperty("transformaties", Order = 0)]
        public required List<AddressCloudTransformEventValue> TransformValues { get; set; }

        [JsonProperty("nisCodes", Order = 1)]
        public required List<string> NisCodes { get; set; }
    }

    public sealed class AddressCloudTransformEventValue
    {
        [JsonProperty("vanId", Order = 0)]
        public required string From { get; set; }

        [JsonProperty("naarId", Order = 1)]
        public required string To { get; set; }
    }
}
