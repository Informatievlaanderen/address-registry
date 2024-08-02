namespace AddressRegistry.Projections.Elastic.Infrastructure
{
    using System;
    using Autofac;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Transport;
    using Microsoft.Extensions.Configuration;

    public class ElasticModule : Module
    {
        public const string ConfigurationSectionName = "Elastic";

        private readonly IConfiguration _configuration;

        public ElasticModule(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var elasticOptions = _configuration.GetSection(ConfigurationSectionName);

            var clientSettings = new ElasticsearchClientSettings(new Uri(elasticOptions["Uri"]!));

            var apiKey = elasticOptions.GetValue("ApiKey", string.Empty);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                clientSettings = clientSettings.Authentication(new ApiKey(apiKey));
            }

            builder
                .Register<ElasticsearchClient>(_ => new ElasticsearchClient(clientSettings))
                .SingleInstance();
        }
    }
}
