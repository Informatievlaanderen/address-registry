namespace AddressRegistry.Api.Backoffice.Address.Requests
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Newtonsoft.Json;
    using TODO_MOVE_TO.Grar.Common;

    public class AddHouseNumberRequest
    {
        [JsonProperty("heeftStraatnaam")]
        public Uri StreetNameId { get; set; }

        [JsonProperty("huisnummer")]
        public string HouseNumber { get; set; }

        [JsonProperty("busnummer")]
        public string BoxNumber { get; set; }

        [JsonProperty("heeftPostinfo")]
        public Uri PostalCode { get; set; }

        [JsonProperty("status")]
        public Uri Status { get; set; }

        [JsonProperty("officieelToegekend")]
        public bool OfficiallyAssigned { get; set; }

        [JsonProperty("positie")]
        public AddressPositionRequest Position { get; set; }

        internal AdresStatus AddressStatus
            => Status.AsIdentifier().AsAddressStatus();
    }

    public class AddressPositionRequest
    {
        [JsonProperty("methode")]
        public Uri Method { get; set; }

        [JsonProperty("specificatie")]
        public Uri Specification { get; set; }

        [JsonProperty("geometrie")]
        public GeoJSONPoint Point { get; set; }

        internal PositieGeometrieMethode AddressPositionMethod
            => Method.AsIdentifier().AsAddressPositionMethod();

        internal PositieSpecificatie AddressPositionSpecification
            => Specification.AsIdentifier().AsAddressPositionSpecification();
    }
}
