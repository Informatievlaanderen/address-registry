namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class AddressId : GuidValueObject<AddressId>
    {
        public static AddressId CreateFor(CrabHouseNumberId crabHouseNumberId)
            => new AddressId(crabHouseNumberId.CreateDeterministicId());

        public static AddressId CreateFor(CrabSubaddressId crabSubaddressId)
            => new AddressId(crabSubaddressId.CreateDeterministicId());

        public AddressId([JsonProperty("value")] Guid addressId) : base(addressId) { }
    }
}
