namespace AddressRegistry.Api.Legacy.Address.Sync
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public sealed record SyndicationAtomContent(string Content);
    public record SyndicationRequest(
        FilteringHeader<AddressSyndicationFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination)
        : IRequest<SyndicationAtomContent>;

    public sealed class AddressSyndicationHandler : AddressSyndicationBaseHandler, IRequestHandler<SyndicationRequest, SyndicationAtomContent>
    {
        private readonly LegacyContext _legacyContext;

        public AddressSyndicationHandler(
            LegacyContext legacyContext,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration) : base (configuration, responseOptions)
        {
            _legacyContext = legacyContext;
        }

        public async Task<SyndicationAtomContent> Handle(SyndicationRequest request, CancellationToken cancellationToken)
        {
            var lastFeedUpdate = await _legacyContext
                .AddressSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
            {
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }

            var pagedAddresses =
                new AddressSyndicationQuery(
                        _legacyContext,
                        request.Filtering.Filter?.Embed)
                    .Fetch(request.Filtering, request.Sorting, request.Pagination);

            return new SyndicationAtomContent(await BuildAtomFeed(lastFeedUpdate, pagedAddresses));
        }
    }
}
