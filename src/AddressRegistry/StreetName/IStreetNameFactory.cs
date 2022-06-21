namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;

    public interface IStreetNameFactory
    {
        public StreetName Create();
    }

    public class StreetNameFactory : IStreetNameFactory
    {
        private readonly ISnapshotStrategy _snapshotStrategy;

        public StreetNameFactory(ISnapshotStrategy snapshotStrategy)
        {
            _snapshotStrategy = snapshotStrategy;
        }

        public StreetName Create()
        {
            return new StreetName(_snapshotStrategy);
        }
    }
}
