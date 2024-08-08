namespace AddressRegistry.Projections.Elastic
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using Exceptions;
    using global::Elastic.Clients.Elasticsearch;

    public interface IAddressElasticsearchClient
    {
        Task CreateDocument(AddressSearchDocument document, CancellationToken ct);
        Task<ICollection<AddressSearchDocument>> GetDocuments(IEnumerable<int> addressPersistentLocalIds, CancellationToken ct);
        Task UpdateDocument(AddressSearchDocument document, CancellationToken ct);
        Task PartialUpdateDocument(int addressPersistentLocalId, AddressSearchPartialDocument document, CancellationToken ct);
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

        public async Task UpdateDocument(AddressSearchDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.IndexAsync(document, _indexName, new Id(document.AddressPersistentLocalId), ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
            }
        }

        public async Task<ICollection<AddressSearchDocument>> GetDocuments(IEnumerable<int> addressPersistentLocalIds, CancellationToken ct)
        {
            var response = await _elasticClient.MultiGetAsync<AddressSearchDocument>(_indexName,
                configureRequest =>
                {
                    configureRequest.Ids(new Ids(addressPersistentLocalIds.Select(x => new Id(x).ToString())));
                }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
            }

            var result = new List<AddressSearchDocument>();

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

        public async Task PartialUpdateDocument(int addressPersistentLocalId, AddressSearchPartialDocument document, CancellationToken ct)
        {
            var response = await _elasticClient.UpdateAsync<AddressSearchDocument, AddressSearchPartialDocument>(
                _indexName,
                new Id(addressPersistentLocalId),
                updateRequestDescriptor => { updateRequestDescriptor.Doc(document); }, ct);

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

//         public async Task UpdateVersionTimestampIfNewer(IEnumerable<int> addressPersistentLocalIds, DateTimeOffset versionTimestamp, CancellationToken ct)
//         {
//             var response = await _elasticClient
//                 .UpdateByQueryAsync<AddressSearchDocument>(updateByQuery => updateByQuery
//                     .Indices(Indices.Index(_indexName))
//                     .Query(queryDescriptor =>
//                     {
//                         queryDescriptor
//                             .Bool(configureBool =>
//                             {
//                                 configureBool
//                                     .Must(
//                                         q =>
//                                         {
//                                             q.Ids(new IdsQuery
//                                             {
//                                                 Values = new Ids(addressPersistentLocalIds.Select(x => new Id(x).ToString()))
//                                             });
//                                         },
//                                         q =>
//                                         {
//                                             q.Range(new DateRangeQuery(new Field("versionTimestamp"))
//                                             {
//                                                 // ToString("o") is required to filter correctly using the timezone
//                                                 Lt = new DateMathExpression(versionTimestamp.ToString("o")),
//                                                 Format = "strict_date_time"
//                                             });
//                                         });
//                             });
//                     })
//                     .Script(script => script
//                         .Source("ctx._source.versionTimestamp = params.versionTimestamp")
//                         .Params(p => p
//                             .Add("versionTimestamp", versionTimestamp)
//                         ))
//                 , ct);
//
//             if (!response.IsValidResponse)
//             {
//                 throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
//             }
//         }
    }
}
