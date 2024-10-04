namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Analysis;
    using global::Elastic.Clients.Elasticsearch.Mapping;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using ExistsRequest = global::Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

    public sealed class StreetNameElasticIndex : ElasticIndexBase
    {
        public const string StreetNameSearchNormalizer = "StreetNameSearchNormalizer";
        public const string StreetNameSearchIndexAnalyzer = "StreetNameSearchIndexAnalyzer";
        public const string StreetNameFullSearchIndexAnalyzer = "StreetNameFullSearchIndexAnalyzer";
        public const string StreetNameSearchAnalyzer = "StreetNameFullSearchAnalyzer";

        public StreetNameElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
            : this(client, ElasticIndexOptions.LoadFromConfiguration(configuration.GetSection(ElasticModule.ConfigurationSectionName)))
        { }

        public StreetNameElasticIndex(
            ElasticsearchClient client,
            ElasticIndexOptions options)
            : base(client, options)
        { }

        public async Task CreateIndexIfNotExist(CancellationToken ct)
        {
            var indexName = Indices.Index(IndexName);
            var response = await Client.Indices.ExistsAsync(new ExistsRequest(indexName), ct);
            if (response.Exists)
                return;

            var createResponse = await Client.Indices.CreateAsync<StreetNameSearchDocument>(indexName, c =>
            {
                c.Settings(x => x
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
                            AddStreetNameSearchNormalizer(descriptor);
                        })
                        .Analyzers(descriptor =>
                        {
                            AddStreetNameSearchIndexAnalyzer(descriptor);
                            AddStreetNameFullSearchIndexAnalyzer(descriptor);
                            AddStreetNameFullSearchAnalyzer(descriptor);
                        })
                    )
                );

                c.Mappings(map => map
                    .Properties(p => p
                            .IntegerNumber(x => x.StreetNamePersistentLocalId)
                            .Date(x => x.VersionTimestamp)
                            .Keyword(x => x.Status)
                            .Boolean(x => x.Active)
                            .Object(x => x.Municipality, objConfig => objConfig
                                .Properties(obj =>
                                {
                                    obj
                                        .Keyword(x => x.Municipality.NisCode)
                                        .Nested("names", ConfigureNames());
                                })
                            )
                            .Nested(x => x.PostalInfos, config =>
                            {
                                config.Properties(property => property
                                    .Keyword(nameof(PostalInfo.PostalCode))
                                    .Nested("names", ConfigureNames()));
                            })
                            .Nested("names", ConfigureNames())
                            .Nested("homonymAdditions", ConfigureNames())
                            .Nested(x => x.FullStreetNames, ConfigureNames(StreetNameFullSearchIndexAnalyzer))
                    ));
            }, ct);

            if (!createResponse.Acknowledged || !createResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to create an index", createResponse.ElasticsearchServerError, createResponse.DebugInformation);
            }
        }

        private Action<NestedPropertyDescriptor<StreetNameSearchDocument>> ConfigureNames(string analyzer = StreetNameSearchIndexAnalyzer)
        {
            return n => n
                .Properties(np => np
                    .Text("spelling", new TextProperty
                    {
                        Fields = new Properties
                        {
                            { "keyword", new KeywordProperty { IgnoreAbove = 256, Normalizer = StreetNameSearchNormalizer } }
                        },
                        Analyzer = analyzer,
                        SearchAnalyzer = StreetNameSearchAnalyzer
                    })
                    .Keyword("language")
                );
        }

        private static TokenFiltersDescriptor AddDutchSynonymWordsFilter(TokenFiltersDescriptor tokenFiltersDescriptor)
        {
            return
                tokenFiltersDescriptor
                    .SynonymGraph("dutch_abbreviation_synonyms", sg => sg.Synonyms(ElasticHelpers.DutchAbbreviationSynonyms))
                    .SynonymGraph("dutch_numeral_synonyms", sg => sg.Synonyms(ElasticHelpers.DutchNumeralSynonyms));
        }

        private static NormalizersDescriptor AddStreetNameSearchNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(StreetNameSearchNormalizer, ca => ca
                .CharFilter(new[] { "underscore_replace", "dot_replace", "quote_replace", "hyphen_replace" })
                .Filter(new[] { "lowercase", "asciifolding", "trim" }));

        private static AnalyzersDescriptor AddStreetNameSearchIndexAnalyzer(AnalyzersDescriptor analyzersDescriptor)
            => analyzersDescriptor.Custom(StreetNameSearchIndexAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(new [] { "underscore_replace", "dot_replace", "quote_replace", "hyphen_replace" })
                .Filter(new [] { "lowercase", "asciifolding" })
            );

        private static AnalyzersDescriptor AddStreetNameFullSearchIndexAnalyzer(AnalyzersDescriptor analyzersDescriptor)
            => analyzersDescriptor.Custom(StreetNameFullSearchIndexAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(new [] { "underscore_replace", "dot_replace", "quote_replace", "hyphen_replace" })
                .Filter(new [] { "lowercase", "asciifolding", "edge_ngram" })
            );

        private static AnalyzersDescriptor AddStreetNameFullSearchAnalyzer(AnalyzersDescriptor analyzersDescriptor)
            => analyzersDescriptor.Custom(StreetNameSearchAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(new [] { "underscore_replace", "dot_replace", "quote_replace", "hyphen_replace" })
                .Filter(new [] { "lowercase", "asciifolding", "dutch_abbreviation_synonyms", "dutch_numeral_synonyms" })
            );
    }
}
