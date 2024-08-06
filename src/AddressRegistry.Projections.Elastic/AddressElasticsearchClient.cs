namespace AddressRegistry.Projections.Elastic
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using Exceptions;
    using global::Elastic.Clients.Elasticsearch;

    public interface IAddressElasticsearchClient
    {
        Task CreateDocument(AddressSearchDocument document, CancellationToken ct);
        Task UpdateDocument(int addressPersistentLocalId, CancellationToken ct);
    }

    public class AddressElasticsearchClient : IAddressElasticsearchClient
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly IndexName _indexName;

        public AddressElasticsearchClient(
            ElasticsearchClient elasticClient,
            IndexName indexName)
        {
            _elasticClient = elasticClient;
            _indexName = indexName;
        }

        public async Task CreateDocument(AddressSearchDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.IndexAsync(document, _indexName, new Id(document.AddressPersistentLocalId), ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
            }
        }

        public async Task UpdateDocument(int addressPersistentLocalId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
