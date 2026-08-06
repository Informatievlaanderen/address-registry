namespace AddressRegistry.Api.Oslo.Address.V2.Sync
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public record SyndicationByPersistentLocalIdRequest(
            FilteringHeader<AddressSyndicationPersistentLocalIdFilter> Filtering,
            SortingHeader Sorting,
            IPaginationRequest Pagination)
        : IRequest<SyndicationAtomContent>;

    public sealed class AddressSyndicationByPersistentLocalIdHandler : AddressSyndicationBaseHandler, IRequestHandler<SyndicationByPersistentLocalIdRequest, SyndicationAtomContent>
    {
        private readonly LegacyContext _legacyContext;

        public AddressSyndicationByPersistentLocalIdHandler(
            LegacyContext legacyContext,
            IOptions<ResponseOptionsV2> responseOptions,
            IConfiguration configuration) : base (configuration, responseOptions)
        {
            _legacyContext = legacyContext;
        }

        public async Task<SyndicationAtomContent> Handle(SyndicationByPersistentLocalIdRequest request, CancellationToken cancellationToken)
        {
            var pagedAddresses =
                new AddressSyndicationPersistentLocalIdQuery(_legacyContext, request.Filtering.Filter?.Embed)
                    .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var lastUpdatedDateTime = pagedAddresses.Items
                .ToList()
                .Last()
                .LastChangedOn
                .ToDateTimeUtc();

            var objectSrid = ObjectCrs.ToSrid(request.Filtering.Filter?.ObjectCrs);

            return new SyndicationAtomContent(await BuildAtomFeed(lastUpdatedDateTime, pagedAddresses, objectSrid));
        }
    }
}
