﻿namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using AddressRegistry.Projections.Elastic.AddressSearch;
    using AddressRegistry.StreetName;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Name = AddressRegistry.Infrastructure.Elastic.Name;

    public sealed class AddressApiSearchElasticsearchClient: AddressApiElasticsearchClientBase, IAddressApiSearchElasticsearchClient
    {
        private const string FullAddress = "fullAddress";

        public AddressApiSearchElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias)
            : base(elasticsearchClient, indexAlias)
        { }

        public async Task<AddressSearchResult> SearchAddresses(
            string addressQuery,
            string? nisCode,
            AddressStatus? status,
            int size = 10)
        {
            var searchResponse = await ElasticsearchClient.SearchAsync<AddressSearchDocument>(IndexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(size)
                        .Query(q =>
                            q.Bool(x =>
                            {
                                var filterConditions = new List<Action<QueryDescriptor<AddressSearchDocument>>>();
                                var mustConditions = new List<Action<QueryDescriptor<AddressSearchDocument>>>();

                                if (status is not null)
                                {
                                    filterConditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.Status))}"!)
                                        .Value(status.ToString()!)));
                                }
                                else
                                {
                                    filterConditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.Active))}"!)
                                        .Value(true)));
                                }

                                if (!string.IsNullOrWhiteSpace(nisCode))
                                {
                                    filterConditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.NisCode))}"!)
                                        .Value(nisCode)));
                                }

                                mustConditions.Add(q2 =>
                                    q2.Nested(full =>
                                        full
                                            .Path(FullAddress)
                                            .Query(fullAddressQuery => fullAddressQuery.Bool(b =>
                                                b.Should(s =>
                                                        s.MatchPhrase(mp => mp
                                                            .Field($"{FullAddress}.{NameSpelling}")
                                                            .Query(addressQuery)
                                                            .Slop(10)),
                                                    s =>
                                                        s.Match(mp => mp
                                                            .Field($"{FullAddress}.{NameSpelling}")
                                                            .Query(addressQuery)
                                                            .Fuzziness(new Fuzziness("AUTO"))
                                                            .Operator(Operator.And))
                                                        )))
                                            .InnerHits(c =>
                                                c.Size(1))));

                                x.Filter(filterConditions.ToArray());
                                x.Must(mustConditions.ToArray());
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
                throw new ElasticsearchClientException("Failed to search for addresses", searchResponse.ElasticsearchServerError, searchResponse.DebugInformation);
            }

            var language = DetermineLanguage(searchResponse);
            return new AddressSearchResult(searchResponse.Documents, searchResponse.Total, language);
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
