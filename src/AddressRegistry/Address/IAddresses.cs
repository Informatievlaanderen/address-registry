namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy interface and should not be used anymore.")]
    public interface IAddresses : IAsyncRepository<Address, AddressId> { }
}
