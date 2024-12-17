namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.Search
{
    using AddressRegistry.Consumer.Read.StreetName.Projections.Elastic;
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.Extensions.Logging;

    public sealed partial class AddressApiStreetNameElasticsearchClient: AddressApiElasticsearchClientBase, IAddressApiStreetNameElasticsearchClient
    {
        private readonly string _fullStreetNames = $"{ToCamelCase(nameof(StreetNameSearchDocument.FullStreetNames))}";

        public AddressApiStreetNameElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias)
            : base(elasticsearchClient, indexAlias)
        { }
    }
}
