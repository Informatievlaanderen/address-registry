namespace AddressRegistry.Projections.Elastic
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Exceptions;
    using global::Elastic.Clients.Elasticsearch;
    using NodaTime;
    using StreetName;

    public interface IAddressElasticsearchClient
    {
        Task CreateDocument(AddressSearchDocument document, CancellationToken ct);
        Task PartialUpdateDocument(int addressPersistentLocalId, AddressSearchPartialUpdateDocument document, CancellationToken ct);
        Task DeleteDocument(int addressPersistentLocalId, CancellationToken ct);
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

        public async Task PartialUpdateDocument(int addressPersistentLocalId, AddressSearchPartialUpdateDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.UpdateAsync<AddressSearchDocument, AddressSearchPartialUpdateDocument>(
                _indexName,
                new Id(addressPersistentLocalId),
                updateRequestDescriptor =>
            {
                updateRequestDescriptor.Doc(document);
            }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
            }
        }

        public async Task DeleteDocument(int addressPersistentLocalId, CancellationToken ct)
        {
            var response = await _elasticClient.DeleteAsync(
                _indexName,
                new Id(addressPersistentLocalId),
                ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
            }
        }
    }
}
