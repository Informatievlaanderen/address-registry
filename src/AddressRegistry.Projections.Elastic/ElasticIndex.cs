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
    using global::Elastic.Clients.Elasticsearch.Analysis;
    using global::Elastic.Clients.Elasticsearch.Core.Reindex;
    using global::Elastic.Clients.Elasticsearch.IndexManagement;
    using global::Elastic.Clients.Elasticsearch.Mapping;
    using Microsoft.Extensions.Configuration;
    using ExistsRequest = global::Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

    public sealed class ElasticIndex
    {
        public const string AddressSearchNormalizer = "AddressSearchNormalizer";
        public const string AddressSearchAnalyzer = "AddressSearchAnalyzer";

        private readonly ElasticsearchClient _client;
        private readonly string _name;
        private readonly string _alias;

        public ElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
            : this(client, ElasticIndexOptions.LoadFromConfiguration(configuration))
        { }

        public ElasticIndex(
            ElasticsearchClient client,
            ElasticIndexOptions options)
        {
            _client = client;
            _name = options.Name;
            _alias = options.Alias;
        }

        public async Task EnsureAliasExistsAndPointsToIndex(CancellationToken ct)
        {
            var aliasName = Names.Parse(_alias);
            var aliasResponse = await _client.Indices.GetAliasAsync(new GetAliasRequest(aliasName), ct);
            if (!aliasResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to get aliases", aliasResponse.ElasticsearchServerError, aliasResponse.DebugInformation);
            }

            var indexName = Indices.Index(_name);
            if (aliasResponse.Aliases.ContainsKey(indexName))
            {
                return;
            }

            var oldIndexName = aliasResponse.Aliases
                .Where(x => x.Key != indexName)
                .Select(x => x.Key)
                .SingleOrDefault();
            if (oldIndexName is not null)
            {
                var reIndexResponse = await _client.ReindexAsync(new ReindexRequest
                {
                    Source = new Source { Indices = Indices.Index(oldIndexName) },
                    Dest = new Destination { Index = indexName }
                }, ct);
                if (!reIndexResponse.IsValidResponse)
                {
                    throw new ElasticsearchClientException($"Failed to get re-index '{oldIndexName}' to '{_name}'", reIndexResponse.ElasticsearchServerError, reIndexResponse.DebugInformation);
                }

                var updateResponse = await _client.Indices.UpdateAliasesAsync(new UpdateAliasesRequest
                {
                    Actions =
                    [
                        IndexUpdateAliasesAction.Remove(new RemoveAction
                        {
                            Alias = _alias,
                            Index = oldIndexName
                        }),
                        IndexUpdateAliasesAction.Add(new AddAction
                        {
                            Alias = _alias,
                            Index = indexName
                        })
                    ]
                }, ct);
                if (!updateResponse.IsValidResponse)
                {
                    throw new ElasticsearchClientException($"Failed to update alias '{_alias}' pointing to index '{_name}'", updateResponse.ElasticsearchServerError, updateResponse.DebugInformation);
                }
            }
            else
            {
                var putResponse = await _client.Indices.PutAliasAsync(new PutAliasRequest(_name, _alias), ct);
                if (!putResponse.IsValidResponse)
                {
                    throw new ElasticsearchClientException($"Failed to create alias '{_alias}' pointing to index '{_name}'", putResponse.ElasticsearchServerError, putResponse.DebugInformation);
                }
            }
        }

        public async Task CreateIndexIfNotExist(CancellationToken ct)
        {
            var indexName = Indices.Index(_name);
            var response = await _client.Indices.ExistsAsync(new ExistsRequest(indexName), ct);
            if (response.Exists)
                return;

            var createResponse = await _client.Indices.CreateAsync<AddressSearchDocument>(indexName, c =>
            {
                c.Settings(x => x.MaxResultWindow(1_000_001)); // Linked to public-api offset limit of 1_000_000

                c.Settings(x => x.Analysis(a =>
                    a
                        .CharFilters(cf => cf
                            .PatternReplace("dot_replace", prcf => prcf.Pattern("\\.").Replacement(""))
                            .PatternReplace("underscore_replace", prcf => prcf.Pattern("_").Replacement(" ")))
                        .TokenFilters(descriptor => AddDutchStopWordsFilter(descriptor))
                        .Normalizers(descriptor => AddAddressSearchNormalizer(descriptor))
                        .Analyzers(descriptor => AddAddressSearchAnalyzer(descriptor)))
                );

                c.Mappings(map => map
                    .Properties(p => p
                        .IntegerNumber(x => x.AddressPersistentLocalId)
                        .IntegerNumber(x => x.ParentAddressPersistentLocalId)
                        .Date(x => x.VersionTimestamp)
                        .Keyword(x => x.Status)
                        .Boolean(x => x.Active)
                        .Boolean(x => x.OfficiallyAssigned)
                        .Keyword(x => x.HouseNumber, c =>
                            c.Normalizer(AddressSearchNormalizer))
                        .Keyword(x => x.BoxNumber, c =>
                            c.Normalizer(AddressSearchNormalizer))
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
                                .Nested("homonymAdditions", ConfigureNames())
                            )
                        )
                        .Nested(x => x.FullAddress, ConfigureNames())
                    ));
            }, ct);

            if (!createResponse.Acknowledged || !createResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to create an index", createResponse.ElasticsearchServerError, createResponse.DebugInformation);
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
                            { "keyword", new KeywordProperty { IgnoreAbove = 256, Normalizer = AddressSearchNormalizer } }
                        },
                        Analyzer = AddressSearchAnalyzer
                    })
                    .Keyword("language")
                );
        }

        private static TokenFiltersDescriptor AddDutchStopWordsFilter(TokenFiltersDescriptor tokenFiltersDescriptor)
            => tokenFiltersDescriptor.Stop("dutch_stop", st => st.Stopwords(new List<string> { "_dutch_" }));

        private static NormalizersDescriptor AddAddressSearchNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(AddressSearchNormalizer, ca => ca
                .CharFilter(new[] { "underscore_replace", "dot_replace" })
                .Filter(new[] { "lowercase", "asciifolding", "trim" }));

        private static AnalyzersDescriptor AddAddressSearchAnalyzer(AnalyzersDescriptor analyzersDescriptor)
            => analyzersDescriptor.Custom(AddressSearchAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(new[] { "underscore_replace", "dot_replace" })
                .Filter(new[] { "lowercase", "asciifolding", "dutch_stop" })
            );
    }

    public sealed class ElasticIndexOptions
    {
        public string Name { get; init; }
        public string Alias { get; init; }

        public static ElasticIndexOptions LoadFromConfiguration(IConfiguration configuration)
        {
            var elasticOptions = configuration.GetSection("Elastic");
            return new ElasticIndexOptions
            {
                Name = elasticOptions["IndexName"]!,
                Alias = elasticOptions["IndexAlias"]!
            };
        }
    }
}
