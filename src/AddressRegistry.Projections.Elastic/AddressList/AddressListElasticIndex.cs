namespace AddressRegistry.Projections.Elastic.AddressList
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

    public sealed class AddressListElasticIndex : ElasticIndexBase
    {
        public const string AddressListNormalizer = "AddressListNormalizer";
        public const string TextNumberNormalizer = "TextNumberNormalizer";
        public const string AddressListIndexAnalyzer = "AddressListIndexAnalyzer";

        public AddressListElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
            : this(client, ElasticIndexOptions.LoadFromConfiguration(configuration.GetSection("Elastic"), indexNameKey: "ListIndexName", indexAliasKey: "ListIndexAlias"))
        { }

        public AddressListElasticIndex(
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

            var createResponse = await Client.Indices.CreateAsync<AddressListDocument>(indexName, c =>
            {
                c.Settings(x => x
                    .MaxResultWindow(1_000_500) // Linked to public-api offset of 1_000_000 + limit of 500
                    .Sort(s => s
                        .Field(Fields.FromExpression((AddressListDocument d) => d.AddressPersistentLocalId))
                        .Order([SegmentSortOrder.Asc]))
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .PatternReplace("dot_replace", prcf => prcf.Pattern("\\.").Replacement(""))
                            .PatternReplace("underscore_replace", prcf => prcf.Pattern("_").Replacement(" "))
                            .PatternReplace("quote_replace", prcf => prcf.Pattern("\'").Replacement(""))
                            .PatternReplace("hyphen_replace", prcf => prcf.Pattern("-").Replacement(" "))
                        )
                        .Normalizers(descriptor =>
                        {
                            AddAddressListNormalizer(descriptor);
                            AddTextNumberNormalizer(descriptor);
                        })
                        .Analyzers(descriptor =>
                        {
                            AddAddressListIndexAnalyzer(descriptor);
                        })
                    )
                );

                c.Mappings(map => map
                    .Properties(p => p
                            .IntegerNumber(x => x.AddressPersistentLocalId)
                            .IntegerNumber(x => x.ParentAddressPersistentLocalId)
                            .Date(x => x.VersionTimestamp)
                            .Keyword(x => x.Status)
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
                    ));
            }, ct);

            if (!createResponse.Acknowledged || !createResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to create an index", createResponse.ElasticsearchServerError, createResponse.DebugInformation);
            }
        }

        private Action<NestedPropertyDescriptor<AddressListDocument>> ConfigureNames(string analyzer = AddressListIndexAnalyzer)
        {
            return n => n
                .Properties(np => np
                    .Text("spelling", new TextProperty
                    {
                        Fields = new Properties
                        {
                            { "keyword", new KeywordProperty { IgnoreAbove = 256, Normalizer = AddressListNormalizer } }
                        },
                        Analyzer = analyzer,
                        SearchAnalyzer = analyzer
                    })
                    .Keyword("language")
                );
        }

        private static void AddAddressListNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(AddressListNormalizer, ca => ca
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding", "trim"]));

        private static void AddTextNumberNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(TextNumberNormalizer, ca => ca
                .Filter(["lowercase", "asciifolding", "trim"]));

        private static void AddAddressListIndexAnalyzer(AnalyzersDescriptor analyzersDescriptor) =>
            analyzersDescriptor.Custom(AddressListIndexAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding"])
            );
    }
}
