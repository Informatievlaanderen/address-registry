﻿namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using Consumer.Read.StreetName.Projections;
    using Consumer.Read.StreetName.Projections.Elastic;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Name = AddressRegistry.Infrastructure.Elastic.Name;

    public sealed partial class AddressApiStreetNameElasticsearchClient
    {
        public async Task<StreetNameSearchResult> SearchStreetNames(
            string query,
            string? nisCode,
            StreetNameStatus? status,
            int size = 10)
        {
            var searchResponse = await ElasticsearchClient.SearchAsync<StreetNameSearchDocument>(IndexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(size)
                        .Query(q =>
                            q.Bool(x =>
                            {
                                var conditions = new List<Action<QueryDescriptor<StreetNameSearchDocument>>>();

                                if (status is not null)
                                {
                                    conditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(StreetNameSearchDocument.Status))}"!)
                                        .Value(status.ToString()!)));
                                }
                                else
                                {
                                    conditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(StreetNameSearchDocument.Active))}"!)
                                        .Value(true)));
                                }

                                if (!string.IsNullOrWhiteSpace(nisCode))
                                {
                                    conditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(StreetNameSearchDocument.Municipality))}.{ToCamelCase(nameof(StreetNameSearchDocument.Municipality.NisCode))}"!)
                                        .Value(nisCode)));
                                }

                                conditions.Add(q2 =>
                                    q2.Nested(full =>
                                        full
                                            .Path(_fullStreetNames)
                                            .Query(fullAddressQuery =>
                                            {
                                                fullAddressQuery.Bool(b => b.Should(s =>
                                                {
                                                    if (!query.Contains(' '))
                                                    {
                                                        s.Prefix(p =>
                                                            p
                                                                .Field($"{_fullStreetNames}.{NameSpelling}")
                                                                .Value(query)
                                                                .Boost(3));
                                                    }

                                                    s.MatchPhrase(mp =>
                                                        mp
                                                            .Field($"{_fullStreetNames}.{NameSpelling}")
                                                            .Query(query)
                                                            .Slop(10));
                                                }));
                                            })
                                            .InnerHits(c =>
                                                c.Size(1))
                                    ));

                                x.Must(conditions.ToArray());
                            })
                        )
                        .Sort(new Action<SortOptionsDescriptor<StreetNameSearchDocument>>[]
                        {
                            s => s.Score(new ScoreSort { Order = SortOrder.Desc }),
                            s => s.Field($"{_fullStreetNames}.{NameSpelling}.{Keyword}",
                                c =>
                                    c.Nested(n =>
                                        n.Path(_fullStreetNames)
                                    ).Order(SortOrder.Asc))
                        });
                });

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogWarning("Failed to search for streetnames: {Error}", searchResponse.ElasticsearchServerError);
                return StreetNameSearchResult.Empty;
            }

            var language = DetermineLanguage(searchResponse);
            return new StreetNameSearchResult(searchResponse.Documents, searchResponse.Total, language);
        }

        private static Language? DetermineLanguage(SearchResponse<StreetNameSearchDocument> response)
        {
            if (!response.Hits.Any())
            {
                return null;
            }

            var innerHits = response.Hits.First().InnerHits;
            if (innerHits is null || !innerHits.TryGetValue("fullStreetNames", out var fullStreetNamesHit))
            {
                return null;
            }

            if (!fullStreetNamesHit.Hits.Hits.Any())
            {
                return null;
            }

            if (fullStreetNamesHit.Hits.Hits.First().Source is not JsonElement source)
            {
                return null;
            }

            var language = source.GetProperty(ToCamelCase(nameof(Name.Language))).GetString();
            return Enum.Parse<Language>(language!);
        }
    }
}
