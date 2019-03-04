namespace AddressRegistry.Tests.AutoFixture
{
    using global::AutoFixture;

    public class WithDefaults : CompositeCustomization
    {
        public WithDefaults(bool forSubaddress = false) : base(
            new NodaTimeCustomization(),
            new SetProvenanceImplementationsCallSetProvenance(),
            forSubaddress ? (ICustomization) new WithFixedSubaddressId() : new WithFixedAddressId(),
            new WithFixedStreetNameId(),
            //forSubaddress ? (ICustomization) new WithFixedBoxNumber(): new WithFixedHouseNumber(),
            new WithFixedCrabHouseNumberId(),
            new WithFixedHouseNumber(),
            new WithInfiniteLifetime(),
            new WithNoDeleteModification(),
            new WithWkbGeometry()
            )
        {
        }
    }
}
