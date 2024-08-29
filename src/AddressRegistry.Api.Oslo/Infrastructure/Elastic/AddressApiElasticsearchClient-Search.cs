namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using AddressRegistry.Projections.Elastic.AddressSearch;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Aggregations;
    using global::Elastic.Clients.Elasticsearch.Core.Search;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;

    public sealed class StreetNameSearchResult
    {
        public int StreetNamePersistentLocalId { get; set; }
        public string Spelling { get; set; }
    }

    public sealed class SpellingObject
    {
        [JsonPropertyName("spelling")]
        public string Spelling { get; set; }
    }

    public sealed partial class AddressApiElasticsearchClient : IAddressApiElasticsearchClient
    {
        private const string Keyword = "keyword";
        private static readonly string NameSpelling = $"{ToCamelCase(nameof(Projections.Elastic.AddressSearch.Name.Spelling))}";

        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly string _indexAlias;
        private readonly ILogger<AddressApiElasticsearchClient> _logger;

        public AddressApiElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias,
            ILoggerFactory loggerFactory)
        {
            _elasticsearchClient = elasticsearchClient;
            _indexAlias = indexAlias;
            _logger = loggerFactory.CreateLogger<AddressApiElasticsearchClient>();
        }

        public async Task<IEnumerable<StreetNameSearchResult>> SearchStreetNames(
            string[] streetNameQueries,
            string municipalityOrPostalName,
            int size = 10)
        {
            var searchResponse = await _elasticsearchClient.SearchAsync<AddressSearchDocument>(_indexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(0)
                        .Query(q =>
                            q.Bool(x => x.Should(q2 =>
                                q2.Nested(nested => QueryStreetNames(nested, streetNameQueries)),
                                q2 => q2.Nested(nested => nested
                                    .Path("municipality.names")
                                    .Query(nestedQuery => nestedQuery
                                        .Bool(b => b
                                            .Should(
                                                m => m
                                                    .ConstantScore(cs =>
                                                        cs.Filter(f => f.Prefix(pfx => pfx.Field("municipality.names.spelling.keyword").Value(municipalityOrPostalName)))
                                                            .Boost(3)),
                                                m => m
                                                    .ConstantScore(cs =>
                                                        cs.Filter(f => f.Match(m2 => m2.Field("municipality.names.spelling").Query(municipalityOrPostalName))))
                                            )
                                        )
                                    )
                                ),
                                q2 => q2.Nested(nested => nested
                                    .Path("postalInfo.names")
                                    .Query(nestedQuery => nestedQuery
                                        .Bool(b => b
                                            .Should(
                                                m => m
                                                    .ConstantScore(cs =>
                                                        cs.Filter(f => f.Prefix(pfx => pfx.Field("postalInfo.names.spelling.keyword").Value(municipalityOrPostalName)))
                                                            .Boost(3)),
                                                m => m
                                                    .ConstantScore(cs =>
                                                        cs.Filter(f => f.Match(m2 => m2.Field("postalInfo.names.spelling").Query(municipalityOrPostalName))))
                                            )
                                        )
                                    )
                                )
                            ))
                        )
                        .Aggregations(a => a.Add("unique_street_names", aggs =>
                        {
                            aggs.Nested(nested => nested.Path("streetName.names"));
                            aggs.Aggregations(innerAggs => innerAggs
                                .Add("filtered_names", aggs2 =>
                                {
                                    aggs2.Filter(filter => filter
                                        .Bool(b => b
                                            .Should(streetNameQueries.SelectMany(name => new List<Action<QueryDescriptor<AddressSearchDocument>>>
                                            {
                                                m => m
                                                    .Prefix(pfx => pfx.Field("streetName.names.spelling.keyword").Value(name)),
                                                m => m
                                                    .Match(m2 => m2.Field("streetName.names.spelling").Query(name))
                                            }).ToArray())
                                        )
                                    );
                                    aggs2.Aggregations(innerAggs2 => innerAggs2
                                        .Add("street_names", AddStreetNamesAggregation(size))
                                    );
                                })
                            );
                        }));
                });

            if (!searchResponse.IsValidResponse)
                return new List<StreetNameSearchResult>();

            return GetStreetNameSearchResults(searchResponse);
        }

        public async Task<IEnumerable<StreetNameSearchResult>> SearchStreetNames(string query, int size = 10)
        {
            var searchResponse = await _elasticsearchClient.SearchAsync<AddressSearchDocument>(_indexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(0)
                        .Query(q =>
                            q.Nested(nested => QueryStreetNames(nested, [query]))
                        )
                        .Aggregations(a => a.Add("unique_street_names", aggs =>
                        {
                            aggs.Nested(nested => nested.Path("streetName.names"));
                            aggs.Aggregations(innerAggs => innerAggs
                                .Add("filtered_names", aggs2 =>
                                {
                                    aggs2.Filter(filter => filter
                                        .Bool(b => b
                                            .Should(
                                                m => m
                                                    .Prefix(pfx => pfx.Field("streetName.names.spelling.keyword").Value(query)),
                                                m => m
                                                    .Match(m2 => m2.Field("streetName.names.spelling").Query(query))
                                            )
                                        )
                                    );
                                    aggs2.Aggregations(innerAggs2 => innerAggs2
                                        .Add("street_names", AddStreetNamesAggregation(size))
                                    );
                                })
                            );
                        }));
                });

            if (!searchResponse.IsValidResponse)
                return new List<StreetNameSearchResult>();

            return GetStreetNameSearchResults(searchResponse);
        }

        private static void QueryStreetNames(NestedQueryDescriptor<AddressSearchDocument> query, string[] names)
        {
            query.Path("streetName.names")
                .Query(nestedQuery => nestedQuery
                    .FunctionScore(fs => fs
                        .Query(fsq => fsq
                            .Bool(fsqb =>
                                fsqb.Should(names.SelectMany(name => new List<Action<QueryDescriptor<AddressSearchDocument>>>
                                {
                                    should => should
                                        .ConstantScore(cs => cs.Boost(10.0F).Filter(csf => csf.Prefix(pfx => pfx.Field("streetName.names.spelling.keyword").Value(name)))),
                                    should => should
                                        .ConstantScore(cs => cs.Boost(3).Filter(csf => csf.Match(m => m.Field("streetName.names.spelling").Query(name))))
                                }).ToArray())
                            )
                        )
                    )
                );
        }

        private static Action<AggregationDescriptor<AddressSearchDocument>> AddStreetNamesAggregation(int size)
        {
            return aggs3 =>
            {
                aggs3.Terms(t => t
                    .Field("streetName.names.spelling.keyword")
                    .Size(size)
                    .Order(new List<KeyValuePair<Field, SortOrder>>
                    {
                        new (new Field("top_hit_score"), SortOrder.Desc)
                    }));
                aggs3.Aggregations(innerAggs3 => innerAggs3
                    .Add("top_street_name", aggs4 =>
                    {
                        aggs4.TopHits(t => t
                            .Source(new SourceConfig(new SourceFilter()
                                { Includes = Fields.FromField(new Field("streetName.names.spelling")) }))
                            .Size(1)
                            .Sort(new List<SortOptions>{ SortOptions.Score(new ScoreSort(){ Order = SortOrder.Desc }) }));

                    })
                    .Add("top_hit_score", aggs4 =>
                    {
                        aggs4.Max(m => m.Script(new Script(){ Source = "_score" }));
                    })
                    .Add("streetNamePersistentLocalId", aggs5 =>
                    {
                        aggs5.ReverseNested(new ReverseNestedAggregation());
                        aggs5.Aggregations(aggs6 => aggs6
                            .Add("id_value", x => x.Max(max => max.Field("streetName.streetNamePersistentLocalId"))));
                    })
                );
            };
        }

        private static IEnumerable<StreetNameSearchResult> GetStreetNameSearchResults(SearchResponse<AddressSearchDocument> searchResponse)
        {
            var names = new List<StreetNameSearchResult>();
            var uniqueStreetNamesAgg = searchResponse.Aggregations.GetNested("unique_street_names");

            // Access the inner 'filtered_names' nested aggregation
            var filteredNames = uniqueStreetNamesAgg?.Aggregations.GetFilter("filtered_names");

            if (filteredNames != null)
            {
                // Access the 'street_names' terms aggregation
                var streetNamesTermsAgg = filteredNames.Aggregations.GetStringTerms("street_names");

                foreach (var bucket in streetNamesTermsAgg.Buckets)
                {
                    var streetNameId = bucket.Aggregations.GetReverseNested("streetNamePersistentLocalId");

                    var id = streetNameId?.Aggregations.GetMax("id_value")?.Value;

                    // Each bucket may have a 'top_street_name' hits aggregation
                    var topStreetNameHits = bucket.Aggregations.GetTopHits("top_street_name");

                    if (topStreetNameHits is not null)
                    {
                        var hit = topStreetNameHits.Hits.Hits.First();

                        var name = JsonSerializer.Deserialize<SpellingObject>(hit.Source.ToString());
                        names.Add(new StreetNameSearchResult
                        {
                            StreetNamePersistentLocalId = Convert.ToInt32(id),
                            Spelling = name.Spelling
                        });
                    }
                }
            }

            return names;
        }

        private static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
                return str;

            char[] chars = str.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 0 || i > 0 && char.IsUpper(chars[i]))
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }
                else
                {
                    break;
                }
            }

            return new string(chars);
        }
    }
}
