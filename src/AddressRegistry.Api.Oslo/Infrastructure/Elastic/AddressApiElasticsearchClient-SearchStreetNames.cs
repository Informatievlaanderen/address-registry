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
    using Name = Projections.Elastic.AddressSearch.Name;

    public sealed class StreetNameSearchResult
    {
        public int StreetNamePersistentLocalId { get; set; }
        public Name StreetName { get; set; }

        public List<Name> MunicipalityNames { get; set; }

        public double Score { get; set; }

        public string GetFormattedStreetName()
        {
            var municipalityName = StreetName.Language switch
            {
                Language.nl => MunicipalityNames.FirstOrDefault(x => x.Language == Language.nl),
                Language.fr => MunicipalityNames.FirstOrDefault(x => x.Language == Language.fr),
                Language.de => MunicipalityNames.FirstOrDefault(x => x.Language == Language.de),
                Language.en => MunicipalityNames.FirstOrDefault(x => x.Language == Language.en),
                _ => null
            } ?? MunicipalityNames.First();

            return $"{StreetName.Spelling}, {municipalityName.Spelling}";
        }
    }

    public sealed class MunicipalityHit
    {
        public Municipality Municipality { get; set; }
        public StreetName StreetName { get; set; }
    }

    public sealed class StreetName
    {
        public int StreetNamePersistentLocalId { get; set; }
    }

    public sealed class Municipality
    {
        [JsonPropertyName("names")]
        public List<Name> Names { get; set; }
    }

    public sealed partial class AddressApiElasticsearchClient
    {
        private static class AggregationNames
        {
            public const string UniqueStreetNames = "unique_street_names";
            public const string FilteredNames = "filtered_names";
            public const string StreetNames = "street_names";
            public const string TopHitScore = "top_hit_score";
            public const string TopStreetName = "top_street_name";
            public const string MunicipalityNames = "municipality_names";
            public const string UniqueMunicipalities = "unique_municipalities";
            public const string InnerMunicipalityNames = "inner_municipality_names";
        }

        private static readonly string StreetNameNames = $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.Names))}";

        public async Task<IEnumerable<StreetNameSearchResult>> SearchStreetNames(
            string[] streetNameQueries,
            string municipalityOrPostalName,
            bool mustBeInMunicipality,
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
                                if (mustBeInMunicipality)
                                {
                                    x.Must(q2 => q2.Nested(nested => QueryStreetNames(nested, streetNameQueries)),
                                        q2 => q2
                                            .Bool(should => should
                                                .Should(q3 =>
                                                    q3.Nested(nested => nested
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
                                                    q3 => q3.Nested(nested => nested
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
                                                )
                                            )
                                    );
                                }
                                else
                                {
                                    x.Should(q2 => q2.Nested(nested => QueryStreetNames(nested, streetNameQueries)),
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
                                }
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
            {
                _logger.LogWarning("Failed to search for addresses: {Error}", searchResponse.ElasticsearchServerError);
                return new List<StreetNameSearchResult>();
            }

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
            {
                _logger.LogWarning("Failed to search for addresses: {Error}", searchResponse.ElasticsearchServerError);
                return new List<StreetNameSearchResult>();
            }

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
                            {
                                Includes = Fields.FromFields([
                                    new Field($"{StreetNameNames}.{NameSpelling}"),
                                    new Field($"{StreetNameNames}.{NameSpelling}.{ToCamelCase(nameof(Name.Language))}")
                                    ])
                            }))
                            .Size(1)
                            .Sort(new List<SortOptions>{ SortOptions.Score(new ScoreSort(){ Order = SortOrder.Desc }) }));

                    })
                    .Add(AggregationNames.TopHitScore, aggs4 =>
                    {
                        aggs4.Max(m => m.Script(new Script{ Source = "_score" }));
                    })
                    .Add(AggregationNames.MunicipalityNames, aggs7 =>
                    {
                        aggs7.ReverseNested(new ReverseNestedAggregation());
                        aggs7.Aggregations(innerAggs7 => innerAggs7
                            .Add(AggregationNames.UniqueMunicipalities, aggs8 =>
                                aggs8.Terms(x => x
                                        .Field(
                                            $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.StreetNamePersistentLocalId))}")
                                        .Size(size))
                                    .Aggregations(innerAggs8 => innerAggs8
                                        .Add(AggregationNames.InnerMunicipalityNames, aggs9 =>
                                        {
                                            aggs9.TopHits(new TopHitsAggregation
                                            {
                                                Size = 1,
                                                Source = new SourceConfig(new SourceFilter
                                                {
                                                    Includes = Fields.FromFields([
                                                        new Field(
                                                            $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.Names))}"),
                                                        new Field(
                                                            $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.StreetNamePersistentLocalId))}")
                                                    ])
                                                })
                                            });
                                        })
                                    )
                            )
                        );
                    })
                );
            };
        }

        private IEnumerable<StreetNameSearchResult> GetStreetNameSearchResults(SearchResponse<AddressSearchDocument> searchResponse)
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
                    var score = bucket.Aggregations.GetMax(AggregationNames.TopHitScore)?.Value ?? 0;

                    var municipalityNamesAggs = bucket.Aggregations.GetReverseNested(AggregationNames.MunicipalityNames);
                    var uniqueMunicipalities = municipalityNamesAggs?.Aggregations.GetLongTerms(AggregationNames.UniqueMunicipalities);
                    var municipalityNames = new Dictionary<int, List<Name>>();

                    foreach (var municipalitiesBucket in uniqueMunicipalities?.Buckets)
                    {
                        var innerMunicipalityNames = municipalitiesBucket.Aggregations.GetTopHits(AggregationNames.InnerMunicipalityNames);
                        if (innerMunicipalityNames is not null)
                        {
                            var hit = innerMunicipalityNames.Hits.Hits.First();
                            var municipalityHitObject = JsonSerializer.Deserialize<MunicipalityHit>(hit.Source!.ToString()!, _jsonSerializerOptions);
                            var streetNameIdByMunicipalityId = municipalityHitObject.StreetName.StreetNamePersistentLocalId;
                            municipalityNames.Add(streetNameIdByMunicipalityId, municipalityHitObject.Municipality.Names);
                        }
                    }

                    // Each bucket may have a 'top_street_name' hits aggregation
                    var topStreetNameHits = bucket.Aggregations.GetTopHits(AggregationNames.TopStreetName);
                    if (topStreetNameHits is not null)
                    {
                        var hit = topStreetNameHits.Hits.Hits.First();

                        var name = JsonSerializer.Deserialize<Name>(hit.Source!.ToString()!, _jsonSerializerOptions);
                        foreach (var municipalityName in municipalityNames)
                        {
                            names.Add(new StreetNameSearchResult
                            {
                                StreetNamePersistentLocalId = municipalityName.Key,
                                StreetName = name,
                                MunicipalityNames = municipalityName.Value,
                                Score = score
                            });
                        }
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
