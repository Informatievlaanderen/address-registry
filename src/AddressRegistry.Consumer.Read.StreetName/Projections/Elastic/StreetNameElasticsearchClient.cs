namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using global::Elastic.Clients.Elasticsearch;

    public interface IStreetNameElasticsearchClient
    {
        Task CreateDocument(StreetNameSearchDocument document, CancellationToken ct);
        Task<ICollection<StreetNameSearchDocument>> GetDocuments(IEnumerable<int> streetNamePersistentLocalIds, CancellationToken ct);
        Task UpdateDocument(StreetNameSearchDocument document, CancellationToken ct);
        Task PartialUpdateDocument(int streetNamePersistentLocalId, StreetNameSearchPartialDocument document, CancellationToken ct);
        Task DeleteDocument(int streetNamePersistentLocalId, CancellationToken ct);
    }

    public class StreetNameElasticsearchClient : IStreetNameElasticsearchClient
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly IndexName _indexName;

        public StreetNameElasticsearchClient(
            ElasticsearchClient elasticClient,
            IndexName indexName)
        {
            _elasticClient = elasticClient;
            _indexName = indexName;
        }

        public async Task CreateDocument(StreetNameSearchDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.IndexAsync(document, _indexName, new Id(document.StreetNamePersistentLocalId), ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to create a document", response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        public async Task UpdateDocument(StreetNameSearchDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.IndexAsync(document, _indexName, new Id(document.StreetNamePersistentLocalId), ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to update a document", response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        public async Task<ICollection<StreetNameSearchDocument>> GetDocuments(IEnumerable<int> streetNamePersistentLocalIds, CancellationToken ct)
        {
            var persistentLocalIds = streetNamePersistentLocalIds.ToList();
            if (persistentLocalIds.Count == 0)
            {
                return new List<StreetNameSearchDocument>();
            }

            var response = await _elasticClient.MultiGetAsync<StreetNameSearchDocument>(_indexName,
                configureRequest =>
                {
                    configureRequest.Ids(new Ids(persistentLocalIds.Select(x => new Id(x).ToString())));
                }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to get documents", response.ElasticsearchServerError, response.DebugInformation);
            }

            var result = new List<StreetNameSearchDocument>();

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

        public async Task PartialUpdateDocument(int streetNamePersistentLocalId, StreetNameSearchPartialDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.UpdateAsync<StreetNameSearchDocument, StreetNameSearchPartialDocument>(
                _indexName,
                new Id(streetNamePersistentLocalId),
                updateRequestDescriptor => { updateRequestDescriptor.Doc(document); }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed trying to do a partial document update", response.ElasticsearchServerError, response.DebugInformation);
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
                throw new ElasticsearchClientException("Failed trying to delete a document", response.ElasticsearchServerError, response.DebugInformation);
            }
        }
    }
}
