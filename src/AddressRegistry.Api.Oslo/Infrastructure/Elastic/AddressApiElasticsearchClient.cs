namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.Extensions.Logging;

    public sealed partial class AddressApiElasticsearchClient : IAddressApiElasticsearchClient
    {
        private const string Keyword = "keyword";
        private static readonly string NameSpelling = $"{ToCamelCase(nameof(Projections.Elastic.AddressSearch.Name.Spelling))}";

        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly string _indexAlias;
        private readonly ILogger<AddressApiElasticsearchClient> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AddressApiElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias,
            ILoggerFactory loggerFactory)
        {
            _elasticsearchClient = elasticsearchClient;
            _indexAlias = indexAlias;
            _logger = loggerFactory.CreateLogger<AddressApiElasticsearchClient>();

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
        }
    }
}
