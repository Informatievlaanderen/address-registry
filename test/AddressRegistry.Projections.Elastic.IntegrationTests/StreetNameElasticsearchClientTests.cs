namespace AddressRegistry.Projections.Elastic.IntegrationTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.Infrastructure.Elastic.Exceptions;
    using AutoFixture;
    using Consumer.Read.StreetName.Projections.Elastic;
    using FluentAssertions;
    using Xunit;

    [Xunit.Collection("Elastic")]
    public class StreetNameElasticsearchClientTests : IClassFixture<ElasticsearchClientTestFixture>
    {
        private readonly ElasticsearchClientTestFixture _clientFixture;
        private readonly Fixture _fixture;

        public StreetNameElasticsearchClientTests(ElasticsearchClientTestFixture clientFixture)
        {
            _clientFixture = clientFixture;
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);
        }

        [Fact]
        public async Task UpdateDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var updateDocument = _fixture.Create<StreetNameSearchDocument>();
            updateDocument.StreetNamePersistentLocalId = givenDocument.StreetNamePersistentLocalId;
            await client.UpdateDocument(updateDocument, CancellationToken.None);

            var actualDocument = await client.GetDocument(updateDocument.StreetNamePersistentLocalId, CancellationToken.None);

            actualDocument.Should().BeEquivalentTo(updateDocument);
        }

        [Fact]
        public async Task GetDocuments()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var actualDocument = await client.GetDocument(givenDocument.StreetNamePersistentLocalId, CancellationToken.None);

            actualDocument.Should().BeEquivalentTo(givenDocument);
        }

        [Fact]
        public async Task UpdateDocument_ShouldUpdateOnlyProvidedFields()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var documentUpdate = new StreetNameSearchPartialDocument(DateTimeOffset.Now)
            {
                Status = _fixture.Create<AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus>(),
            };

            EnsureAllPropertiesAreNotNull(documentUpdate);

            await client.PartialUpdateDocument(
                givenDocument.StreetNamePersistentLocalId,
                documentUpdate,
                CancellationToken.None);

            var actualDocument = await client.GetDocument(givenDocument.StreetNamePersistentLocalId, CancellationToken.None);

            actualDocument.StreetNamePersistentLocalId.Should().Be(givenDocument.StreetNamePersistentLocalId);
            actualDocument.Status.Should().Be(documentUpdate.Status);
            actualDocument.Active.Should().Be(documentUpdate.Active!.Value);
            actualDocument.VersionTimestamp.Should().Be(documentUpdate.VersionTimestamp);
        }

        [Fact]
        public async Task DeleteDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            await client.DeleteDocument(givenDocument.StreetNamePersistentLocalId, CancellationToken.None);

            await Assert.ThrowsAsync<ElasticsearchClientException>(
                () => client.GetDocument(givenDocument.StreetNamePersistentLocalId, CancellationToken.None));
        }

        private async Task<IStreetNameElasticsearchClient> BuildClient()
        {
            var indexName = $"test-{Guid.NewGuid():N}";
            var index = new StreetNameElasticIndex(_clientFixture.Client, new ElasticIndexOptions
            {
                Name = indexName
            });
            await index.CreateIndexIfNotExist(CancellationToken.None);
            return new StreetNameElasticsearchClient(_clientFixture.Client, indexName);
        }

        private void EnsureAllPropertiesAreNotNull(object value)
        {
            var properties = value.GetType().GetProperties();
            properties.Should().NotBeEmpty();
            foreach (var pi in properties)
            {
                pi.GetValue(value).Should().NotBeNull();
            }
        }
    }
}
