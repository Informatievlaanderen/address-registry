namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;

    public sealed class StreetNameExistsValidator
    {
        private readonly IStreamStore _streamStore;

        public StreetNameExistsValidator(IStreamStore streamStore)
        {
            _streamStore = streamStore;
        }

        public async Task<bool> Exists(string streetNameId, CancellationToken cancellationToken)
        {
            var identifier = streetNameId
                .AsIdentifier()
                .Map(x => x);

            if (!int.TryParse(identifier.Value, out var streetNamePersistentLocalId))
            {
                return false;
            }

            return await Exists(new StreetNamePersistentLocalId(streetNamePersistentLocalId), cancellationToken);
        }

        public async Task<bool> Exists(StreetNamePersistentLocalId streetNamePersistentLocalId, CancellationToken cancellationToken)
        {
            var page = await _streamStore.ReadStreamBackwards(
                new StreamId(new StreetNameStreamId(streetNamePersistentLocalId)),
                StreamVersion.End,
                maxCount: 1,
                prefetchJsonData: false,
                cancellationToken);

            return page.Status == PageReadStatus.Success;
        }
    }
}
