namespace AddressRegistry.Api.Legacy.Address.Sync
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Projections.Legacy;

    public sealed record SyndicationAtomContent(string Content);
    public sealed record SyndicationRequest(FilteringHeader<AddressSyndicationFilter> Filtering, SortingHeader Sorting, IPaginationRequest Pagination) : IRequest<SyndicationAtomContent>;

    public sealed class AddressSyndicationHandler : IRequestHandler<SyndicationRequest, SyndicationAtomContent>
    {
        private readonly LegacyContext _legacyContext;
        private readonly IOptions<ResponseOptions> _responseOptions;
        private readonly IConfiguration _configuration;

        public AddressSyndicationHandler(
            LegacyContext legacyContext,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            _legacyContext = legacyContext;
            _responseOptions = responseOptions;
            _configuration = configuration;
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

        private async Task<string> BuildAtomFeed(
            DateTimeOffset lastFeedUpdate,
            PagedQueryable<AddressSyndicationQueryResult> pagedAddresses)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = _configuration.GetSection("Syndication");
                var atomFeedConfig = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastFeedUpdate);

                await writer.WriteDefaultMetadata(atomFeedConfig);

                var addresses = pagedAddresses.Items.ToList();

                var nextFrom = addresses.Any()
                    ? addresses.Max(x => x.Position) + 1
                    : (long?)null;

                var nextUri = BuildNextSyncUri(pagedAddresses.PaginationInfo.Limit, nextFrom, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                {
                    await writer.Write(new SyndicationLink(nextUri, "next"));
                }

                foreach (var address in addresses)
                {
                    await writer.WriteAddress(_responseOptions, formatter, syndicationConfiguration["Category"], address);
                }

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri BuildNextSyncUri(int limit, long? from, string nextUrlBase)
        {
            return from.HasValue
                ? new Uri(string.Format(nextUrlBase, from, limit))
                : null;
        }
    }
}
