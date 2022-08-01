namespace AddressRegistry.Api.BackOffice.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using StreetName;

    public interface IIfMatchHeaderValidator
    {
        public Task<bool> IsValid(
            string? ifMatchHeaderValue,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken);
    }

    public class IfMatchHeaderValidator : IIfMatchHeaderValidator
    {
        private readonly IStreetNames _streetNames;

        public IfMatchHeaderValidator(IStreetNames streetNames)
        {
            _streetNames = streetNames;
        }

        public async Task<bool> IsValid(
            string? ifMatchHeaderValue,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ifMatchHeaderValue))
            {
                return true;
            }

            var ifMatchTag = ifMatchHeaderValue.Trim();
            var lastHash = await GetHash(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            return ifMatchTag == lastHashTag.ToString();
        }

        private async Task<string> GetHash(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _streetNames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), cancellationToken);
            var streetNameHash = aggregate.GetAddressHash(addressPersistentLocalId);
            return streetNameHash;
        }
    }
}
