namespace AddressRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using SqlStreamStore;
    using StreetName;

    public class StreetNames : Repository<StreetName, StreetNameStreamId>, IStreetNames
    {
        public StreetNames(IStreetNameFactory streetNameFactory, ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, ISnapshotStore snapshotStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(streetNameFactory.Create, unitOfWork, eventStore, snapshotStore, eventMapping, eventDeserializer) { }
    }
}
