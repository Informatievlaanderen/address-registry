namespace AddressRegistry.Projections.Elastic.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressSearch;
    using AutoFixture;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using StreetName;
    using Xunit;

    [Collection("Elastic")]
    public class AddressSearchElasticClientTests : IClassFixture<ElasticsearchClientTestFixture>
    {
        private readonly ElasticsearchClientTestFixture _clientFixture;
        private readonly Fixture _fixture;

        public AddressSearchElasticClientTests(ElasticsearchClientTestFixture clientFixture)
        {
            _clientFixture = clientFixture;
            _fixture = new Fixture();
            _fixture.Customize<AddressPosition>(composer =>
            {
                return composer.FromFactory(() => new AddressPosition(
                        new Point(73862.07, 211634.58),
                        _fixture.Create<GeometryMethod>(),
                        _fixture.Create<GeometrySpecification>()))
                    .OmitAutoProperties();
            });
        }

        [Fact]
        public async Task CreateDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<AddressSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).Single();
            actualDocument.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<AddressSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var updateDocument = _fixture.Create<AddressSearchDocument>();
            updateDocument.AddressPersistentLocalId = givenDocument.AddressPersistentLocalId;
            await client.UpdateDocument(updateDocument, CancellationToken.None);

            var actualDocument = (await client.GetDocuments([updateDocument.AddressPersistentLocalId], CancellationToken.None)).Single();

            actualDocument.Should().BeEquivalentTo(updateDocument);
        }

        [Fact]
        public async Task GetDocuments()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<AddressSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).Single();

            actualDocument.Should().BeEquivalentTo(givenDocument);
        }

        [Fact]
        public async Task UpdateDocument_ShouldUpdateOnlyProvidedFields()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<AddressSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var documentUpdate = new AddressSearchPartialDocument(DateTimeOffset.Now)
            {
                Status = _fixture.Create<AddressStatus>(),
                OfficiallyAssigned = _fixture.Create<bool>(),
                AddressPosition = _fixture.Create<AddressPosition>(),
                BoxNumber = _fixture.Create<string>()
            };

            EnsureAllPropertiesAreNotNull(documentUpdate);

            await client.PartialUpdateDocument(
                givenDocument.AddressPersistentLocalId,
                documentUpdate,
                CancellationToken.None);

            var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).Single();

            actualDocument.AddressPersistentLocalId.Should().Be(givenDocument.AddressPersistentLocalId);
            actualDocument.Status.Should().Be(documentUpdate.Status);
            actualDocument.Active.Should().Be(documentUpdate.Active!.Value);
            actualDocument.OfficiallyAssigned.Should().Be(documentUpdate.OfficiallyAssigned!.Value);
            actualDocument.AddressPosition.Should().BeEquivalentTo(documentUpdate.AddressPosition);
            actualDocument.BoxNumber.Should().BeEquivalentTo(documentUpdate.BoxNumber);
            actualDocument.VersionTimestamp.Should().Be(documentUpdate.VersionTimestamp);
        }

        [Fact]
        public async Task DeleteDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<AddressSearchDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            await client.DeleteDocument(givenDocument.AddressPersistentLocalId, CancellationToken.None);
            var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).SingleOrDefault();

            actualDocument.Should().BeNull();
        }

        private async Task<IAddressSearchElasticClient> BuildClient()
        {
            var indexName = $"test-{Guid.NewGuid():N}";
            await _clientFixture.CreateIndex(indexName);
            return new AddressSearchElasticClient(_clientFixture.Client, indexName);
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
