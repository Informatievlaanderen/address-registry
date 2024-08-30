namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.Extensions.Logging;

    public sealed partial class AddressApiElasticsearchClient : IAddressApiElasticsearchClient
    {
        private const string Keyword = "keyword";
        private static readonly string NameSpelling = $"{ToCamelCase(nameof(Projections.Elastic.AddressSearch.Name.Spelling))}";

        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly string _indexAlias;
        private readonly ILogger<AddressApiElasticsearchClient> _logger;

        public AddressApiElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias,
            ILoggerFactory loggerFactory)
        {
            _elasticsearchClient = elasticsearchClient;
            _indexAlias = indexAlias;
            _logger = loggerFactory.CreateLogger<AddressApiElasticsearchClient>();
        }
    }
}
