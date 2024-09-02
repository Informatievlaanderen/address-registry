namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;
    using Projections.Elastic.AddressSearch;

    public sealed partial class AddressApiElasticsearchClient
    {
        public async Task<AddressSearchResult> SearchAddresses(
            string streetNameQuery,
            string houseNumberQuery,
            string? boxNumberQuery,
            string? postalCodeQuery,
            string? municipalityOrPostalName,
            bool mustBeInMunicipality,
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
                                if (mustBeInMunicipality)
                                {
                                    var shouldQueries = new List<Action<QueryDescriptor<AddressSearchDocument>>>();

                                    shouldQueries.Add(q2 => q2.Nested(nested => QueryStreetNames(nested, [streetNameQuery])));
                                    shouldQueries.Add(q2 => q2.Prefix(pfx => pfx.Field(document => document.HouseNumber).Value(houseNumberQuery).Boost(2)));
                                    shouldQueries.Add(q2 => q2.Term(t => t.Field(document => document.HouseNumber).Value(houseNumberQuery).Boost(5)));

                                    if (!string.IsNullOrWhiteSpace(boxNumberQuery))
                                        shouldQueries.Add(q2 => q2.Prefix(pfx => pfx.Field(document => document.BoxNumber).Value(boxNumberQuery)));

                                    if (!string.IsNullOrWhiteSpace(postalCodeQuery))
                                        shouldQueries.Add(q2 => q2.Prefix(pfx => pfx.Field(document => document.PostalInfo.PostalCode).Value(postalCodeQuery)));

                                    var shouldMunicipalityOrPostalQueries = new List<Action<QueryDescriptor<AddressSearchDocument>>>();
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
                                    var shouldQueries = new List<Action<QueryDescriptor<AddressSearchDocument>>>();

                                    shouldQueries.Add(q2 => q2.Nested(nested => QueryStreetNames(nested, [streetNameQuery])));
                                    shouldQueries.Add(q2 => q2.Prefix(pfx => pfx.Field(document => document.HouseNumber).Value(houseNumberQuery).Boost(2)));
                                    shouldQueries.Add(q2 => q2.Term(t => t.Field(document => document.HouseNumber).Value(houseNumberQuery).Boost(5)));

                                    if (!string.IsNullOrWhiteSpace(boxNumberQuery))
                                        shouldQueries.Add(q2 => q2.Prefix(pfx => pfx.Field(document => document.BoxNumber).Value(boxNumberQuery)));

                                    if (!string.IsNullOrWhiteSpace(postalCodeQuery))
                                        shouldQueries.Add(q2 => q2.Prefix(pfx => pfx.Field(document => document.PostalInfo.PostalCode).Value(postalCodeQuery)));

                                    if (!string.IsNullOrWhiteSpace(municipalityOrPostalName))
                                        shouldQueries.Add(q2 =>
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

                                    if (!string.IsNullOrWhiteSpace(municipalityOrPostalName))
                                    {
                                        shouldQueries.Add(q2 =>
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

                                    x.Should(shouldQueries.ToArray());
                                }
                            })
                        );
                });

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogWarning("Failed to search for addresses: {Error}", searchResponse.ElasticsearchServerError);
                return new AddressSearchResult(Enumerable.Empty<AddressSearchDocument>().AsQueryable(), 0);
            }

            return new AddressSearchResult(searchResponse.Documents.AsQueryable(), searchResponse.Total);
        }
    }
}
