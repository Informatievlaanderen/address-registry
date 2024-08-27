namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Projections.Elastic.AddressSearch;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Convertors;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Core.Search;
    using global::Elastic.Clients.Elasticsearch.QueryDsl;
    using Microsoft.Extensions.Logging;

    public interface IAddressApiElasticsearchClient
    {
        Task<IEnumerable<object>> SearchStreetNames(string query, int size = 10);

        Task<AddressSearchResult> ListAddresses(
            string? streetNameId,
            string? streetName,
            string? homonymAddition,
            string? houseNumber,
            string? boxNumber,
            string? postalCode,
            string? nisCode,
            string? municipalityName,
            string? status,
            int? from,
            int? size);
    }

    public sealed class AddressApiElasticsearchClient : IAddressApiElasticsearchClient
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

        public async Task<IEnumerable<object>> SearchStreetNames(string query, int size = 10)
        {
            var searchResponse = await _elasticsearchClient.SearchAsync<AddressSearchDocument>(_indexAlias,
                descriptor =>
                {
                    descriptor
                        .Size(0)
                        .Query(q =>
                            q.Nested(nested => nested
                                .Path("streetName.names").Query(nestedQuery => nestedQuery
                                    .FunctionScore(fs => fs
                                        .Query(fsq => fsq
                                                .Bool(fsqb =>
                                                    fsqb.Should(should => should
                                                        .ConstantScore(cs => cs.Boost(5.0F).Filter(csf => csf.Prefix(pfx => pfx.Field("streetName.names.spelling").Value(query))))
                                                        .ConstantScore(cs => cs.Boost(1).Filter(csf => csf.Match(m => m.Field("streetName.names.spelling").Query(query)))))
                                                    )
                                        )
                                        .Functions(f => f
                                            .ScriptScore(ss => ss
                                                .Script(script => script
                                                    .Source("Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)")
                                                )
                                            )
                                        )
                                        .BoostMode(FunctionBoostMode.Replace)
                                        .ScoreMode(FunctionScoreMode.Sum)
                                    )
                                )
                            )
                        )
                        .Aggregations(a => a.Add("unique_street_names", aggs =>
                        {
                            aggs.Nested(nested => nested.Path("streetName.names"));
                            aggs.Aggregations(innerAggs => innerAggs
                                .Add("filtered_names", aggs2 =>
                                {
                                    aggs2.Filter(filter => filter
                                        .Bool(b => b
                                            .Should(m => m
                                                .Prefix(pfx => pfx.Field("streetName.names.spelling").Value(query))
                                                .Match(m2 => m2.Field("streetName.names.spelling").Query(query))
                                            )
                                        )
                                    );
                                    aggs2.Aggregations(innerAggs2 => innerAggs2
                                        .Add("street_names", aggs3 =>
                                        {
                                            aggs3.Terms(t => t
                                                .Field("streetName.names.spelling.keyword")
                                                .Size(size)
                                                .Order(new List<KeyValuePair<Field, SortOrder>>
                                                {
                                                    new (new Field("top_hit_score"), SortOrder.Desc)
                                                }));
                                            aggs3.Aggregations(innerAggs3 => innerAggs3
                                                .Add("top_street_nane", aggs4 =>
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
                                            );
                                        })
                                    );
                                })
                            );
                        }));
                });
            if (!searchResponse.IsValidResponse)
                return new List<object>();

            var names = new List<object>();
            var uniqueStreetNamesAgg = searchResponse.Aggregations.GetNested("unique_street_names");
            if (uniqueStreetNamesAgg != null)
            {
                // Access the inner 'filtered_names' nested aggregation
                var filteredNamesAgg = uniqueStreetNamesAgg.Aggregations.GetNested("filtered_names");

                if (filteredNamesAgg != null)
                {
                    // Access the 'street_names' terms aggregation
                    var streetNamesTermsAgg = filteredNamesAgg.Aggregations.GetStringTerms("street_names");

                    foreach (var bucket in streetNamesTermsAgg.Buckets)
                    {
                        // Each bucket may have a 'top_street_name' hits aggregation
                        var topStreetNameHits = bucket.Aggregations.GetTopHits("top_street_name");

                        if (topStreetNameHits != null)
                        {
                            foreach (var hit in topStreetNameHits.Hits.Hits)
                            {
                                //TODO: Access the 'streetName.names.spelling' field value
                                names.Add(hit.Source);
                            }
                        }
                    }
                }
            }

            return names;
        }

        public async Task<AddressSearchResult> ListAddresses(
            string? streetNameId,
            string? streetName,
            string? homonymAddition,
            string? houseNumber,
            string? boxNumber,
            string? postalCode,
            string? nisCode,
            string? municipalityName,
            string? status,
            int? from,
            int? size)
        {
            var searchResponse = await _elasticsearchClient.SearchAsync<AddressSearchDocument>(_indexAlias, descriptor =>
            {
                descriptor.Size(size);
                descriptor.From(from);
                descriptor.TrackTotalHits(new TrackHits(true));
                descriptor.Sort(new List<SortOptions>{SortOptions.Field(new Field(ToCamelCase(nameof(AddressSearchDocument.AddressPersistentLocalId))), new FieldSort{ Order = SortOrder.Asc})});

                if (!string.IsNullOrEmpty(streetNameId)
                    || !string.IsNullOrEmpty(streetName)
                    || !string.IsNullOrEmpty(homonymAddition)
                    || !string.IsNullOrEmpty(houseNumber)
                    || !string.IsNullOrEmpty(boxNumber)
                    || !string.IsNullOrEmpty(postalCode)
                    || !string.IsNullOrEmpty(nisCode)
                    || !string.IsNullOrEmpty(municipalityName)
                    || !string.IsNullOrEmpty(status))
                {
                    descriptor.Query(query =>
                    {
                        query.Bool(b =>
                        {
                            if (!string.IsNullOrEmpty(streetNameId))
                                b.Must(m => m
                                    .Term(t => t
                                        .Field($"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.StreetNamePersistentLocalId))}"!)
                                        .Value(streetNameId)));

                            var streetNameNames = $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.Names))}";
                            if (!string.IsNullOrEmpty(streetName))
                                b.Must(m => m.Nested(t => t.Path(streetNameNames!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{streetNameNames}.{NameSpelling}.{Keyword}"!)
                                        .Value(streetName)))));

                            var streetNameHomonymAdditions = $"{ToCamelCase(nameof(AddressSearchDocument.StreetName))}.{ToCamelCase(nameof(AddressSearchDocument.StreetName.HomonymAdditions))}";
                            if (!string.IsNullOrEmpty(homonymAddition))
                                b.Must(m => m.Nested(t => t.Path(streetNameHomonymAdditions!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{streetNameHomonymAdditions}.{NameSpelling}.{Keyword}"!)
                                        .Value(homonymAddition)))));

                            if (!string.IsNullOrEmpty(houseNumber))
                                b.Must(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressSearchDocument.HouseNumber))}"!)
                                    .Value(houseNumber)));

                            if (!string.IsNullOrEmpty(boxNumber))
                                b.Must(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressSearchDocument.BoxNumber))}"!)
                                    .Value(boxNumber)));

                            if (!string.IsNullOrEmpty(postalCode))
                                b.Must(m => m.Term(t => t
                                    .Field(
                                        $"{ToCamelCase(nameof(AddressSearchDocument.PostalInfo))}.{ToCamelCase(nameof(AddressSearchDocument.PostalInfo.PostalCode))}"!)
                                    .Value(postalCode)));

                            if (!string.IsNullOrEmpty(nisCode))
                                b.Must(m => m.Term(t => t
                                    .Field(
                                        $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.NisCode))}"!)
                                    .Value(nisCode)));

                            var municipalityNames = $"{ToCamelCase(nameof(AddressSearchDocument.Municipality))}.{ToCamelCase(nameof(AddressSearchDocument.Municipality.Names))}";
                            if (!string.IsNullOrEmpty(municipalityName))
                                b.Must(m => m.Nested(t => t.Path($"{municipalityNames}"!)
                                    .Query(q => q.Term(t2 => t2
                                        .Field($"{municipalityNames}.{NameSpelling}.{Keyword}"!)
                                        .Value(municipalityName)))));

                            if (!string.IsNullOrEmpty(status) && Enum.TryParse(typeof(AdresStatus), status, true, out var parsedStatus))
                            {
                                var addressStatus = StreetNameAddressStatusExtensions.ConvertFromAdresStatus((AdresStatus)parsedStatus);
                                b.Must(m => m.Term(t => t
                                    .Field($"{ToCamelCase(nameof(AddressSearchDocument.Status))}"!)
                                    .Value(Enum.GetName(addressStatus)!)));
                            }
                        });

                    });
                }
            });

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogWarning("Failed to search for addresses: {Error}", searchResponse.ElasticsearchServerError);
                return new AddressSearchResult(Enumerable.Empty<AddressSearchDocument>().AsQueryable(), 0);
            }

            return new AddressSearchResult(searchResponse.Documents.AsQueryable(), searchResponse.Total);
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
