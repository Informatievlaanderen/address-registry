namespace AddressRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using SqlStreamStore;
    using StreetName;

    public class StreetNames : Repository<StreetName, StreetNameStreamId>, IStreetNames
    {
        public StreetNames(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(StreetName.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}
