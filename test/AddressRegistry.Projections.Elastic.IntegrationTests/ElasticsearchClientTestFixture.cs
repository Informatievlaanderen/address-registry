namespace AddressRegistry.Projections.Elastic.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressSearch;
    using Be.Vlaanderen.Basisregisters.DockerUtilities;
    using Ductus.FluentDocker.Services;
    using global::Elastic.Clients.Elasticsearch;
    using Xunit;

    public class ElasticsearchClientTestFixture : IAsyncLifetime
    {
        public ElasticsearchClient Client { get; private set; }

        private ICompositeService? _dockerService;

        public async Task InitializeAsync()
        {
            var clientSettings = new ElasticsearchClientSettings(new Uri("http://localhost:39200"))
                .DisableDirectStreaming();

            Client = new ElasticsearchClient(clientSettings);

            _dockerService = DockerComposer.Compose("elastic.yml", "address-elastic-integration-tests");
            await WaitForElasticsearchToBecomeAvailable();
        }

        private async Task WaitForElasticsearchToBecomeAvailable()
        {
            foreach (var _ in Enumerable.Range(0, 60))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    await CreateIndex("initial");
                    break;
                }
                catch
                {
                    // Try again
                }
            }
        }

        public async Task CreateIndex(string name)
        {
            var index = new AddressSearchElasticIndex(Client, new ElasticIndexOptions
            {
                Name = name
            });
            await index.CreateIndexIfNotExist(CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            _dockerService?.Dispose();
            return Task.CompletedTask;
        }
    }
}
