namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IStreetNames : IAsyncRepository<StreetName, StreetNameStreamId> { }

    public class StreetNameStreamId : ValueObject<StreetNameStreamId>
    {
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public StreetNameStreamId(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            _streetNamePersistentLocalId = streetNamePersistentLocalId;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return _streetNamePersistentLocalId;
        }

        public override string ToString() => $"streetname-{_streetNamePersistentLocalId}";
    }
}
