namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using ValueObjects;

    public interface IAddresses : IAsyncRepository<Address, AddressId> { }
}
