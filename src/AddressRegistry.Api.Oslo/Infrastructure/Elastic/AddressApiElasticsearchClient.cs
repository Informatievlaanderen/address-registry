﻿namespace AddressRegistry.Api.Oslo.Infrastructure.Elastic
{
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.Extensions.Logging;

    public sealed partial class AddressApiElasticsearchClient: AddressApiElasticsearchClientBase, IAddressApiElasticsearchClient
    {
        private const string FullAddress = "fullAddress";

        private readonly ILogger<AddressApiElasticsearchClient> _logger;

        public AddressApiElasticsearchClient(
            ElasticsearchClient elasticsearchClient,
            string indexAlias,
            ILoggerFactory loggerFactory)
            : base(elasticsearchClient, indexAlias)
        {
            _logger = loggerFactory.CreateLogger<AddressApiElasticsearchClient>();
        }
    }
}
