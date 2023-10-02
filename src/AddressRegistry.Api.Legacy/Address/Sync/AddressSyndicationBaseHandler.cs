namespace AddressRegistry.Api.Legacy.Address.Sync
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;

    public class AddressSyndicationBaseHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressSyndicationBaseHandler(IConfiguration configuration, IOptions<ResponseOptions> responseOptions)
        {
            _configuration = configuration;
            _responseOptions = responseOptions;
        }

        protected async Task<string> BuildAtomFeed(
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
