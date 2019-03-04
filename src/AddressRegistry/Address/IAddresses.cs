namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IAddresses : IAsyncRepository<Address, AddressId> { }
}
