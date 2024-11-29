namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Projections.Elastic.AddressSearch;
    using StreetName;
    using Name = AddressRegistry.Infrastructure.Elastic.Name;

    public sealed partial class AddressApiElasticsearchClient
    {
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
                                var conditions = new List<Action<QueryDescriptor<AddressSearchDocument>>>();

                                if (status is not null)
                                {
                                    conditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.Status))}"!)
                                        .Value(status.ToString()!)));
                                }
                                else
                                {
                                    conditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.Active))}"!)
                                        .Value(true)));
                                }

                                //TODO-rik in List gebeurt gemeenteNaam filter via elastic, mss beter ook zo ipv cache of de List aanpassen om de cache te gebruiken?

                                if (!string.IsNullOrWhiteSpace(nisCode))
                                {
                                    conditions.Add(m => m.Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.NisCode))}"!)
                                        .Value(nisCode)));
                                }

                                conditions.Add(q2 =>
                                    q2.Nested(full =>
                                        full
                                            .Path(FullAddress)
                                            .Query(fullAddressQuery => fullAddressQuery.MatchPhrase(mp =>
                                                mp
                                                    .Field($"{FullAddress}.{NameSpelling}")
                                                    .Query(addressQuery)
                                                    .Slop(10)))
                                            .InnerHits(c =>
                                                c.Size(1))));

                                x.Must(conditions.ToArray());
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
                return AddressSearchResult.Empty;
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
