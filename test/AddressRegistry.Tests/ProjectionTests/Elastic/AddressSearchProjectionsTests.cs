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
                            && doc.Municipality.Names.Single(n => n.Language == Language.nl).Spelling == municipalityLatestItem.NameDutch
                            && doc.Municipality.Names.Single(n => n.Language == Language.fr).Spelling == municipalityLatestItem.NameFrench
                            && doc.PostalInfo!.PostalCode == postalInfoPostalName.PostalCode
                            && doc.PostalInfo.Names.Length == 1
                            && doc.PostalInfo.Names.Single().Language == Language.nl
                            && doc.PostalInfo.Names.Single().Spelling == postalInfoPostalName.PostalName
                            && doc.StreetName.StreetNamePersistentLocalId == @event.StreetNamePersistentLocalId
                            && doc.StreetName.Names.Length == 2
                            && doc.StreetName.Names.Single(n => n.Language == Language.nl).Spelling == streetNameLatestItem.NameDutch
                            && doc.StreetName.Names.Single(n => n.Language == Language.fr).Spelling == streetNameLatestItem.NameFrench
                            && doc.StreetName.HomonymAdditions.Length == 2
                            && doc.StreetName.HomonymAdditions.Single(n => n.Language == Language.nl).Spelling == streetNameLatestItem.HomonymAdditionDutch
                            && doc.StreetName.HomonymAdditions.Single(n => n.Language == Language.fr).Spelling == streetNameLatestItem.HomonymAdditionFrench
                            && doc.FullAddress.Length == 2
                            // full name
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
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        @event.AddressPersistentLocalId,
                        It.Is<AddressSearchPartialDocument>(doc =>
                            doc.Status == AddressStatus.Current
                            && doc.Active == true
                            && doc.VersionTimestamp == @event.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            // && doc.PostalCode == null
                            // && doc.StreetName == null
                            // && doc.AddressPosition == null
                            // && doc.FullAddress == null
                            // && doc.HouseNumber == null
                            // && doc.BoxNumber == null
                            // && doc.OfficiallyAssigned == null
                            ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }
    }
}
