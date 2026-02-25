namespace AddressRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StreetNameId
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasMigrated.StreetNamePersistentLocalId.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue == null)
                                && attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasMigrated.HouseNumber)
                                && attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == (addressWasMigrated.PostalCode ?? string.Empty))
                                && attrs.Any(a => a.Name == AddressAttributeNames.OfficiallyAssigned
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasMigrated.OfficiallyAssigned.ToString())
                                && (string.IsNullOrEmpty(addressWasMigrated.BoxNumber)
                                    || attrs.Any(a => a.Name == AddressAttributeNames.BoxNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasMigrated.BoxNumber))),
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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StreetNameId
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedV2.StreetNamePersistentLocalId.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Voorgesteld))
                                && attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedV2.HouseNumber)
                                && attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedV2.PostalCode)
                                && attrs.Any(a => a.Name == AddressAttributeNames.OfficiallyAssigned
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == true.ToString())
                                && (string.IsNullOrEmpty(addressWasProposedV2.BoxNumber)
                                    || attrs.Any(a => a.Name == AddressAttributeNames.BoxNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedV2.BoxNumber))),
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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.InGebruik))),
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

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasCorrected.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Voorgesteld))),
                            AddressWasCorrectedFromApprovedToProposed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Afgekeurd))),
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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Gehistoreerd))),
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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs => attrs.Count == 0),
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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue!.ToString() == addressWasProposedV2.PostalCode
                                               && a.NewValue!.ToString() == addressPostalCodeWasChangedV2.PostalCode)),
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

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressHouseNumberWasCorrectedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressHouseNumberWasCorrectedV2.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue!.ToString() == addressWasProposedV2.HouseNumber
                                               && a.NewValue!.ToString() == addressHouseNumberWasCorrectedV2.HouseNumber)),
                            AddressHouseNumberWasCorrectedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

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

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressBoxNumberWasCorrectedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressBoxNumberWasCorrectedV2.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.BoxNumber
                                               && a.OldValue!.ToString() == addressWasProposedV2.BoxNumber
                                               && a.NewValue!.ToString() == addressBoxNumberWasCorrectedV2.BoxNumber)),
                            AddressBoxNumberWasCorrectedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

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

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasDeregulated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasDeregulated.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.OfficiallyAssigned
                                               && a.OldValue!.ToString() == true.ToString()
                                               && a.NewValue!.ToString() == false.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.InGebruik))),
                            AddressWasDeregulated.EventName,
                            It.IsAny<string>()),
                        Times.Once);

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

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRegularized.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasRegularized.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.OfficiallyAssigned
                                               && a.OldValue!.ToString() == false.ToString()
                                               && a.NewValue!.ToString() == true.ToString())),
                            AddressWasRegularized.EventName,
                            It.IsAny<string>()),
                        Times.Once);

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
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue == null)
                                && attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressRemovalWasCorrected.HouseNumber)
                                && attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == (addressRemovalWasCorrected.PostalCode ?? string.Empty))
                                && attrs.Any(a => a.Name == AddressAttributeNames.OfficiallyAssigned
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressRemovalWasCorrected.OfficiallyAssigned.ToString())
                                && (string.IsNullOrEmpty(addressRemovalWasCorrected.BoxNumber)
                                    || attrs.Any(a => a.Name == AddressAttributeNames.BoxNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressRemovalWasCorrected.BoxNumber))),
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

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPositionWasChanged.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPositionWasChanged.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedV2.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PositionGeometryMethod
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressPositionWasChanged.GeometryMethod.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionSpecification
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressPositionWasChanged.GeometrySpecification.ToString())),
                            AddressPositionWasChanged.EventName,
                            It.IsAny<string>()),
                        Times.Once);

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


        [Fact]
        public async Task WhenAddressWasProposedForMunicipalityMerger_ThenFeedItemAndDocumentAndTransformAreAdded()
        {
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedForMunicipalityMerger, position))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId);
                    document.Should().NotBeNull();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.CreateV1,
                            addressWasProposedForMunicipalityMerger.AddressPersistentLocalId.ToString(),
                            addressWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StreetNameId
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedForMunicipalityMerger.StreetNamePersistentLocalId.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue == null)
                                && attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedForMunicipalityMerger.HouseNumber)
                                && attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedForMunicipalityMerger.PostalCode)
                                && attrs.Any(a => a.Name == AddressAttributeNames.OfficiallyAssigned
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedForMunicipalityMerger.OfficiallyAssigned.ToString())
                                && (string.IsNullOrEmpty(addressWasProposedForMunicipalityMerger.BoxNumber)
                                    || attrs.Any(a => a.Name == AddressAttributeNames.BoxNumber
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == addressWasProposedForMunicipalityMerger.BoxNumber))),
                            AddressWasProposedForMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                            It.IsAny<long>(),
                            addressWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.TransformV1,
                            It.Is<AddressCloudTransformEvent>(t =>
                                t.From.Contains(addressWasProposedForMunicipalityMerger.MergedAddressPersistentLocalId.ToString())
                                && t.To.Contains(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId.ToString())),
                            It.IsAny<Uri>(),
                            AddressWasProposedForMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfMunicipalityMerger_ThenFeedItemIsAddedWithTransform()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();

            var streetNamePersistentLocalId = addressWasProposedV2.StreetNamePersistentLocalId;
            var addressPersistentLocalId = addressWasProposedV2.AddressPersistentLocalId;
            var newAddressPersistentLocalId = _fixture.Create<int>();

            var addressWasRejectedBecauseOfMunicipalityMerger = new AddressWasRejectedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId));
            ((ISetProvenance)addressWasRejectedBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasRejectedBecauseOfMunicipalityMerger, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.Afgekeurd);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(streetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.Voorgesteld)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Afgekeurd))),
                            AddressWasRejectedBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                            It.IsAny<long>(),
                            addressWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.TransformV1,
                            It.Is<AddressCloudTransformEvent>(t =>
                                t.From.Contains(addressPersistentLocalId.ToString())
                                && t.To.Contains(newAddressPersistentLocalId.ToString())),
                            It.IsAny<Uri>(),
                            AddressWasRejectedBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(3));
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfMunicipalityMerger_ThenFeedItemIsAddedWithTransform()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = _fixture.Create<AddressWasApproved>();

            var streetNamePersistentLocalId = addressWasProposedV2.StreetNamePersistentLocalId;
            var addressPersistentLocalId = addressWasProposedV2.AddressPersistentLocalId;
            var newAddressPersistentLocalId = _fixture.Create<int>();

            var addressWasRetiredBecauseOfMunicipalityMerger = new AddressWasRetiredBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId));
            ((ISetProvenance)addressWasRetiredBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasApproved, position + 1),
                    CreateEnvelope(addressWasRetiredBecauseOfMunicipalityMerger, position + 2))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(AdresStatus.Gehistoreerd);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(streetNamePersistentLocalId.ToString())),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(AdresStatus.InGebruik)
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Gehistoreerd))),
                            AddressWasRetiredBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                            It.IsAny<long>(),
                            addressWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.TransformV1,
                            It.Is<AddressCloudTransformEvent>(t =>
                                t.From.Contains(addressPersistentLocalId.ToString())
                                && t.To.Contains(newAddressPersistentLocalId.ToString())),
                            It.IsAny<Uri>(),
                            AddressWasRetiredBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(4));
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

            ChangeFeedServiceMock.Setup(x => x.CreateCloudEvent(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<AddressCloudTransformEvent>(),
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new CloudEvent());

            ChangeFeedServiceMock.Setup(x => x.DataSchemaUriTransform).Returns(new Uri("https://test.com"));

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
