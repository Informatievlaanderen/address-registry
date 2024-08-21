namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Elastic.Clients.Elasticsearch;
    using Elastic.Clients.Elasticsearch.Core.Search;
    using Elastic.Clients.Elasticsearch.QueryDsl;
    using Projections.Elastic.AddressSearch;

    public interface IAddressElasticsearchClient
    {
        Task<IEnumerable<object>> SearchStreetNames(string query, int size = 10);
    }

    public sealed class AddressElasticsearchClient : IAddressElasticsearchClient
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly IndexName _indexAlias;

        public AddressElasticsearchClient(ElasticsearchClient elasticsearchClient, IndexName indexAlias)
        {
            _elasticsearchClient = elasticsearchClient;
            _indexAlias = indexAlias;
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
                                
                            }
                        }
                    }
                }
            }

        }
    }
}
