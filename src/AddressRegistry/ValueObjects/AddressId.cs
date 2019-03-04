namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;
    using System;

    public class AddressId : GuidValueObject<AddressId>
    {
        public static AddressId CreateFor(CrabHouseNumberId crabHouseNumberId)
            => new AddressId(crabHouseNumberId.CreateDeterministicId());

        public static AddressId CreateFor(CrabSubaddressId crabSubaddressId)
            => new AddressId(crabSubaddressId.CreateDeterministicId());

        public AddressId([JsonProperty("value")] Guid addressId) : base(addressId) { }
    }
}
