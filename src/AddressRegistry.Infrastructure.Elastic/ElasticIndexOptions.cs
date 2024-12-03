namespace AddressRegistry.Infrastructure.Elastic
{
    using Microsoft.Extensions.Configuration;

    public sealed class ElasticIndexOptions
    {
        public string Name { get; init; }
        public string Alias { get; init; }

        public static ElasticIndexOptions LoadFromConfiguration(
            IConfigurationSection configuration,
            string indexNameKey = "IndexName",
            string indexAliasKey = "IndexAlias")
        {
            return new ElasticIndexOptions
            {
                Name = configuration[indexNameKey]!,
                Alias = configuration[indexAliasKey]!
            };
        }
    }
}
