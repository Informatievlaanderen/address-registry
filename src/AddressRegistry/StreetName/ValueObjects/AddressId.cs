namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class AddressId : GuidValueObject<AddressId>
    {
        public AddressId([JsonProperty("value")] Guid addressId) : base(addressId) { }

        public static AddressId Default => new AddressId(Guid.Empty);
    }
}
