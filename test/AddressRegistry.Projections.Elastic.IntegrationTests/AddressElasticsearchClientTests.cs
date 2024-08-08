// namespace AddressRegistry.Projections.Elastic.IntegrationTests
// {
//     using System;
//     using System.Linq;
//     using System.Threading;
//     using System.Threading.Tasks;
//     using AddressSearch;
//     using AutoFixture;
//     using FluentAssertions;
//     using NetTopologySuite.Geometries;
//     using StreetName;
//     using Xunit;
//
//     public class AddressElasticsearchClientTests : IClassFixture<ElasticsearchClientTestFixture>
//     {
//         private readonly ElasticsearchClientTestFixture _clientFixture;
//         private readonly Fixture _fixture;
//
//         public AddressElasticsearchClientTests(ElasticsearchClientTestFixture clientFixture)
//         {
//             _clientFixture = clientFixture;
//             _fixture = new Fixture();
//             _fixture.Customize<AddressPosition>(composer =>
//             {
//                 return composer.FromFactory(() => new AddressPosition(
//                     new Point(73862.07, 211634.58),
//                     _fixture.Create<GeometryMethod>(),
//                     _fixture.Create<GeometrySpecification>()))
//                     .OmitAutoProperties();
//             });
//         }
//
//         [Fact]
//         public async Task CreateDocument()
//         {
//             var client = await BuildClient();
//
//             var givenDocument = _fixture.Create<AddressSearchDocument>();
//             await client.CreateDocument(givenDocument, CancellationToken.None);
//         }
//
//         [Fact]
//         public async Task GetDocuments()
//         {
//             var client = await BuildClient();
//
//             var givenDocument = _fixture.Create<AddressSearchDocument>();
//             await client.CreateDocument(givenDocument, CancellationToken.None);
//
//             var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).Single();
//
//             actualDocument.AddressPersistentLocalId.Should().Be(givenDocument.AddressPersistentLocalId);
//             actualDocument.ParentAddressPersistentLocalId.Should().Be(givenDocument.ParentAddressPersistentLocalId);
//             actualDocument.VersionTimestamp.Should().Be(givenDocument.VersionTimestamp);
//             actualDocument.Status.Should().Be(givenDocument.Status);
//             actualDocument.Active.Should().Be(givenDocument.Active);
//             actualDocument.OfficiallyAssigned.Should().Be(givenDocument.OfficiallyAssigned);
//             actualDocument.HouseNumber.Should().Be(givenDocument.HouseNumber);
//             actualDocument.BoxNumber.Should().Be(givenDocument.BoxNumber);
//             actualDocument.Municipality.Should().BeEquivalentTo(givenDocument.Municipality);
//             actualDocument.PostalInfo.Should().BeEquivalentTo(givenDocument.PostalInfo);
//             actualDocument.StreetName.Should().BeEquivalentTo(givenDocument.StreetName);
//             actualDocument.FullAddress.Should().BeEquivalentTo(givenDocument.FullAddress);
//             actualDocument.AddressPosition.Should().BeEquivalentTo(givenDocument.AddressPosition);
//         }
//
//         [Fact]
//         public async Task UpdateDocument_ShouldUpdateOnlyProvidedFields()
//         {
//             var client = await BuildClient();
//
//             var givenDocument = _fixture.Create<AddressSearchDocument>();
//             await client.CreateDocument(givenDocument, CancellationToken.None);
//
//             var documentUpdate = new AddressSearchPartialDocument(DateTimeOffset.Now)
//             {
//                 HouseNumber = "2",
//                 BoxNumber = "A"
//             };
//
//             await client.PartialUpdateDocument(
//                 givenDocument.AddressPersistentLocalId,
//                 documentUpdate,
//                 CancellationToken.None);
//
//             var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).Single();
//
//             actualDocument.AddressPersistentLocalId.Should().Be(givenDocument.AddressPersistentLocalId);
//             actualDocument.Status.Should().Be(givenDocument.Status);
//             actualDocument.HouseNumber.Should().Be(documentUpdate.HouseNumber);
//             actualDocument.BoxNumber.Should().Be(documentUpdate.BoxNumber);
//             actualDocument.VersionTimestamp.Should().Be(documentUpdate.VersionTimestamp);
//         }
//
//         [Fact]
//         public async Task DeleteDocument()
//         {
//             var client = await BuildClient();
//
//             var givenDocument = _fixture.Create<AddressSearchDocument>();
//             await client.CreateDocument(givenDocument, CancellationToken.None);
//
//             await client.DeleteDocument(givenDocument.AddressPersistentLocalId, CancellationToken.None);
//             var actualDocument = (await client.GetDocuments([givenDocument.AddressPersistentLocalId], CancellationToken.None)).SingleOrDefault();
//
//             actualDocument.Should().BeNull();
//         }
//
//         private async Task<IAddressElasticsearchClient> BuildClient()
//         {
//             var indexName = $"test-{Guid.NewGuid():N}";
//             await _clientFixture.CreateIndex(indexName);
//             return new AddressElasticsearchClient(_clientFixture.Client, indexName);
//         }
//     }
// }
