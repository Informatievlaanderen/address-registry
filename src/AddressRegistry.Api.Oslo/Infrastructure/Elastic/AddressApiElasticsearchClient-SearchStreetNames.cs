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

    public sealed partial class AddressApiElasticsearchClient
    {
        private static class AggregationNames
        {
            public const string UniqueStreetNames = "unique_street_names";
            public const string FilteredNames = "filtered_names";
            public const string StreetNames = "street_names";
            public const string FilteredByActive = "filtered_by_active";
            public const string IdValue = "id_value";
            public const string StreetNamePersistentLocalId = "streetNamePersistentLocalId";
            public const string TopHitScore = "top_hit_score";
            public const string TopStreetName = "top_street_name";
        }

        private static readonly string StreetNameNames = $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.Names))}";

        public async Task<IEnumerable<StreetNameSearchResult>> SearchStreetNames(
            string[] streetNameQueries,
            string municipalityOrPostalName,
            int size = 10)
        {
            var searchResponse = await _elasticsearchClient.SearchAsync<AddressSearchDocument>(_indexAlias,
                descriptor =>
                {
                    var municipalityNames =
                        $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.Names))}";
                    var postalNames =
                        $"{ToCamelCase(nameof(AddressSearchDocument.PostalInfo))}.{ToCamelCase(nameof(AddressSearchDocument.PostalInfo.Names))}";
                    descriptor
                        .Size(0)
                        .Query(q =>
                            q.Bool(x =>
                            {
                                x.Filter(f => f.Term(t => t.Field(ToCamelCase(nameof(AddressSearchDocument.Active))!).Value(true)));
                                x.Should(q2 =>
                                        q2.Nested(nested => QueryStreetNames(nested, streetNameQueries)),
                                    q2 => q2.Nested(nested => nested
                                        .Path(municipalityNames!)
                                        .Query(nestedQuery => nestedQuery
                                            .Bool(b => b
                                                .Should(
                                                    m => m
                                                        .ConstantScore(cs =>
                                                            cs.Filter(f => f.Prefix(pfx =>
                                                                    pfx.Field($"{municipalityNames}.{NameSpelling}.{Keyword}"!).Value(municipalityOrPostalName)))
                                                                .Boost(3)),
                                                    m => m
                                                        .ConstantScore(cs =>
                                                            cs.Filter(f => f.Match(m2 =>
                                                                m2.Field($"{municipalityNames}.{NameSpelling}"!).Query(municipalityOrPostalName))))
                                                )
                                            )
                                        )
                                    ),
                                    q2 => q2.Nested(nested => nested
                                        .Path(postalNames!)
                                        .Query(nestedQuery => nestedQuery
                                            .Bool(b => b
                                                .Should(
                                                    m => m
                                                        .ConstantScore(cs =>
                                                            cs.Filter(f => f.Prefix(pfx =>
                                                                    pfx.Field($"{postalNames}.{NameSpelling}.{Keyword}"!).Value(municipalityOrPostalName)))
                                                                .Boost(3)),
                                                    m => m
                                                        .ConstantScore(cs =>
                                                            cs.Filter(f => f.Match(m2 =>
                                                                m2.Field($"{postalNames}.{NameSpelling}"!).Query(municipalityOrPostalName))))
                                                )
                                            )
                                        )
                                    )
                                );
                            })
                        )
                        .Aggregations(a => a.Add(AggregationNames.UniqueStreetNames, aggs =>
                        {
                            aggs.Nested(nested => nested.Path(StreetNameNames!));
                            aggs.Aggregations(innerAggs => innerAggs
                                .Add(AggregationNames.FilteredNames, aggs2 =>
                                {
                                    aggs2.Filter(filter => filter
                                        .Bool(b => b
                                            .Should(streetNameQueries.SelectMany(name => new List<Action<QueryDescriptor<AddressSearchDocument>>>
                                            {
                                                m => m
                                                    .Prefix(pfx => pfx.Field($"{StreetNameNames}.{NameSpelling}.{Keyword}"!).Value(name)),
                                                m => m
                                                    .Match(m2 => m2.Field($"{StreetNameNames}.{NameSpelling}"!).Query(name))
                                            }).ToArray())
                                        )
                                    );
                                    aggs2.Aggregations(innerAggs2 => innerAggs2
                                        .Add(AggregationNames.StreetNames, AddStreetNamesAggregation(size))
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
                            q.Bool(q2 =>
                            {
                                q2.Filter(f => f.Term(t => t.Field(ToCamelCase(nameof(AddressSearchDocument.Active))!).Value(true)));
                                q2.Should(m => m.Nested(nested => QueryStreetNames(nested, [query])));
                            })
                        )
                        .Aggregations(a => a.Add(AggregationNames.UniqueStreetNames, aggs =>
                        {
                            aggs.Nested(nested => nested.Path(StreetNameNames));
                            aggs.Aggregations(innerAggs => innerAggs
                                .Add(AggregationNames.FilteredNames, aggs2 =>
                                {
                                    aggs2.Filter(filter => filter
                                        .Bool(b => b
                                            .Should(
                                                m => m
                                                    .Prefix(pfx => pfx.Field($"{StreetNameNames}.{NameSpelling}.{Keyword}"!).Value(query)),
                                                m => m
                                                    .Match(m2 => m2.Field($"{StreetNameNames}.{NameSpelling}"!).Query(query))
                                            )
                                        )
                                    );
                                    aggs2.Aggregations(innerAggs2 => innerAggs2
                                        .Add(AggregationNames.StreetNames, AddStreetNamesAggregation(size))
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
            query.Path(StreetNameNames!)
                .Query(nestedQuery => nestedQuery
                    .FunctionScore(fs => fs
                        .Query(fsq => fsq
                            .Bool(fsqb =>
                                fsqb.Should(names.SelectMany(name => new List<Action<QueryDescriptor<AddressSearchDocument>>>
                                {
                                    should => should
                                        .ConstantScore(cs => cs.Boost(10.0F).Filter(csf => csf.Prefix(pfx => pfx.Field($"{StreetNameNames}.{NameSpelling}.{Keyword}"!).Value(name)))),
                                    should => should
                                        .ConstantScore(cs => cs.Boost(3).Filter(csf => csf.Match(m => m.Field($"{StreetNameNames}.{NameSpelling}"!).Query(name))))
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
                    .Field($"{StreetNameNames}.{NameSpelling}.{Keyword}"!)
                    .Size(size)
                    .Order(new List<KeyValuePair<Field, SortOrder>>
                    {
                        new (new Field(AggregationNames.TopHitScore), SortOrder.Desc)
                    }));
                aggs3.Aggregations(innerAggs3 => innerAggs3
                    .Add(AggregationNames.TopStreetName, aggs4 =>
                    {
                        aggs4.TopHits(t => t
                            .Source(new SourceConfig(new SourceFilter()
                                { Includes = Fields.FromField(new Field($"{StreetNameNames}.{NameSpelling}")) }))
                            .Size(1)
                            .Sort(new List<SortOptions>{ SortOptions.Score(new ScoreSort(){ Order = SortOrder.Desc }) }));

                    })
                    .Add(AggregationNames.TopHitScore, aggs4 =>
                    {
                        aggs4.Max(m => m.Script(new Script(){ Source = "_score" }));
                    })
                    .Add(AggregationNames.StreetNamePersistentLocalId, aggs5 =>
                    {
                        aggs5.ReverseNested(new ReverseNestedAggregation());
                        aggs5.Aggregations(aggs6 => aggs6
                            .Add(AggregationNames.FilteredByActive, innerAggs6 =>
                            {
                                innerAggs6.Filter(filter => filter
                                    .Term(t => t.Field(ToCamelCase(nameof(AddressSearchDocument.Active))!).Value(true))
                                );
                                innerAggs6.Aggregations(aggs7 =>
                                    aggs7.Add(AggregationNames.IdValue, x => x.Max(max => max.Field($"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.StreetNamePersistentLocalId))}"))));
                            }));
                    })
                );
            };
        }

        private static IEnumerable<StreetNameSearchResult> GetStreetNameSearchResults(SearchResponse<AddressSearchDocument> searchResponse)
        {
            var names = new List<StreetNameSearchResult>();
            var uniqueStreetNamesAgg = searchResponse.Aggregations?.GetNested(AggregationNames.UniqueStreetNames);

            // Access the inner 'filtered_names' nested aggregation
            var filteredNames = uniqueStreetNamesAgg?.Aggregations.GetFilter(AggregationNames.FilteredNames);

            if (filteredNames != null)
            {
                // Access the 'street_names' terms aggregation
                var streetNamesTermsAgg = filteredNames.Aggregations.GetStringTerms(AggregationNames.StreetNames);

                foreach (var bucket in streetNamesTermsAgg.Buckets)
                {
                    var streetNameId = bucket.Aggregations.GetReverseNested(AggregationNames.StreetNamePersistentLocalId);

                    var id = streetNameId?.Aggregations.GetFilter(AggregationNames.FilteredByActive)?.Aggregations.GetMax(AggregationNames.IdValue)?.Value;

                    // Each bucket may have a 'top_street_name' hits aggregation
                    var topStreetNameHits = bucket.Aggregations.GetTopHits(AggregationNames.TopStreetName);

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

            var chars = str.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
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
