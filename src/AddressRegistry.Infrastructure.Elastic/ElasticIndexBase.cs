namespace AddressRegistry.Infrastructure.Elastic
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.IndexManagement;

    public abstract class ElasticIndexBase
    {
        protected ElasticsearchClient Client { get; }
        protected string IndexName { get; }
        protected string IndexAlias { get; }

        protected ElasticIndexBase(ElasticsearchClient client, ElasticIndexOptions indexOptions)
        {
            Client = client;
            IndexName = indexOptions.Name;
            IndexAlias = indexOptions.Alias;
        }

        public async Task CreateAliasIfNotExist(CancellationToken ct)
        {
            var aliasResponse = await Client.Indices.GetAliasAsync(new GetAliasRequest(Names.Parse(IndexAlias)), ct);
            if (aliasResponse.IsValidResponse && aliasResponse.Aliases.Any())
                return;

            await Client.Indices.PutAliasAsync(new PutAliasRequest(IndexName, IndexAlias), ct);
        }
    }
}
