namespace AddressRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using CloudNative.CloudEvents;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Newtonsoft.Json;
    using Projections.Feed;
    using Projections.Feed.AddressFeed;
    using Projections.Feed.Contract;
    using Xunit;

    public sealed class AddressFeedProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly FeedContext _feedContext;

        private ConnectedProjectionTest<FeedContext, AddressFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }

        public AddressFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            _feedContext = CreateContext();
            Sut = new ConnectedProjectionTest<FeedContext, AddressFeedProjections>(() => _feedContext,
                () => new AddressFeedProjections(ChangeFeedServiceMock.Object));

            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customizations.Insert(0, new WithoutUnknownStreetNameAddressStatus());

            SetupChangeFeedServiceMock();
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName_ThenFeedItemAndDocumentAreAdded()
        {
            var addressWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>();

            var position = 2L;

            await Sut
                .Given(
                    _fixture.Create<MigratedStreetNameWasImported>(),
                    CreateEnvelope(addressWasMigrated, position))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasMigrated.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().Be(addressWasMigrated.IsRemoved);
                    document.RecordCreatedAt.Should().Be(addressWasMigrated.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(addressWasMigrated.Provenance.Timestamp);

                    document.Document.PersistentLocalId.Should().Be(addressWasMigrated.AddressPersistentLocalId);
                    document.Document.StreetNamePersistentLocalId.Should().Be(addressWasMigrated.StreetNamePersistentLocalId);
                    document.Document.HouseNumber.Should().Be(addressWasMigrated.HouseNumber);
                    document.Document.BoxNumber.Should().Be(addressWasMigrated.BoxNumber);
                    document.Document.PostalCode.Should().Be(addressWasMigrated.PostalCode ?? string.Empty);
                    document.Document.OfficiallyAssigned.Should().Be(addressWasMigrated.OfficiallyAssigned);

                    var feedItem = await context.AddressFeed.SingleOrDefaultAsync(x => x.PersistentLocalId == addressWasMigrated.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position, addressWasMigrated);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.CreateV1,
                            addressWasMigrated.AddressPersistentLocalId.ToString(),
                            addressWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(addressWasMigrated.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressWasMigratedToStreetName.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedV2_ThenFeedItemAndDocumentAreAdded()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.RecordCreatedAt.Should().Be(addressWasProposedV2.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(addressWasProposedV2.Provenance.Timestamp);

                    document.Document.PersistentLocalId.Should().Be(addressWasProposedV2.AddressPersistentLocalId);
                    document.Document.StreetNamePersistentLocalId.Should().Be(addressWasProposedV2.StreetNamePersistentLocalId);
                    document.Document.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
                    document.Document.BoxNumber.Should().Be(addressWasProposedV2.BoxNumber);
                    document.Document.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
                    document.Document.Status.Should().Be(AdresStatus.Voorgesteld);
                    document.Document.OfficiallyAssigned.Should().BeTrue();

                    var feedItem = await context.AddressFeed.SingleOrDefaultAsync(x => x.PersistentLocalId == addressWasProposedV2.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position, addressWasProposedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.CreateV1,
                            addressWasProposedV2.AddressPersistentLocalId.ToString(),
                            addressWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressWasProposedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenAddressWasApproved_ThenStatusIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = _fixture.Create<AddressWasApproved>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasApproved, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasApproved.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.InGebruik);
                    document.LastChangedOn.Should().Be(addressWasApproved.Provenance.Timestamp);

                    var feedItem = await context.AddressFeed.LastAsync(x => x.PersistentLocalId == addressWasApproved.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, addressWasApproved);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasApproved.AddressPersistentLocalId.ToString(),
                            addressWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressWasApproved.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed_ThenStatusIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var addressWasCorrected = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasApproved, position + 1),
                    CreateEnvelope(addressWasCorrected, position + 2))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasCorrected.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.Voorgesteld);
                    document.LastChangedOn.Should().Be(addressWasCorrected.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected_ThenStatusIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasRejected = _fixture.Create<AddressWasRejected>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasRejected, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasRejected.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.Afgekeurd);
                    document.LastChangedOn.Should().Be(addressWasRejected.Provenance.Timestamp);

                    var feedItem = await context.AddressFeed.LastAsync(x => x.PersistentLocalId == addressWasRejected.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, addressWasRejected);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasRejected.AddressPersistentLocalId.ToString(),
                            addressWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressWasRejected.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2_ThenStatusIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasApproved, position + 1),
                    CreateEnvelope(addressWasRetiredV2, position + 2))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasRetiredV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.Gehistoreerd);
                    document.LastChangedOn.Should().Be(addressWasRetiredV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRetiredV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasRetiredV2.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressWasRetiredV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedV2_ThenDocumentIsMarkedRemoved()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasRemovedV2 = _fixture.Create<AddressWasRemovedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasRemovedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasRemovedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeTrue();
                    document.LastChangedOn.Should().Be(addressWasRemovedV2.Provenance.Timestamp);

                    var feedItem = await context.AddressFeed.LastAsync(x => x.PersistentLocalId == addressWasRemovedV2.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, addressWasRemovedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.DeleteV1,
                            addressWasRemovedV2.AddressPersistentLocalId.ToString(),
                            addressWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressWasRemovedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasChangedV2_ThenPostalCodeIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPostalCodeWasChangedV2 = _fixture.Create<AddressPostalCodeWasChangedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressPostalCodeWasChangedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPostalCodeWasChangedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    document.LastChangedOn.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPostalCodeWasChangedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPostalCodeWasChangedV2.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressPostalCodeWasChangedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2_ThenHouseNumberIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressHouseNumberWasCorrectedV2 = _fixture.Create<AddressHouseNumberWasCorrectedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressHouseNumberWasCorrectedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressHouseNumberWasCorrectedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    document.LastChangedOn.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressBoxNumberWasCorrectedV2_ThenBoxNumberIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressBoxNumberWasCorrectedV2 = _fixture.Create<AddressBoxNumberWasCorrectedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressBoxNumberWasCorrectedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressBoxNumberWasCorrectedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.BoxNumber.Should().Be(addressBoxNumberWasCorrectedV2.BoxNumber);
                    document.LastChangedOn.Should().Be(addressBoxNumberWasCorrectedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated_ThenOfficiallyAssignedIsFalse()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasDeregulated, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasDeregulated.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.OfficiallyAssigned.Should().BeFalse();
                    document.LastChangedOn.Should().Be(addressWasDeregulated.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressWasRegularized_ThenOfficiallyAssignedIsTrue()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var addressWasRegularized = _fixture.Create<AddressWasRegularized>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasDeregulated, position + 1),
                    CreateEnvelope(addressWasRegularized, position + 2))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasRegularized.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.OfficiallyAssigned.Should().BeTrue();
                    document.LastChangedOn.Should().Be(addressWasRegularized.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenAddressRemovalWasCorrected_ThenDocumentIsRestored()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasRemovedV2 = _fixture.Create<AddressWasRemovedV2>();
            var addressRemovalWasCorrected = _fixture.Create<AddressRemovalWasCorrected>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasRemovedV2, position + 1),
                    CreateEnvelope(addressRemovalWasCorrected, position + 2))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressRemovalWasCorrected.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.Document.HouseNumber.Should().Be(addressRemovalWasCorrected.HouseNumber);
                    document.Document.BoxNumber.Should().Be(addressRemovalWasCorrected.BoxNumber);
                    document.Document.PostalCode.Should().Be(addressRemovalWasCorrected.PostalCode ?? string.Empty);
                    document.Document.OfficiallyAssigned.Should().Be(addressRemovalWasCorrected.OfficiallyAssigned);
                    document.LastChangedOn.Should().Be(addressRemovalWasCorrected.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressRemovalWasCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressRemovalWasCorrected.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            AddressRemovalWasCorrected.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged_ThenFeedItemIsAdded()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressPositionWasChanged, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPositionWasChanged.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(addressPositionWasChanged.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged_ThenAddressDocumentsAreUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var streetNameNamesWereChanged = new StreetNameNamesWereChanged(
                new StreetNamePersistentLocalId(addressWasProposedV2.StreetNamePersistentLocalId),
                new Dictionary<string, string> { { "nl", "Nieuwe Straatnaam" } },
                new List<AddressPersistentLocalId> { new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId) });
            ((ISetProvenance)streetNameNamesWereChanged).SetProvenance(_fixture.Create<Provenance>());

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(streetNameNamesWereChanged, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasProposedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent_ThenStatusIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = _fixture.Create<AddressWasApproved>();
            var addressWasRetiredV2 = _fixture.Create<AddressWasRetiredV2>();
            var addressWasCorrected = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasApproved, position + 1),
                    CreateEnvelope(addressWasRetiredV2, position + 2),
                    CreateEnvelope(addressWasCorrected, position + 3))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasCorrected.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.InGebruik);
                    document.LastChangedOn.Should().Be(addressWasCorrected.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrectedV2_ThenPostalCodeIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPostalCodeWasCorrectedV2 = _fixture.Create<AddressPostalCodeWasCorrectedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressPostalCodeWasCorrectedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    document.LastChangedOn.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        private static void AssertFeedItem(
            AddressFeedItem? feedItem,
            long position,
            IStreetNameEvent @event)
        {
            feedItem.Should().NotBeNull();
            feedItem!.CloudEventAsString.Should().NotBeNullOrEmpty();
            feedItem.Page.Should().Be(1);
            feedItem.Position.Should().Be(position);
            feedItem.Application.Should().Be(@event.Provenance.Application);
            feedItem.Modification.Should().Be(@event.Provenance.Modification);
            feedItem.Operator.Should().Be(@event.Provenance.Operator);
            feedItem.Organisation.Should().Be(@event.Provenance.Organisation);
            feedItem.Reason.Should().Be(@event.Provenance.Reason);
        }

        private Envelope<T> CreateEnvelope<T>(T @event, long position) where T : IMessage
        {
            var metadata = new Dictionary<string, object>
            {
                { "Position", position },
                { "EventName", @event.GetType().Name },
                { "CommandId", Guid.NewGuid().ToString() }
            };
            return new Envelope<T>(new Envelope(@event, metadata));
        }

        private void SetupChangeFeedServiceMock()
        {
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEventWithData(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new CloudEvent());

            ChangeFeedServiceMock.Setup(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>())).Returns("serialized cloud event");

            ChangeFeedServiceMock.Setup(x => x.CheckToUpdateCacheAsync(
                It.IsAny<int>(),
                It.IsAny<FeedContext>(),
                It.IsAny<Func<int, Task<int>>>()));
        }

        private FeedContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FeedContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FeedContext(options, new JsonSerializerSettings().ConfigureDefaultForApi());
        }
    }
}
