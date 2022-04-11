namespace AddressRegistry.Infrastructure.Repositories
{
    using Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using SqlStreamStore;

    public class Addresses : Repository<Address, AddressId>, IAddresses
    {
        public Addresses(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(Address.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}
