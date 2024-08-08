namespace AddressRegistry.Tests.ProjectionTests.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.Postal;
    using AddressRegistry.Consumer.Read.Postal.Projections;
    using AddressRegistry.Consumer.Read.StreetName;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using EventExtensions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Projections.Elastic;
    using Projections.Elastic.AddressSearch;
    using Tests.BackOffice.Infrastructure;
    using Xunit;

    public class AddressSearchProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly ConnectedProjectionTest<ElasticRunnerContext, AddressSearchProjections> _sut;

        private readonly Mock<IAddressElasticsearchClient> _elasticSearchClient;
        private readonly TestMunicipalityConsumerContext _municipalityContext;
        private readonly FakePostalConsumerContext _postalConsumerContext;
        private readonly TestStreetNameConsumerContext _streetNameConsumerContext;
        private readonly Mock<IDbContextFactory<MunicipalityConsumerContext>> _municipalityDbContextFactory;
        private readonly Mock<IDbContextFactory<PostalConsumerContext>> _postalDbContextFactory;
        private readonly Mock<IDbContextFactory<StreetNameConsumerContext>> _streetNameDbContextFactory;

        public AddressSearchProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithValidHouseNumber());

            _elasticSearchClient = new Mock<IAddressElasticsearchClient>();
            _postalConsumerContext = new FakePostalConsumerContextFactory().CreateDbContext();
            _municipalityContext = new FakeMunicipalityConsumerContextFactory().CreateDbContext();
            _streetNameConsumerContext = new FakeStreetNameConsumerContextFactory().CreateDbContext();
            _municipalityDbContextFactory = new Mock<IDbContextFactory<MunicipalityConsumerContext>>();
            _municipalityDbContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_municipalityContext);
            _postalDbContextFactory = new Mock<IDbContextFactory<PostalConsumerContext>>();
            _postalDbContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_postalConsumerContext);
            _streetNameDbContextFactory = new Mock<IDbContextFactory<StreetNameConsumerContext>>();
            _streetNameDbContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_streetNameConsumerContext);

            _sut = new ConnectedProjectionTest<ElasticRunnerContext, AddressSearchProjections>(CreateContext, CreateProjection);
        }

        private ElasticRunnerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ElasticRunnerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElasticRunnerContext(options);
        }

        private AddressSearchProjections CreateProjection() => new(
            _elasticSearchClient.Object,
            _municipalityDbContextFactory.Object,
            _postalDbContextFactory.Object,
            _streetNameDbContextFactory.Object);

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            var @event = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithPosition(new ExtendedWkbGeometry(GeometryHelpers.ExampleExtendedWkb));
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var streetNameLatestItem = new StreetNameLatestItem
            {
                PersistentLocalId = @event.StreetNamePersistentLocalId,
                NisCode = "44021",
                NameDutch = "Bosstraat",
                NameFrench = "Rue Forestière",
                HomonymAdditionDutch = "MA",
                HomonymAdditionFrench = "AM"
            };
            _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
            await _streetNameConsumerContext.SaveChangesAsync();

            var municipalityLatestItem = new MunicipalityLatestItem
            {
                NisCode = streetNameLatestItem.NisCode,
                NameDutch = "Gent",
                NameFrench = "Gand"
            };
            _municipalityContext.MunicipalityLatestItems.Add(municipalityLatestItem);
            await _municipalityContext.SaveChangesAsync();

            var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = @event.PostalCode!,
                PostalNames = new List<PostalInfoPostalName>
                {
                    postalInfoPostalName
                }
            });
            await _postalConsumerContext.SaveChangesAsync();

            await _sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.CreateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.ParentAddressPersistentLocalId == @event.ParentPersistentLocalId
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == @event.Status
                            && doc.OfficiallyAssigned == @event.OfficiallyAssigned
                            && doc.HouseNumber == @event.HouseNumber
                            && doc.BoxNumber == @event.BoxNumber
                            && doc.Municipality.NisCode == municipalityLatestItem.NisCode
                            && doc.Municipality.Names.Length == 2
                            && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
                            && doc.StreetName.Names.Length == 2
                            && doc.StreetName.HomonymAdditions.Length == 2
                            && doc.FullAddress.Length == 2
                            && doc.AddressPosition.GeometryMethod == @event.GeometryMethod
                            && doc.AddressPosition.GeometrySpecification == @event.GeometrySpecification
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedV2()
        {
            var @event = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(new ExtendedWkbGeometry(GeometryHelpers.ExampleExtendedWkb));
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var streetNameLatestItem = new StreetNameLatestItem
            {
                PersistentLocalId = @event.StreetNamePersistentLocalId,
                NisCode = "44021",
                NameDutch = "Bosstraat",
                NameFrench = "Rue Forestière",
                HomonymAdditionDutch = "MA",
                HomonymAdditionFrench = "AM"
            };
            _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
            await _streetNameConsumerContext.SaveChangesAsync();

            var municipalityLatestItem = new MunicipalityLatestItem
            {
                NisCode = streetNameLatestItem.NisCode,
                NameDutch = "Gent",
                NameFrench = "Gand"
            };
            _municipalityContext.MunicipalityLatestItems.Add(municipalityLatestItem);
            await _municipalityContext.SaveChangesAsync();

            var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = @event.PostalCode,
                PostalNames = new List<PostalInfoPostalName>
                {
                    postalInfoPostalName
                }
            });
            await _postalConsumerContext.SaveChangesAsync();

            await _sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.CreateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.ParentAddressPersistentLocalId == @event.ParentPersistentLocalId
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == AddressStatus.Proposed
                            && doc.OfficiallyAssigned == true
                            && doc.HouseNumber == @event.HouseNumber
                            && doc.BoxNumber == @event.BoxNumber
                            && doc.Municipality.NisCode == municipalityLatestItem.NisCode
                            && doc.Municipality.Names.Length == 2
                            && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
                            && doc.StreetName.Names.Length == 2
                            && doc.StreetName.HomonymAdditions.Length == 2
                            && doc.FullAddress.Length == 2
                            && doc.AddressPosition.GeometryMethod == @event.GeometryMethod
                            && doc.AddressPosition.GeometrySpecification == @event.GeometrySpecification
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedBecauseOfReaddress()
        {
            var @event = _fixture.Create<AddressWasProposedBecauseOfReaddress>()
                .WithExtendedWkbGeometry(new ExtendedWkbGeometry(GeometryHelpers.ExampleExtendedWkb));
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var streetNameLatestItem = new StreetNameLatestItem
            {
                PersistentLocalId = @event.StreetNamePersistentLocalId,
                NisCode = "44021",
                NameDutch = "Bosstraat",
                NameFrench = "Rue Forestière",
                HomonymAdditionDutch = "MA",
                HomonymAdditionFrench = "AM"
            };
            _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
            await _streetNameConsumerContext.SaveChangesAsync();

            var municipalityLatestItem = new MunicipalityLatestItem
            {
                NisCode = streetNameLatestItem.NisCode,
                NameDutch = "Gent",
                NameFrench = "Gand"
            };
            _municipalityContext.MunicipalityLatestItems.Add(municipalityLatestItem);
            await _municipalityContext.SaveChangesAsync();

            var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = @event.PostalCode,
                PostalNames = new List<PostalInfoPostalName>
                {
                    postalInfoPostalName
                }
            });
            await _postalConsumerContext.SaveChangesAsync();

            await _sut
                .Given(new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.CreateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.ParentAddressPersistentLocalId == @event.ParentPersistentLocalId
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == AddressStatus.Proposed
                            && doc.OfficiallyAssigned == true
                            && doc.HouseNumber == @event.HouseNumber
                            && doc.BoxNumber == @event.BoxNumber
                            && doc.Municipality.NisCode == municipalityLatestItem.NisCode
                            && doc.Municipality.Names.Length == 2
                            && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
                            && doc.StreetName.Names.Length == 2
                            && doc.StreetName.HomonymAdditions.Length == 2
                            && doc.FullAddress.Length == 2
                            && doc.AddressPosition.GeometryMethod == @event.GeometryMethod
                            && doc.AddressPosition.GeometrySpecification == @event.GeometrySpecification
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedForMunicipalityMerger()
        {
            var @event = _fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .WithExtendedWkbGeometry(new ExtendedWkbGeometry(GeometryHelpers.ExampleExtendedWkb));
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var streetNameLatestItem = new StreetNameLatestItem
            {
                PersistentLocalId = @event.StreetNamePersistentLocalId,
                NisCode = "44021",
                NameDutch = "Bosstraat",
                NameFrench = "Rue Forestière",
                HomonymAdditionDutch = "MA",
                HomonymAdditionFrench = "AM"
            };
            _streetNameConsumerContext.StreetNameLatestItems.Add(streetNameLatestItem);
            await _streetNameConsumerContext.SaveChangesAsync();

            var municipalityLatestItem = new MunicipalityLatestItem
            {
                NisCode = streetNameLatestItem.NisCode,
                NameDutch = "Gent",
                NameFrench = "Gand"
            };
            _municipalityContext.MunicipalityLatestItems.Add(municipalityLatestItem);
            await _municipalityContext.SaveChangesAsync();

            var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = @event.PostalCode,
                PostalNames = new List<PostalInfoPostalName>
                {
                    postalInfoPostalName
                }
            });
            await _postalConsumerContext.SaveChangesAsync();

            await _sut
                .Given(new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.CreateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.ParentAddressPersistentLocalId == @event.ParentPersistentLocalId
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == AddressStatus.Proposed
                            && doc.OfficiallyAssigned == @event.OfficiallyAssigned
                            && doc.HouseNumber == @event.HouseNumber
                            && doc.BoxNumber == @event.BoxNumber
                            && doc.Municipality.NisCode == municipalityLatestItem.NisCode
                            && doc.Municipality.Names.Length == 2
                            && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
                            && doc.StreetName.Names.Length == 2
                            && doc.StreetName.HomonymAdditions.Length == 2
                            && doc.FullAddress.Length == 2
                            && doc.AddressPosition.GeometryMethod == @event.GeometryMethod
                            && doc.AddressPosition.GeometrySpecification == @event.GeometrySpecification
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasApproved()
        {
            var @event = _fixture.Create<AddressWasApproved>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasApproved>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Current
                            && doc.Active == true
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed()
        {
            var @event = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Proposed
                            && doc.Active == true
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
        {
            var @event = _fixture.Create<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Proposed
                            && doc.Active == true
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected()
        {
            var @event = _fixture.Create<AddressWasRejected>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRejected>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Rejected
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var @event = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRejected>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Rejected
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var @event = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRetired>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Rejected
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var @event = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRetired>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Rejected
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var @event = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Rejected
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseOfMunicipalityMerger()
        {
            var @event = _fixture.Create<AddressWasRejectedBecauseOfMunicipalityMerger>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Rejected
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2()
        {
            var @event = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Retired
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseHouseNumberWasRetired()
        {
            var @event = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Retired
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var @event = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Retired
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRetired()
        {
            var @event = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Retired
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseOfMunicipalityMerger()
        {
            var @event = _fixture.Create<AddressWasRetiredBecauseOfMunicipalityMerger>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Retired
                            && doc.Active == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
        {
            var @event = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Current
                            && doc.Active == true
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated()
        {
            var @event = _fixture.Create<AddressWasDeregulated>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasDeregulated>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Current
                            && doc.Active == true
                            && doc.OfficiallyAssigned == false
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressWasRegularized()
        {
            var @event = _fixture.Create<AddressWasRegularized>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            await _sut
                .Given(new Envelope<AddressWasRegularized>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.OfficiallyAssigned == true
                            && doc.Status == null
                            && doc.Active == null
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.AddressPosition == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasChangedV2()
        {
            var @event = _fixture.Create<AddressPostalCodeWasChangedV2>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var storedDocuments = new[] { @event.AddressPersistentLocalId }.Concat(@event.BoxNumberPersistentLocalIds)
                .Select(x =>
                {
                    var document = _fixture.Create<AddressSearchDocument>();
                    document.AddressPersistentLocalId = x;
                    return document;
                })
                .ToArray();

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedDocuments);

            var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = @event.PostalCode,
                PostalNames = new List<PostalInfoPostalName>
                {
                    postalInfoPostalName
                }
            });
            await _postalConsumerContext.SaveChangesAsync();

            await _sut
                .Given(new Envelope<AddressPostalCodeWasChangedV2>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.PostalInfo!.PostalCode == @event.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.PostalInfo.Names.Single().Language == Language.nl
                            && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            ),
                        It.IsAny<CancellationToken>()));

                    foreach (var boxNumberAddressPersistentLocalId in @event.BoxNumberPersistentLocalIds)
                    {
                        _elasticSearchClient.Verify(x => x.UpdateDocument(
                            It.Is<AddressSearchDocument>(doc =>
                                doc.AddressPersistentLocalId == boxNumberAddressPersistentLocalId
                                && doc.PostalInfo!.PostalCode == @event.PostalCode
                                && doc.PostalInfo.Names.Length == 1
                                && doc.PostalInfo.Names.Single().Language == Language.nl
                                && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
                                && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            ),
                            It.IsAny<CancellationToken>()));
                    }

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressPostalCodeWasCorrectedV2()
        {
            var @event = _fixture.Create<AddressPostalCodeWasCorrectedV2>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var storedDocuments = new[] { @event.AddressPersistentLocalId }.Concat(@event.BoxNumberPersistentLocalIds)
                .Select(x =>
                {
                    var document = _fixture.Create<AddressSearchDocument>();
                    document.AddressPersistentLocalId = x;
                    return document;
                })
                .ToArray();

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedDocuments);

            var postalInfoPostalName = new PostalInfoPostalName("9030", PostalLanguage.Dutch, "Mariakerke");
            _postalConsumerContext.PostalLatestItems.Add(new PostalLatestItem
            {
                PostalCode = @event.PostalCode,
                PostalNames = new List<PostalInfoPostalName>
                {
                    postalInfoPostalName
                }
            });
            await _postalConsumerContext.SaveChangesAsync();

            await _sut
                .Given(new Envelope<AddressPostalCodeWasCorrectedV2>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.PostalInfo!.PostalCode == @event.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.PostalInfo.Names.Single().Language == Language.nl
                            && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                        ),
                        It.IsAny<CancellationToken>()));

                    foreach (var boxNumberAddressPersistentLocalId in @event.BoxNumberPersistentLocalIds)
                    {
                        _elasticSearchClient.Verify(x => x.UpdateDocument(
                            It.Is<AddressSearchDocument>(doc =>
                                doc.AddressPersistentLocalId == boxNumberAddressPersistentLocalId
                                && doc.PostalInfo!.PostalCode == @event.PostalCode
                                && doc.PostalInfo.Names.Length == 1
                                && doc.PostalInfo.Names.Single().Language == Language.nl
                                && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
                                && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            ),
                            It.IsAny<CancellationToken>()));
                    }

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2()
        {
            var @event = _fixture.Create<AddressHouseNumberWasCorrectedV2>();
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey,  _fixture.Create<long>() }
            };

            var storedDocuments = new[] { @event.AddressPersistentLocalId }.Concat(@event.BoxNumberPersistentLocalIds)
                .Select(x =>
                {
                    var document = _fixture.Create<AddressSearchDocument>();
                    document.AddressPersistentLocalId = x;
                    return document;
                })
                .ToArray();

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedDocuments);

            await _sut
                .Given(new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(@event, eventMetadata)))
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<AddressSearchDocument>(doc =>
                            doc.AddressPersistentLocalId == @event.AddressPersistentLocalId
                            && doc.HouseNumber == @event.HouseNumber
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                        ),
                        It.IsAny<CancellationToken>()));

                    foreach (var boxNumberAddressPersistentLocalId in @event.BoxNumberPersistentLocalIds)
                    {
                        _elasticSearchClient.Verify(x => x.UpdateDocument(
                            It.Is<AddressSearchDocument>(doc =>
                                doc.AddressPersistentLocalId == boxNumberAddressPersistentLocalId
                                && doc.HouseNumber == @event.HouseNumber
                                && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            ),
                            It.IsAny<CancellationToken>()));
                    }

                    return Task.CompletedTask;
                });
        }
    }
}
