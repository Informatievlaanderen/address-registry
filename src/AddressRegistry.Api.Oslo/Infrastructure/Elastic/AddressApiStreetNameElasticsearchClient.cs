namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using Consumer.Read.StreetName.Projections.Elastic;
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.Extensions.Logging;

    public sealed partial class AddressApiStreetNameElasticsearchClient: AddressApiElasticsearchClientBase, IAddressApiStreetNameElasticsearchClient
    {
        private readonly ILogger<AddressApiElasticsearchClient> _logger;

        private readonly string _fullStreetNames = $"{ToCamelCase(nameof(StreetNameSearchDocument.FullStreetNames))}";

        public AddressApiStreetNameElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias,
            ILoggerFactory loggerFactory)
            : base(elasticsearchClient, indexAlias)
        {
            _logger = loggerFactory.CreateLogger<AddressApiElasticsearchClient>();
        }
    }
}
