namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Core.Search;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Projections.Elastic.AddressSearch;
    using Name = Projections.Elastic.AddressSearch.Name;

    public sealed partial class AddressApiElasticsearchClient
    {
        public async Task<AddressSearchResult> SearchAddresses(
            string addressQuery,
            string? municipalityOrPostalName,
            int? size = 10)
        {
            var municipalityNames =
                $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.Names))}";
            var postalNames =
                $"{ToCamelCase(nameof(AddressSearchDocument.PostalInfo))}.{ToCamelCase(nameof(AddressSearchDocument.PostalInfo.Names))}";

            var searchResponse = await _elasticsearchClient.SearchAsync<AddressSearchDocument>(_indexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(size)
                        .Query(q =>
                            q.Bool(x =>
                            {
                                x.Filter(f => f.Term(t => t.Field(ToCamelCase(nameof(AddressSearchDocument.Active))!).Value(true)));
                                if (!string.IsNullOrWhiteSpace(municipalityOrPostalName))
                                {
                                    var shouldQueries = new List<Action<QueryDescriptor<AddressSearchDocument>>>();
                                    var shouldMunicipalityOrPostalQueries = new List<Action<QueryDescriptor<AddressSearchDocument>>>();

                                    shouldQueries.Add(q2 =>
                                        q2.Nested(full =>
                                            full
                                                .Path(FullAddress)
                                                .Query(fullAddressQuery => fullAddressQuery.MatchPhrase(mp =>
                                                    mp
                                                        .Field($"{FullAddress}.{NameSpelling}")
                                                        .Query(addressQuery)
                                                        .Slop(10)))));

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

                                    x.Must(
                                        must => must.Bool(b => b.Should(shouldQueries.ToArray())),
                                        must => must.Bool(b => b.Should(shouldMunicipalityOrPostalQueries.ToArray())));
                                }
                                else
                                {
                                    x.Must(must =>
                                        must.Nested(full =>
                                            full
                                                .Path(FullAddress)
                                                .Query(fullAddressQuery => fullAddressQuery.MatchPhrase(mp =>
                                                    mp
                                                        .Field($"{FullAddress}.{NameSpelling}")
                                                        .Query(addressQuery)
                                                        .Slop(10)))
                                                .InnerHits(c =>
                                                    c.Size(1))
                                        ));
                                }
                            })
                        )
                        .Sort(new Action<SortOptionsDescriptor<AddressSearchDocument>>[]
                        {
                            s => s.Score(new ScoreSort { Order = SortOrder.Desc }),
                            s => s.Field($"{FullAddress}.{NameSpelling}.{Keyword}",
                                c =>
                                    c.Nested(n =>
                                        n.Path(FullAddress)
                                    ).Order(SortOrder.Asc))
                        });
                });

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogWarning("Failed to search for addresses: {Error}", searchResponse.ElasticsearchServerError);
                return new AddressSearchResult(Enumerable.Empty<AddressSearchDocument>().AsQueryable(), 0);
            }

            var language = DetermineLanguage(searchResponse);
            return new AddressSearchResult(searchResponse.Documents.AsQueryable(), searchResponse.Total, language);
        }

        private static Language? DetermineLanguage(SearchResponse<AddressSearchDocument> response)
        {
            if (!response.Hits.Any())
            {
                return null;
            }

            var innerHits = response.Hits.First().InnerHits;
            if (innerHits is null || !innerHits.TryGetValue(FullAddress, out var fullAddressHitResult))
            {
                return null;
            }

            if (!fullAddressHitResult.Hits.Hits.Any())
            {
                return null;
            }

            if (fullAddressHitResult.Hits.Hits.First().Source is not JsonElement source)
            {
                return null;
            }

            var language = source.GetProperty(ToCamelCase(nameof(Name.Language))).GetString();
            return Enum.Parse<Language>(language!);
        }
    }
}
