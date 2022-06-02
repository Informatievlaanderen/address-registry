namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class AddressId : GuidValueObject<AddressId>
    {
        public AddressId(Guid addressId) : base(addressId) { }

        public static AddressId Default => new AddressId(Guid.Empty);
    }
}
