namespace AddressRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.StreetName.Events;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Projections.BackOffice;
    using Xunit;

    public class BackOfficeProjectionsTests : BackOfficeProjectionsTest
    {
        private readonly Fixture _fixture;
        private readonly TestBackOfficeContext _fakeBackOfficeContext;

        public BackOfficeProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());

            _fakeBackOfficeContext = new FakeBackOfficeContextFactory(false).CreateDbContext(Array.Empty<string>());
            BackOfficeContextMock
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fakeBackOfficeContext);
        }

        [Fact]
        public async Task GivenAddressWasProposed_ThenRelationIsAdded()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>
                {
                    { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
                })))
                .Then(async _ =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds + 1));
                    var result = await _fakeBackOfficeContext
                        .AddressPersistentIdStreetNamePersistentIds
                        .FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenAddressWasProposedAndRelationPresent_ThenNothing()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();

            var expectedRelation = await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                addressWasProposedV2.AddressPersistentLocalId,
                addressWasProposedV2.StreetNamePersistentLocalId,
                CancellationToken.None);

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>
                {
                    { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
                })))
                .Then(async _ =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds + 1));

                    var result = await _fakeBackOfficeContext
                        .AddressPersistentIdStreetNamePersistentIds
                        .FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenAddressWasRemovedV2_ThenRelationIsRemoved()
        {
            var addressWasRemovedV2 = _fixture.Create<AddressWasRemovedV2>();

            var oldAddressPersistentLocalId = _fixture.Create<int>();
            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                oldAddressPersistentLocalId,
                _fixture.Create<int>(),
                CancellationToken.None);

            await _fakeBackOfficeContext.AddIdempotentMunicipalityMergerAddress(
                oldAddressPersistentLocalId,
                addressWasRemovedV2.StreetNamePersistentLocalId,
                addressWasRemovedV2.AddressPersistentLocalId,
                CancellationToken.None);

            await Sut
                .Given(new Envelope<AddressWasRemovedV2>(new Envelope(addressWasRemovedV2, new Dictionary<string, object>
                {
                    { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
                })))
                .Then(async _ =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds + 1));

                    var result = await _fakeBackOfficeContext.MunicipalityMergerAddresses
                        .FindAsync(oldAddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenAddressWasProposedForMunicipalityMerger_ThenRelationIsAdded()
        {
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>();

            var mergedStreetNamePersistentLocalId = _fixture.Create<int>();
            _fakeBackOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .Add(new AddressPersistentIdStreetNamePersistentId(
                    addressWasProposedForMunicipalityMerger.MergedAddressPersistentLocalId,
                    mergedStreetNamePersistentLocalId));
            await _fakeBackOfficeContext.SaveChangesAsync();

            await Sut
                .Given(new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(
                    addressWasProposedForMunicipalityMerger,
                    new Dictionary<string, object> { { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow } })))
                .Then(async _ =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds + 1));
                    var addressPersistentIdStreetNamePersistentId = await _fakeBackOfficeContext
                        .AddressPersistentIdStreetNamePersistentIds
                        .FindAsync(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);

                    addressPersistentIdStreetNamePersistentId.Should().NotBeNull();
                    addressPersistentIdStreetNamePersistentId!.StreetNamePersistentLocalId
                        .Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);

                    var municipalityMergerAddress = await _fakeBackOfficeContext
                        .MunicipalityMergerAddresses
                        .FindAsync(addressWasProposedForMunicipalityMerger.MergedAddressPersistentLocalId);

                    municipalityMergerAddress.Should().NotBeNull();
                    municipalityMergerAddress!.NewAddressPersistentLocalId
                        .Should().Be(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);
                    municipalityMergerAddress.NewStreetNamePersistentLocalId
                        .Should().Be(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId);
                    municipalityMergerAddress.OldAddressPersistentLocalId
                        .Should().Be(addressWasProposedForMunicipalityMerger.MergedAddressPersistentLocalId);
                    municipalityMergerAddress.OldStreetNamePersistentLocalId
                        .Should().Be(mergedStreetNamePersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenAddressWasProposedBecauseReaddress_ThenRelationIsAdded()
        {
            var @event = _fixture.Create<AddressWasProposedBecauseOfReaddress>();

            await Sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(@event, new Dictionary<string, object>
                {
                    { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
                })))
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext
                        .AddressPersistentIdStreetNamePersistentIds
                        .FindAsync(@event.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.StreetNamePersistentLocalId.Should().Be(@event.StreetNamePersistentLocalId);
                });
        }
    }

    public abstract class BackOfficeProjectionsTest
    {
        protected const int DelayInSeconds = 1;
        protected ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections> Sut { get; }
        protected Mock<IDbContextFactory<BackOfficeContext>> BackOfficeContextMock { get; }

        protected BackOfficeProjectionsTest()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {nameof(DelayInSeconds), DelayInSeconds.ToString()}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();


            BackOfficeContextMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            Sut = new ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections>(
                CreateContext,
                () => new BackOfficeProjections(BackOfficeContextMock.Object, configuration));
        }

        protected virtual BackOfficeProjectionsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BackOfficeProjectionsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new BackOfficeProjectionsContext(options);
        }
    }
}
