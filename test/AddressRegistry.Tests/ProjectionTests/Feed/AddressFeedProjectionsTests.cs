namespace AddressRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.StreetName;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.DataStructures;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using CloudNative.CloudEvents;
    using EventExtensions;
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
        private const string NisCode = "11001";
        private readonly Fixture _fixture;
        private readonly FeedContext _feedContext;
        private readonly FakeStreetNameConsumerContext _streetNameConsumerContext;

        private ConnectedProjectionTest<FeedContext, AddressFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }

        public AddressFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            _feedContext = CreateContext();
            _streetNameConsumerContext = CreateConsumerContext();
            var mockStreetNameFactory = new Mock<IDbContextFactory<StreetNameConsumerContext>>();
            mockStreetNameFactory.Setup(f => f
                    .CreateDbContextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_streetNameConsumerContext);

            Sut = new ConnectedProjectionTest<FeedContext, AddressFeedProjections>(() => _feedContext,
                () => new AddressFeedProjections(ChangeFeedServiceMock.Object, mockStreetNameFactory.Object));

            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customizations.Insert(0, new WithoutUnknownStreetNameAddressStatus());

            _streetNameConsumerContext.StreetNameLatestItems.Add(
                new StreetNameLatestItem(_fixture.Create<StreetNamePersistentLocalId>(), NisCode));
            _streetNameConsumerContext.SaveChanges();

            SetupChangeFeedServiceMock();
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName_ThenFeedItemAndDocumentAreAdded()
        {
            _fixture.Register(() => AddressStatus.Proposed);
            _fixture.Register(() => GeometryMethod.AppointedByAdministrator);
            _fixture.Register(() => GeometrySpecification.Building);

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
                    document.Document.Status.Should().Be(AdresStatus.Voorgesteld);
                    document.Document.PositionGeometryMethod.Should().Be(PositieGeometrieMethode.AangeduidDoorBeheerder);
                    document.Document.PositionSpecification.Should().Be(PositieSpecificatie.Gebouw);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(addressWasMigrated.ExtendedWkbGeometry);

                    var feedItem = await FindFeedItemByAddressPersistentLocalId(context, addressWasMigrated.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position, addressWasMigrated);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.CreateV1,
                            addressWasMigrated.AddressPersistentLocalId.ToString(),
                            addressWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                                               && a.NewValue!.ToString() == addressWasMigrated.BoxNumber))
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionGeometryMethod
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieGeometrieMethode.AangeduidDoorBeheerder.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionSpecification
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieSpecificatie.Gebouw.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.Position
                                               && a.OldValue == null
                                               && a.NewValue != null && AssertPositionList((List<AddressPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
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
            _fixture.Register(() => GeometryMethod.DerivedFromObject);
            _fixture.Register(() => GeometrySpecification.BuildingUnit);
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
                    document.Document.PositionGeometryMethod.Should().Be(PositieGeometrieMethode.AfgeleidVanObject);
                    document.Document.PositionSpecification.Should().Be(PositieSpecificatie.Gebouweenheid);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(addressWasProposedV2.ExtendedWkbGeometry);

                    var feedItem = await FindFeedItemByAddressPersistentLocalId(context, addressWasProposedV2.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position, addressWasProposedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.CreateV1,
                            addressWasProposedV2.AddressPersistentLocalId.ToString(),
                            addressWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                                               && a.NewValue!.ToString() == addressWasProposedV2.BoxNumber))
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionGeometryMethod
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieGeometrieMethode.AfgeleidVanObject.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionSpecification
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieSpecificatie.Gebouweenheid.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.Position
                                               && a.OldValue == null
                                               && a.NewValue != null && AssertPositionList((List<AddressPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
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

                    var feedItem = await FindLastFeedItemByAddressPersistentLocalId(context, addressWasApproved.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, addressWasApproved);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasApproved.AddressPersistentLocalId.ToString(),
                            addressWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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

                    var feedItem = await FindLastFeedItemByAddressPersistentLocalId(context, addressWasRejected.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, addressWasRejected);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressWasRejected.AddressPersistentLocalId.ToString(),
                            addressWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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

                    var feedItem = await FindLastFeedItemByAddressPersistentLocalId(context, addressWasRemovedV2.AddressPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, addressWasRemovedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.DeleteV1,
                            addressWasRemovedV2.AddressPersistentLocalId.ToString(),
                            addressWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
            var addressBoxWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId + 1))
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithHouseNumber(new HouseNumber(addressWasProposedV2.HouseNumber))
                .WithBoxNumber(new BoxNumber("1A"))
                .WithPostalCode(new PostalCode(addressWasProposedV2.PostalCode));
            var addressPostalCodeWasChangedV2 = _fixture.Create<AddressPostalCodeWasChangedV2>()
                .WithBoxNumberPersistentLocalIds([new AddressPersistentLocalId(addressBoxWasProposedV2.AddressPersistentLocalId)]);

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressBoxWasProposedV2, position + 1),
                    CreateEnvelope(addressPostalCodeWasChangedV2, position + 2))
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue!.ToString() == addressWasProposedV2.PostalCode
                                               && a.NewValue!.ToString() == addressPostalCodeWasChangedV2.PostalCode)),
                            AddressPostalCodeWasChangedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    var boxDocument = await context.AddressDocuments.FindAsync(addressPostalCodeWasChangedV2.BoxNumberPersistentLocalIds.First());
                    boxDocument.Should().NotBeNull();
                    boxDocument!.Document.PostalCode.Should().Be(addressPostalCodeWasChangedV2.PostalCode);
                    boxDocument.LastChangedOn.Should().Be(addressPostalCodeWasChangedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPostalCodeWasChangedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPostalCodeWasChangedV2.BoxNumberPersistentLocalIds.First().ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue!.ToString() == addressBoxWasProposedV2.PostalCode
                                               && a.NewValue!.ToString() == addressPostalCodeWasChangedV2.PostalCode)),
                            AddressPostalCodeWasChangedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2_ThenHouseNumberIsUpdated()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressBoxWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId + 1))
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithHouseNumber(new HouseNumber(addressWasProposedV2.HouseNumber))
                .WithBoxNumber(new BoxNumber("1A"));
            var addressHouseNumberWasCorrectedV2 = _fixture.Create<AddressHouseNumberWasCorrectedV2>()
                .WithBoxNumberPersistentLocalIds([new AddressPersistentLocalId(addressBoxWasProposedV2.AddressPersistentLocalId)]);

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressBoxWasProposedV2, position + 1),
                    CreateEnvelope(addressHouseNumberWasCorrectedV2, position + 2))
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue!.ToString() == addressWasProposedV2.HouseNumber
                                               && a.NewValue!.ToString() == addressHouseNumberWasCorrectedV2.HouseNumber)),
                            AddressHouseNumberWasCorrectedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    var boxDocument = await context.AddressDocuments.FindAsync(addressHouseNumberWasCorrectedV2.BoxNumberPersistentLocalIds.First());
                    boxDocument.Should().NotBeNull();
                    boxDocument!.Document.HouseNumber.Should().Be(addressHouseNumberWasCorrectedV2.HouseNumber);
                    boxDocument.LastChangedOn.Should().Be(addressHouseNumberWasCorrectedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressHouseNumberWasCorrectedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressHouseNumberWasCorrectedV2.BoxNumberPersistentLocalIds.First().ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.HouseNumber
                                               && a.OldValue!.ToString() == addressBoxWasProposedV2.HouseNumber
                                               && a.NewValue!.ToString() == addressHouseNumberWasCorrectedV2.HouseNumber)),
                            AddressHouseNumberWasCorrectedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
            _fixture.Register(() => AddressStatus.Retired);
            _fixture.Register(() => GeometryMethod.AppointedByAdministrator);
            _fixture.Register(() => GeometrySpecification.BuildingUnit);
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
                    document.Document.ExtendedWkbGeometry.Should().Be(addressRemovalWasCorrected.ExtendedWkbGeometry);
                    document.LastChangedOn.Should().Be(addressRemovalWasCorrected.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressRemovalWasCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.CreateV1,
                            addressRemovalWasCorrected.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == nameof(AdresStatus.Gehistoreerd))
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
                                               && a.NewValue!.ToString() == addressRemovalWasCorrected.BoxNumber))
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionGeometryMethod
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieGeometrieMethode.AangeduidDoorBeheerder.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionSpecification
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieSpecificatie.Gebouweenheid.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.Position
                                               && a.OldValue == null
                                               && a.NewValue != null && AssertPositionList((List<AddressPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
                            AddressRemovalWasCorrected.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(3));
                });
        }

        [Theory]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Ingang)]
        [InlineData(GeometryMethod.DerivedFromObject, GeometrySpecification.Municipality, PositieGeometrieMethode.AfgeleidVanObject, PositieSpecificatie.Gemeente)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Perceel)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Street, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Straat)]
        [InlineData(GeometryMethod.Interpolated, GeometrySpecification.Lot, PositieGeometrieMethode.Geinterpoleerd, PositieSpecificatie.Lot)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Standplaats)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Berth, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Ligplaats)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Building, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Gebouw)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Gebouweenheid)]
        [InlineData(GeometryMethod.AppointedByAdministrator, GeometrySpecification.RoadSegment, PositieGeometrieMethode.AangeduidDoorBeheerder, PositieSpecificatie.Wegsegment)]
        public async Task WhenAddressPositionWasChanged_ThenFeedItemIsAdded(
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            PositieGeometrieMethode positieGeometrieMethode,
            PositieSpecificatie positieSpecificatie)
        {
            //Make sure value changes
            _fixture.Register(() => geometryMethod == GeometryMethod.AppointedByAdministrator ? GeometryMethod.DerivedFromObject : GeometryMethod.AppointedByAdministrator);
            _fixture.Register(() => geometrySpecification == GeometrySpecification.Lot ? GeometrySpecification.Parcel : GeometrySpecification.Lot);
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            _fixture.Register(() => geometryMethod);
            _fixture.Register(() => geometrySpecification);
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
                    document.Document.PositionGeometryMethod.Should().Be(positieGeometrieMethode);
                    document.Document.PositionSpecification.Should().Be(positieSpecificatie);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(addressPositionWasChanged.ExtendedWkbGeometry);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPositionWasChanged.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPositionWasChanged.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PositionGeometryMethod
                                    && a.OldValue != null && a.NewValue != null
                                    && a.NewValue.ToString() == positieGeometrieMethode.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionSpecification
                                   && a.OldValue != null && a.NewValue != null
                                   && a.NewValue.ToString() == positieSpecificatie.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.Position
                                               && a.OldValue != null && ((List<AddressPositionCloudEventValue>)a.OldValue).Count == 2
                                               && a.NewValue != null && AssertPositionList((List<AddressPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
                            AddressPositionWasChanged.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrectedV2_ThenFeedItemIsAdded()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            _fixture.Register(() => GeometryMethod.DerivedFromObject);
            _fixture.Register(() =>GeometrySpecification.BuildingUnit);
            var addressPositionWasCorrectedV2 = _fixture.Create<AddressPositionWasCorrectedV2>();

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressPositionWasCorrectedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPositionWasCorrectedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(addressPositionWasCorrectedV2.Provenance.Timestamp);
                    document.Document.PositionGeometryMethod.Should().Be(PositieGeometrieMethode.AfgeleidVanObject);
                    document.Document.PositionSpecification.Should().Be(PositieSpecificatie.Gebouweenheid);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(addressPositionWasCorrectedV2.ExtendedWkbGeometry);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPositionWasCorrectedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPositionWasCorrectedV2.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.Position
                                               && a.OldValue != null && ((List<AddressPositionCloudEventValue>)a.OldValue).Count == 2
                                               && a.NewValue != null && AssertPositionList((List<AddressPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
                            AddressPositionWasCorrectedV2.EventName,
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
            var addressBoxWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId + 1))
                .WithParentAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId))
                .WithHouseNumber(new HouseNumber(addressWasProposedV2.HouseNumber))
                .WithBoxNumber(new BoxNumber("1A"))
                .WithPostalCode(new PostalCode(addressWasProposedV2.PostalCode));
            var addressPostalCodeWasCorrectedV2 = _fixture.Create<AddressPostalCodeWasCorrectedV2>()
                .WithBoxNumberPersistentLocalIds([new AddressPersistentLocalId(addressBoxWasProposedV2.AddressPersistentLocalId)]);

            var position = 1L;

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressBoxWasProposedV2, position + 1),
                    CreateEnvelope(addressPostalCodeWasCorrectedV2, position + 2))
                .Then(async context =>
                {
                    var document = await context.AddressDocuments.FindAsync(addressPostalCodeWasCorrectedV2.AddressPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    document.LastChangedOn.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPostalCodeWasCorrectedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPostalCodeWasCorrectedV2.AddressPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue!.ToString() == addressWasProposedV2.PostalCode
                                               && a.NewValue!.ToString() == addressPostalCodeWasCorrectedV2.PostalCode)),
                            AddressPostalCodeWasCorrectedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    var boxDocument = await context.AddressDocuments.FindAsync(addressPostalCodeWasCorrectedV2.BoxNumberPersistentLocalIds.First());
                    boxDocument.Should().NotBeNull();
                    boxDocument!.Document.PostalCode.Should().Be(addressPostalCodeWasCorrectedV2.PostalCode);
                    boxDocument.LastChangedOn.Should().Be(addressPostalCodeWasCorrectedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            addressPostalCodeWasCorrectedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            addressPostalCodeWasCorrectedV2.BoxNumberPersistentLocalIds.First().ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == AddressAttributeNames.PostalCode
                                               && a.OldValue!.ToString() == addressBoxWasProposedV2.PostalCode
                                               && a.NewValue!.ToString() == addressPostalCodeWasCorrectedV2.PostalCode)),
                            AddressPostalCodeWasCorrectedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                });
        }


        [Fact]
        public async Task WhenAddressWasProposedForMunicipalityMerger_ThenFeedItemAndDocumentAndTransformAreAdded()
        {
            _fixture.Register(() => GeometryMethod.DerivedFromObject);
            _fixture.Register(() => GeometrySpecification.Municipality);
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                                               && a.NewValue!.ToString() == addressWasProposedForMunicipalityMerger.BoxNumber))
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionGeometryMethod
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieGeometrieMethode.AfgeleidVanObject.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.PositionSpecification
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == PositieSpecificatie.Gemeente.ToString())
                                && attrs.Any(a => a.Name == AddressAttributeNames.Position
                                               && a.OldValue == null
                                               && a.NewValue != null && AssertPositionList((List<AddressPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
                            AddressWasProposedForMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(1));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(1));
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfMunicipalityMerger_ThenFeedItemIsAddedWithTransform()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var streetNamePersistentLocalId = addressWasProposedV2.StreetNamePersistentLocalId;
            var addressPersistentLocalId = addressWasProposedV2.AddressPersistentLocalId;
            var newAddressPersistentLocalId = _fixture.Create<int>();

            var newNisCode = "11002";
            var newStreetNamePersistentLocalId = _fixture.Create<int>();
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .WithStreetNamePersistentLocalId(new StreetNamePersistentLocalId(newStreetNamePersistentLocalId))
                .WithAddressPersistentLocalId(newAddressPersistentLocalId)
                .WithMergedAddressPersistentLocalId(new AddressPersistentLocalId(addressPersistentLocalId));

            _streetNameConsumerContext.StreetNameLatestItems.Add(new StreetNameLatestItem(newStreetNamePersistentLocalId, newNisCode));
            await _streetNameConsumerContext.SaveChangesAsync();

            var addressWasRejectedBecauseOfMunicipalityMerger = new AddressWasRejectedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId));
            ((ISetProvenance)addressWasRejectedBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            var position = 1L;

            var callOrder = new List<string>();
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEvent(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<AddressCloudTransformEvent>(),
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Callback(() => callOrder.Add("Transform"))
                .Returns(new CloudEvent());
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
                .Callback(() => callOrder.Add("Update"))
                .Returns(new CloudEvent());

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasProposedForMunicipalityMerger, position + 1),
                    CreateEnvelope(addressWasRejectedBecauseOfMunicipalityMerger, position + 2))
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                                t.NisCodes.Contains(NisCode) && t.NisCodes.Contains(newNisCode)
                                && t.TransformValues.First().From.Contains(addressPersistentLocalId.ToString())
                                && t.TransformValues.First().To.Contains(newAddressPersistentLocalId.ToString())),
                            It.IsAny<Uri>(),
                            AddressWasRejectedBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    // The last two calls should be Transform then Update (for the rejected event)
                    callOrder.TakeLast(2).Should().ContainInOrder("Transform", "Update");

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(4));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(4));
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

            var newNisCode = "11002";
            var newStreetNamePersistentLocalId = _fixture.Create<int>();
            var addressWasProposedForMunicipalityMerger = _fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .WithStreetNamePersistentLocalId(new StreetNamePersistentLocalId(newStreetNamePersistentLocalId))
                .WithAddressPersistentLocalId(newAddressPersistentLocalId)
                .WithMergedAddressPersistentLocalId(new AddressPersistentLocalId(addressPersistentLocalId));

            _streetNameConsumerContext.StreetNameLatestItems.Add(new StreetNameLatestItem(newStreetNamePersistentLocalId, newNisCode));
            await _streetNameConsumerContext.SaveChangesAsync();

            var addressWasRetiredBecauseOfMunicipalityMerger = new AddressWasRetiredBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId));
            ((ISetProvenance)addressWasRetiredBecauseOfMunicipalityMerger).SetProvenance(_fixture.Create<Provenance>());

            var position = 1L;

            var callOrder = new List<string>();
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEvent(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<AddressCloudTransformEvent>(),
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Callback(() => callOrder.Add("Transform"))
                .Returns(new CloudEvent());
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
                .Callback(() => callOrder.Add("Update"))
                .Returns(new CloudEvent());

            await Sut
                .Given(CreateEnvelope(addressWasProposedV2, position),
                    CreateEnvelope(addressWasApproved, position + 1),
                    CreateEnvelope(addressWasProposedForMunicipalityMerger, position + 2),
                    CreateEnvelope(addressWasRetiredBecauseOfMunicipalityMerger, position + 3))
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
                            It.Is<List<string>>(l => l.Contains(NisCode)),
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
                                t.NisCodes.Contains(NisCode) && t.NisCodes.Contains(newNisCode)
                                    && t.TransformValues.First().From.Contains(addressPersistentLocalId.ToString())
                                    && t.TransformValues.First().To.Contains(newAddressPersistentLocalId.ToString())),
                            It.IsAny<Uri>(),
                            AddressWasRetiredBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    // The last two calls should be Transform then Update (for the retired event)
                    callOrder.TakeLast(2).Should().ContainInOrder("Transform", "Update");

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(5));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(5));
                });
        }

        [Fact]
        public async Task WhenStreetNameWasReaddressed_ThenTransformAndDocumentsAreUpdated()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var sourceHouseNumberId = 100;
            var destinationHouseNumberId = 200;
            var sourceBoxNumberId = 101;
            var destinationBoxNumberId = 201;

            // Create source documents first via AddressWasProposedV2
            var sourceHouseProposed = _fixture.Create<AddressWasProposedV2>()
                .WithStreetNamePersistentLocalId(streetNamePersistentLocalId)
                .WithAddressPersistentLocalId(sourceHouseNumberId);

            var destHouseProposed = _fixture.Create<AddressWasProposedV2>()
                .WithStreetNamePersistentLocalId(streetNamePersistentLocalId)
                .WithAddressPersistentLocalId(destinationHouseNumberId);

            var destBoxProposed = _fixture.Create<AddressWasProposedV2>()
                .WithStreetNamePersistentLocalId(streetNamePersistentLocalId)
                .WithAddressPersistentLocalId(destinationBoxNumberId);

            var readdressedHouseNumber = new ReaddressedAddressData(
                new AddressPersistentLocalId(sourceHouseNumberId),
                new AddressPersistentLocalId(destinationHouseNumberId),
                isDestinationNewlyProposed: false,
                AddressStatus.Current,
                new HouseNumber("1"),
                boxNumber: null,
                new PostalCode("9000"),
                new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    new ExtendedWkbGeometry(sourceHouseProposed.ExtendedWkbGeometry)),
                sourceIsOfficiallyAssigned: true);

            var readdressedBoxNumber = new ReaddressedAddressData(
                new AddressPersistentLocalId(sourceBoxNumberId),
                new AddressPersistentLocalId(destinationBoxNumberId),
                isDestinationNewlyProposed: false,
                AddressStatus.Current,
                new HouseNumber("1"),
                new BoxNumber("A"),
                new PostalCode("9000"),
                new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    new ExtendedWkbGeometry(destBoxProposed.ExtendedWkbGeometry)),
                sourceIsOfficiallyAssigned: true);

            var addressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(destinationHouseNumberId),
                readdressedHouseNumber,
                [readdressedBoxNumber]);
            ((ISetProvenance)addressHouseNumberWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var streetNameWasReaddressed = new StreetNameWasReaddressed(
                streetNamePersistentLocalId,
                [addressHouseNumberWasReaddressed]);
            ((ISetProvenance)streetNameWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var position = 1L;

            var callOrder = new List<string>();
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEvent(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<AddressCloudTransformEvent>(),
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Callback(() => callOrder.Add("Transform"))
                .Returns(new CloudEvent());
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
                .Callback(() => callOrder.Add("Update"))
                .Returns(new CloudEvent());

            await Sut
                .Given(
                    CreateEnvelope(sourceHouseProposed, position),
                    CreateEnvelope(destHouseProposed, position + 1),
                    CreateEnvelope(destBoxProposed, position + 2),
                    CreateEnvelope(streetNameWasReaddressed, position + 3))
                .Then(async context =>
                {
                    // Verify transform event was created with correct values
                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEvent(
                            It.IsAny<long>(),
                            streetNameWasReaddressed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.TransformV1,
                            It.Is<AddressCloudTransformEvent>(t =>
                                t.NisCodes.Contains(NisCode)
                                && t.TransformValues.Any(v =>
                                    v.From == sourceHouseNumberId.ToString()
                                    && v.To == destinationHouseNumberId.ToString())
                                && t.TransformValues.Any(v =>
                                    v.From == sourceBoxNumberId.ToString()
                                    && v.To == destinationBoxNumberId.ToString())),
                            It.IsAny<Uri>(),
                            StreetNameWasReaddressed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    // Verify update cloud events for house number and box number
                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasReaddressed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            destinationHouseNumberId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            StreetNameWasReaddressed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            streetNameWasReaddressed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            AddressEventTypes.UpdateV1,
                            destinationBoxNumberId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(l => l.Contains(NisCode)),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            StreetNameWasReaddressed.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    // Verify transform happens before update for the readdressed event
                    // The first 3 calls are "Update" from the 3 AddressWasProposedV2 creates,
                    // then the last 3 should be Transform, Update, Update
                    callOrder.Skip(3).First().Should().Be("Transform");

                    // Verify the transform feed item is linked to both destination addresses
                    var transformFeedItemAddresses = await context.AddressFeedItemAddresses
                        .Where(x => x.AddressPersistentLocalId == destinationHouseNumberId
                                    || x.AddressPersistentLocalId == destinationBoxNumberId)
                        .ToListAsync();

                    var transformFeedItemId = transformFeedItemAddresses
                        .GroupBy(x => x.FeedItemId)
                        .Where(g => g.Count() == 2)
                        .Select(g => g.Key)
                        .FirstOrDefault();

                    transformFeedItemId.Should().NotBe(0, "transform feed item should be linked to both destination addresses");

                    // 3 create events + 1 transform + 2 update = 6 serialize calls
                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(6));
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Exactly(6));
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

        private static async Task<AddressFeedItem?> FindFeedItemByAddressPersistentLocalId(FeedContext context, int addressPersistentLocalId)
        {
            var feedItemId = await context.AddressFeedItemAddresses
                .Where(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                .Select(x => x.FeedItemId)
                .SingleOrDefaultAsync();

            return await context.AddressFeed.SingleOrDefaultAsync(x => x.Id == feedItemId);
        }

        private static async Task<AddressFeedItem> FindLastFeedItemByAddressPersistentLocalId(FeedContext context, int addressPersistentLocalId)
        {
            var feedItemIds = await context.AddressFeedItemAddresses
                .Where(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                .Select(x => x.FeedItemId)
                .ToListAsync();

            return await context.AddressFeed
                .Where(x => feedItemIds.Contains(x.Id))
                .OrderBy(x => x.Id)
                .LastAsync();
        }

        private static bool AssertPositionList(List<AddressPositionCloudEventValue> positionList, string gml)
        {
            var lambert72 = positionList.SingleOrDefault(p => p.Projection == SystemReferenceId.SrsNameLambert72);
            lambert72.Should().NotBeNull();

            var lambert08 = positionList.SingleOrDefault(p => p.Projection == SystemReferenceId.SrsNameLambert2008);
            lambert08.Should().NotBeNull();

            positionList.Count.Should().Be(2);
            positionList.Should().Contain(p => p.Gml == gml);

            return true;
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

        private FakeStreetNameConsumerContext CreateConsumerContext()
        {
            var options = new DbContextOptionsBuilder<StreetNameConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FakeStreetNameConsumerContext(options);
        }
    }
}
