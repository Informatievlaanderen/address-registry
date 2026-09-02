namespace AddressRegistry.Projections.Elastic.AddressList
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using global::Elastic.Clients.Elasticsearch;

    public interface IAddressListElasticClient
    {
        Task CreateDocument(AddressListDocument document, CancellationToken ct);
        Task<ICollection<AddressListDocument>> GetDocuments(IEnumerable<int> addressPersistentLocalIds, CancellationToken ct);
        Task UpdateDocument(AddressListDocument document, CancellationToken ct);
        Task PartialUpdateDocument(int addressPersistentLocalId, AddressListPartialDocument document, CancellationToken ct);
        Task PartialUpdateDocumentIfExists(int addressPersistentLocalId, AddressListPartialDocument document, CancellationToken ct);
        Task DeleteDocument(int addressPersistentLocalId, CancellationToken ct);
    }

    public class AddressListElasticClient : IAddressListElasticClient
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly IndexName _indexName;

        public AddressListElasticClient(
            ElasticsearchClient elasticClient,
            IndexName indexName)
        {
            _elasticClient = elasticClient;
            _indexName = indexName;
        }

        public async Task CreateDocument(AddressListDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.IndexAsync(document, _indexName, new Id(document.AddressPersistentLocalId), ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to create a document", response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        public async Task UpdateDocument(AddressListDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.IndexAsync(document, _indexName, new Id(document.AddressPersistentLocalId), ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to update a document", response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        public async Task<ICollection<AddressListDocument>> GetDocuments(IEnumerable<int> addressPersistentLocalIds, CancellationToken ct)
        {
            var persistentLocalIds = addressPersistentLocalIds.ToList();
            if (persistentLocalIds.Count == 0)
            {
                return new List<AddressListDocument>();
            }

            var response = await _elasticClient.MultiGetAsync<AddressListDocument>(_indexName,
                configureRequest =>
                {
                    configureRequest.Ids(new Ids(persistentLocalIds.Select(x => new Id(x).ToString())));
                }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to get documents", response.ElasticsearchServerError, response.DebugInformation);
            }

            var result = new List<AddressListDocument>();

            foreach (var docResponse in response.Docs)
            {
                docResponse.Match(doc =>
                    {
                        if (doc.Source is not null)
                        {
                            result.Add(doc.Source);
                        }
                    }, error => throw new ElasticsearchClientException($"Failed trying to get document for {error.Id}. Type={error.Error.Type}, Reason={error.Error.Reason}, StackTrace={error.Error.StackTrace}"));
            }

            return result;
        }

        public async Task PartialUpdateDocument(int addressPersistentLocalId, AddressListPartialDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.UpdateAsync<AddressListDocument, AddressListPartialDocument>(
                _indexName,
                new Id(addressPersistentLocalId),
                updateRequestDescriptor => { updateRequestDescriptor.Doc(document); }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to do a partial document update", response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        /// <summary>
        /// Partially updates a document that may no longer be in the index.
        /// </summary>
        /// <remarks>
        /// Removed addresses have their document deleted, and the Lambert 2008 transformation is the one
        /// position event that reaches removed addresses — for those there is nothing to update, which is
        /// not a failure. Every other error still throws. See ADR 0005.
        /// </remarks>
        public async Task PartialUpdateDocumentIfExists(int addressPersistentLocalId, AddressListPartialDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.UpdateAsync<AddressListDocument, AddressListPartialDocument>(
                _indexName,
                new Id(addressPersistentLocalId),
                updateRequestDescriptor => { updateRequestDescriptor.Doc(document); }, ct);

            if (response.IsValidResponse || response.ApiCallDetails.HttpStatusCode == 404)
            {
                return;
            }

            throw new ElasticsearchClientException("Failed trying to do a partial document update", response.ElasticsearchServerError, response.DebugInformation);
        }

        public async Task DeleteDocument(int addressPersistentLocalId, CancellationToken ct)
        {
            var response = await _elasticClient.DeleteAsync(
                _indexName,
                new Id(addressPersistentLocalId),
                ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to delete a document", response.ElasticsearchServerError, response.DebugInformation);
            }
        }
    }
}
