namespace AddressRegistry.Api.BackOffice.Infrastructure
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using StreetName;

    public interface IIfMatchHeaderValidator
    {
        public Task<bool> IsValid(
            string? ifMatchHeaderValue,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken);
    }

    public class IfMatchHeaderValidator : IIfMatchHeaderValidator
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IStreetNames _streetNames;

        public IfMatchHeaderValidator(
            BackOfficeContext backOfficeContext,
            IStreetNames streetNames)
        {
            _backOfficeContext = backOfficeContext;
            _streetNames = streetNames;
        }

        public async Task<bool> IsValid(
            string? ifMatchHeaderValue,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ifMatchHeaderValue))
            {
                return true;
            }

            var streetNamePersistentLocalId = GetStreetNamePersistentLocalId(addressPersistentLocalId);

            var ifMatchTag = ifMatchHeaderValue.Trim();
            var lastHash = await GetHash(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            return ifMatchTag == lastHashTag.ToString();
        }


        private StreetNamePersistentLocalId GetStreetNamePersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
        {
            var relation = _backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (relation is null)
            {
                throw new AggregateIdIsNotFoundException();
            }

            return new StreetNamePersistentLocalId(relation.StreetNamePersistentLocalId);
        }

        private async Task<string> GetHash(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate = await _streetNames.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), cancellationToken);
            return aggregate.GetAddressHash(addressPersistentLocalId);
        }
    }
}
