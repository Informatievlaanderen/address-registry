namespace AddressRegistry.Projections.Elastic
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.IndexManagement;
    using global::Elastic.Clients.Elasticsearch.Mapping;
    using Microsoft.Extensions.Configuration;
    using ExistsRequest = global::Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

    public sealed class ElasticIndex
    {
        private readonly ElasticsearchClient _client;

        private readonly string _indexName;
        private readonly string _indexAlias;

        public ElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
        {
            _client = client;
            var elasticOptions = configuration.GetSection("Elastic");
            _indexName = elasticOptions["IndexName"]!;
            _indexAlias = elasticOptions["IndexAlias"]!;
        }

        public async Task CreateAliasIfNotExist(CancellationToken ct)
        {
            var aliasResponse = await _client.Indices.GetAliasAsync(new GetAliasRequest(Names.Parse(_indexAlias)), ct);
            if (aliasResponse.IsValidResponse && aliasResponse.Aliases.Any())
                return;

            await _client.Indices.PutAliasAsync(new PutAliasRequest(_indexName, _indexAlias), ct);
        }

        public async Task CreateIndexIfNotExist(CancellationToken ct)
        {
            var indexName = Indices.Index(_indexName);
            var response = await _client.Indices.ExistsAsync(new ExistsRequest(indexName), ct);
            if (response.Exists)
                return;

            var createResponse = await _client.Indices.CreateAsync<AddressSearchDocument>(indexName, c =>
            {
                c.Mappings(map => map
                    .Properties(p => p
                            .IntegerNumber(x => x.AddressPersistentLocalId)
                            .IntegerNumber(x => x.ParentAddressPersistentLocalId)
                            .Date(x => x.VersionTimestamp)
                            .Keyword(x => x.Status)
                            .Boolean(x => x.Active)
                            .Boolean(x => x.OfficiallyAssigned)
                            .Keyword(x => x.HouseNumber)
                            .Keyword(x => x.BoxNumber)
                            .Object(x => x.AddressPosition, objConfig => objConfig
                                .Properties(obj => obj
                                    .Text(x => x.AddressPosition.GeometryAsWkt)
                                    .GeoPoint(x => x.AddressPosition.GeometryAsWgs84)
                                    .Keyword(x => x.AddressPosition.GeometryMethod)
                                    .Keyword(x => x.AddressPosition.GeometrySpecification)
                                )
                            )
                            .Object(x => x.Municipality, objConfig => objConfig
                                .Properties(obj =>
                                {
                                    obj
                                        .Keyword(x => x.Municipality.NisCode)
                                        .Nested("names", ConfigureNames());
                                })
                            )
                            .Object(x => x.PostalInfo, objConfig => objConfig
                                .Properties(obj => obj
                                    .Keyword(x => x.PostalInfo.PostalCode)
                                    .Nested("names", ConfigureNames())
                                )
                            )
                            .Object(x => x.StreetName, objConfig => objConfig
                                .Properties(obj => obj
                                    .IntegerNumber(x => x.StreetName.StreetNamePersistentLocalId)
                                    .Nested("names", ConfigureNames())
                                    .Nested("homonyms", ConfigureNames())
                                )
                            )
                            .Nested(x => x.FullAddress, ConfigureNames())
                    ));
            }, ct);

            if (!createResponse.Acknowledged || !createResponse.IsValidResponse)
            {
                throw new InvalidOperationException("Failed to create index");
            }
        }

        private Action<NestedPropertyDescriptor<AddressSearchDocument>> ConfigureNames()
        {
            return n => n
                .Properties(np => np
                    .Text("spelling", new TextProperty
                    {
                        Fields = new Properties
                        {
                            { "keyword", new KeywordProperty { IgnoreAbove = 256 } }
                        }
                    })
                    .Keyword("language")
                );
        }
    }
}
