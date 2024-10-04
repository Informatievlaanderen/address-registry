namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using Consumer.Read.StreetName.Projections.Elastic;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Name = AddressRegistry.Infrastructure.Elastic.Name;
    using PostalInfo = Projections.Elastic.AddressSearch.PostalInfo;

    public sealed partial class AddressApiStreetNameElasticsearchClient
    {
        public async Task<StreetNameSearchResult> SearchStreetNames(
            string query,
            string? municipalityOrPostalName,
            int size = 10)
        {
            var municipalityNames =
                $"{ToCamelCase(nameof(StreetNameSearchDocument.Municipality))}.{ToCamelCase(nameof(StreetNameSearchDocument.Municipality.Names))}";
            var postalNames =
                $"{ToCamelCase(nameof(StreetNameSearchDocument.PostalInfos))}.{ToCamelCase(nameof(PostalInfo.Names))}";

            var searchResponse = await ElasticsearchClient.SearchAsync<StreetNameSearchDocument>(IndexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(size)
                        .Query(q =>
                            q.Bool(x =>
                            {
                                x.Filter(f => f.Term(t => t.Field(ToCamelCase(nameof(StreetNameSearchDocument.Active))!).Value(true)));

                                var mustQueries = new List<Action<QueryDescriptor<StreetNameSearchDocument>>>();
                                var shouldMunicipalityOrPostalQueries = new List<Action<QueryDescriptor<StreetNameSearchDocument>>>();

                                mustQueries.Add(q2 =>
                                    q2.Bool(b =>
                                    {
                                        b.Must(must =>
                                            must.Nested(full =>
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
                                    }));

                                //query municipality names
                                if (!string.IsNullOrWhiteSpace(municipalityOrPostalName))
                                    shouldMunicipalityOrPostalQueries.Add(q2 =>
                                        q2.Nested(nested => nested
                                            .Path(municipalityNames!)
                                            .Query(nestedQuery => nestedQuery
                                                .Bool(b => b
                                                    .Should(
                                                        m => m
                                                            .ConstantScore(cs =>
                                                                cs.Filter(f => f.Prefix(pfx =>
                                                                        pfx.Field($"{municipalityNames}.{NameSpelling}.{Keyword}"!)
                                                                            .Value(municipalityOrPostalName)))
                                                                    .Boost(3)),
                                                        m => m
                                                            .ConstantScore(cs =>
                                                                cs.Filter(f => f.Match(m2 =>
                                                                    m2.Field($"{municipalityNames}.{NameSpelling}"!)
                                                                        .Query(municipalityOrPostalName))))
                                                    )
                                                )
                                            )
                                        )
                                    );

                                //query postal names
                                if (!string.IsNullOrWhiteSpace(municipalityOrPostalName))
                                {
                                    shouldMunicipalityOrPostalQueries.Add(q2 =>
                                        q2.Nested(nested => nested
                                            .Path(postalNames!)
                                            .Query(nestedQuery => nestedQuery
                                                .Bool(b => b
                                                    .Should(
                                                        m => m
                                                            .ConstantScore(cs =>
                                                                cs.Filter(f => f.Prefix(pfx =>
                                                                        pfx.Field($"{postalNames}.{NameSpelling}.{Keyword}"!)
                                                                            .Value(municipalityOrPostalName)))
                                                                    .Boost(3)),
                                                        m => m
                                                            .ConstantScore(cs =>
                                                                cs.Filter(f => f.Match(m2 =>
                                                                    m2.Field($"{postalNames}.{NameSpelling}"!).Query(municipalityOrPostalName))))
                                                    )
                                                )
                                            )
                                        ));
                                }

                                if (!string.IsNullOrWhiteSpace(municipalityOrPostalName))
                                {
                                    x.Must(
                                        must => must.Bool(b => b.Should(mustQueries.ToArray())),
                                        must => must.Bool(b => b.Should(shouldMunicipalityOrPostalQueries.ToArray())));
                                }
                                else
                                {
                                    x.Must(
                                        must => must.Bool(b => b.Should(mustQueries.ToArray())));
                                }
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
                return new StreetNameSearchResult(Enumerable.Empty<StreetNameSearchDocument>().ToList(), 0);
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
