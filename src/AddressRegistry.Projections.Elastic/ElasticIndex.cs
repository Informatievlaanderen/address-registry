namespace AddressRegistry.Projections.Elastic
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.Extensions.Configuration;
    using ExistsRequest = global::Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

    public class ElasticIndex
    {
        private readonly ElasticsearchClient _client;

        private readonly string _indexName;
        private readonly string _indexAlias;

        public ElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
        {
            _client = client;
            var elasticOptions = configuration.GetSection("Elastic");
            _indexName = elasticOptions["IndexName"]!;
            _indexAlias = elasticOptions["IndexAlias"]!;
        }

        public async Task CreateIfNotExist(CancellationToken ct)
        {
            var indexName = Indices.Index(_indexName);
            var response = await _client.Indices.ExistsAsync(new ExistsRequest(indexName), ct);
            if (response.Exists)
            {
                return;
            }
            /*

        public Municipality Municipality { get; set; }
        public PostalInfo PostalInfo { get; set; }
        public StreetName StreetName { get; set; }
        public Name[] FullAddress { get; set; }

        public AddressPosition AddressPosition { get; set; }
             */

            var createResponse = await _client.Indices.CreateAsync<AddressSearchDocument>(indexName, c =>
            {
                // c.Aliases(dictionary => dictionary.Add())
                c.Mappings(map => map
                    .Properties(p => p
                            .IntegerNumber(x => x.AddressPersistentLocalId)
                            .IntegerNumber(x => x.ParentAddressPersistentLocalId)
                            .Date(x => x.VersionTimestamp)
                            .Keyword(x => x.Status)
                            .Boolean(x => x.Active)
                            .Boolean(x => x.OfficiallyAssigned)
                            .Keyword(x => x.HouseNumber)
                            .Keyword(x => x.BoxNumber)
                            .Keyword(x => x.Municipality.NisCode)
                            .Nested(x => x.Municipality.Names)

                        // .Object(o => o.Municipality, objConfig => objConfig
                        //     .Properties(obj => obj
                        //         .Text(x => x.Municipality.NisCode)
                        //     )
                    ));
            }, ct);

            if (!createResponse.Acknowledged)
            {
                // todo-rik throw?
            }
        }
    }
}
