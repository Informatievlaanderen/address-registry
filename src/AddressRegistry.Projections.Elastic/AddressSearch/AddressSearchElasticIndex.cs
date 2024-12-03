namespace AddressRegistry.Projections.Elastic.AddressSearch
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Analysis;
    using global::Elastic.Clients.Elasticsearch.IndexManagement;
    using global::Elastic.Clients.Elasticsearch.Mapping;
    using Microsoft.Extensions.Configuration;
    using ExistsRequest = global::Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

    public sealed class AddressSearchElasticIndex : ElasticIndexBase
    {
        public const string AddressSearchNormalizer = "AddressSearchNormalizer";
        public const string TextNumberNormalizer = "TextNumberNormalizer";
        public const string AddressSearchIndexAnalyzer = "AddressSearchIndexAnalyzer";
        public const string AddressFullSearchIndexAnalyzer = "AddressFullSearchIndexAnalyzer";
        public const string AddressFullSearchAnalyzer = "AddressFullSearchAnalyzer";

        public AddressSearchElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
            : this(client, ElasticIndexOptions.LoadFromConfiguration(configuration.GetSection("Elastic"), indexNameKey: "SearchIndexName", indexAliasKey: "SearchIndexAlias"))
        { }

        public AddressSearchElasticIndex(
            ElasticsearchClient client,
            ElasticIndexOptions options)
            :base(client, options)
        { }

        public override async Task CreateIndexIfNotExist(CancellationToken ct)
        {
            var indexName = Indices.Index(IndexName);
            var response = await Client.Indices.ExistsAsync(new ExistsRequest(indexName), ct);
            if (response.Exists)
                return;

            var createResponse = await Client.Indices.CreateAsync<AddressSearchDocument>(indexName, c =>
            {
                c.Settings(x => x
                    .MaxResultWindow(1_000_500) // Linked to public-api offset of 1_000_000 + limit of 500
                    .Sort(s => s
                        .Field(Fields.FromExpression((AddressSearchDocument d) => d.AddressPersistentLocalId))
                        .Order([SegmentSortOrder.Asc]))
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .PatternReplace("dot_replace", prcf => prcf.Pattern("\\.").Replacement(""))
                            .PatternReplace("underscore_replace", prcf => prcf.Pattern("_").Replacement(" "))
                            .PatternReplace("quote_replace", prcf => prcf.Pattern("\'").Replacement(""))
                            .PatternReplace("hyphen_replace", prcf => prcf.Pattern("-").Replacement(" "))
                        )
                        .TokenFilters(descriptor =>
                        {
                            descriptor.EdgeNGram("edge_ngram", e => e.MinGram(1).MaxGram(20));
                            AddDutchSynonymWordsFilter(descriptor);
                        })
                        .Normalizers(descriptor =>
                        {
                            AddAddressSearchNormalizer(descriptor);
                            AddTextNumberNormalizer(descriptor);
                        })
                        .Analyzers(descriptor =>
                        {
                            AddAddressSearchIndexAnalyzer(descriptor);
                            AddAddressFullSearchIndexAnalyzer(descriptor);
                            AddAddressFullSearchAnalyzer(descriptor);
                        })
                    )
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
                                c.Normalizer(TextNumberNormalizer))
                            .Keyword(x => x.BoxNumber, c =>
                                c.Normalizer(TextNumberNormalizer))
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
                            .Nested(x => x.FullAddress, ConfigureNames(AddressFullSearchIndexAnalyzer))
                    ));
            }, ct);

            if (!createResponse.Acknowledged || !createResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to create an index", createResponse.ElasticsearchServerError, createResponse.DebugInformation);
            }
        }

        private Action<NestedPropertyDescriptor<AddressSearchDocument>> ConfigureNames(string analyzer = AddressSearchIndexAnalyzer)
        {
            return n => n
                .Properties(np => np
                    .Text("spelling", new TextProperty
                    {
                        Fields = new Properties
                        {
                            { "keyword", new KeywordProperty { IgnoreAbove = 256, Normalizer = AddressSearchNormalizer } }
                        },
                        Analyzer = analyzer,
                        SearchAnalyzer = AddressFullSearchAnalyzer
                    })
                    .Keyword("language")
                );
        }

        private static void AddDutchSynonymWordsFilter(TokenFiltersDescriptor tokenFiltersDescriptor) =>
            tokenFiltersDescriptor
                .SynonymGraph("dutch_abbreviation_synonyms", sg => sg.Synonyms(ElasticHelpers.DutchAbbreviationSynonyms))
                .SynonymGraph("dutch_numeral_synonyms", sg => sg.Synonyms(ElasticHelpers.DutchNumeralSynonyms));

        private static void AddAddressSearchNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(AddressSearchNormalizer, ca => ca
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding", "trim"]));

        private static void AddTextNumberNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(TextNumberNormalizer, ca => ca
                .Filter(["lowercase", "asciifolding", "trim"]));

        private static void AddAddressSearchIndexAnalyzer(AnalyzersDescriptor analyzersDescriptor) =>
            analyzersDescriptor.Custom(AddressSearchIndexAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding"])
            );

        private static void AddAddressFullSearchIndexAnalyzer(AnalyzersDescriptor analyzersDescriptor) =>
            analyzersDescriptor.Custom(AddressFullSearchIndexAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding", "edge_ngram"])
            );

        private static void AddAddressFullSearchAnalyzer(AnalyzersDescriptor analyzersDescriptor) =>
            analyzersDescriptor.Custom(AddressFullSearchAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding", "dutch_abbreviation_synonyms", "dutch_numeral_synonyms"])
            );
    }
}
