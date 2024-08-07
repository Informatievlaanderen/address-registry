namespace AddressRegistry.Projections.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using Exceptions;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;

    public interface IAddressElasticsearchClient
    {
        Task CreateDocument(AddressSearchDocument document, CancellationToken ct);
        Task PartialUpdateDocument(int addressPersistentLocalId, AddressSearchPartialUpdateDocument document, CancellationToken ct);
        Task UpdateVersionTimestampIfNewer(IEnumerable<int> addressPersistentLocalIds, DateTimeOffset versionTimestamp, CancellationToken ct);
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
                updateRequestDescriptor => { updateRequestDescriptor.Doc(document); }, ct);

            if (!response.IsValidResponse)
            {
                throw new ElasticsearchClientException(response.ApiCallDetails.OriginalException);
            }
        }

        public async Task UpdateVersionTimestampIfNewer(IEnumerable<int> addressPersistentLocalIds, DateTimeOffset versionTimestamp, CancellationToken ct)
        {
            var response = await _elasticClient
                .UpdateByQueryAsync<AddressSearchDocument>(updateByQuery => updateByQuery
                        .Indices(Indices.Index(_indexName))
                        .Query(queryDescriptor =>
                        {
                            queryDescriptor
                                .Bool(configureBool =>
                                {
                                    configureBool
                                        .Must(
                                            q =>
                                            {
                                                q.Ids(new IdsQuery
                                                {
                                                    Values = new Ids(addressPersistentLocalIds.Select(x => new Id(x).ToString()))
                                                });
                                            },
                                            q =>
                                            {
                                                q.Range(new DateRangeQuery(new Field("versionTimestamp"))
                                                {
                                                    Lt = new DateMathExpression(versionTimestamp.ToString("o")),
                                                    Format = "strict_date_time"
                                                });
                                            });
                                });
                        })
                        .Script(script => script
                            .Source("ctx._source.versionTimestamp = params.versionTimestamp")
                            .Params(p => p
                                .Add("versionTimestamp", versionTimestamp)
                            ))
                    , ct);

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
