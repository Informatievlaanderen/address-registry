namespace AddressRegistry.Tests.ProjectionTests.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Consumer.Read.StreetName.Projections.Elastic;
    using AddressRegistry.StreetName;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using global::AutoFixture;
    using Infrastructure.Elastic;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime;
    using Tests.BackOffice.Infrastructure;
    using Xunit;
    using Xunit.Abstractions;
    using StreetNameStatus = AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus;

    public class StreetNameElasticProjectionsTests : KafkaProjectionTest<StreetNameConsumerContext, StreetNameSearchProjections>
    {
        private readonly Fixture _fixture;
        private readonly Provenance _provenance;

        private readonly StreetNameWasProposedV2 _streetNameWasProposedV2;

        private readonly Dictionary<string, string> _names = new()
        {
            { StreetNameLatestItemProjections.Dutch, "nl-name" },
            { StreetNameLatestItemProjections.French, "fr-name" },
            { StreetNameLatestItemProjections.German, "ger-name" },
            { StreetNameLatestItemProjections.English, "en-name" },
        };

        private readonly Dictionary<string, string> _homonyms = new()
        {
            { StreetNameLatestItemProjections.Dutch, "nl hom" },
            { StreetNameLatestItemProjections.French, "fr hom" },
            { StreetNameLatestItemProjections.German, "ger hom" },
            { StreetNameLatestItemProjections.English, "en hom" },
        };

        private readonly TestMunicipalityConsumerContext _municipalityContext;
        private readonly Mock<IDbContextFactory<MunicipalityConsumerContext>> _municipalityDbContextFactory;
        private readonly Mock<IStreetNameElasticsearchClient> _elasticClientMock;
        private readonly Instant _timestamp;
        private readonly MunicipalityLatestItem _municipalityLatestItem;

        public StreetNameElasticProjectionsTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithContractProvenance());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _timestamp = _fixture.Create<Instant>().Plus(Duration.FromMinutes(1));
            _provenance = new Provenance(
                _timestamp.ToString(), string.Empty, string.Empty, string.Empty, string.Empty);

            var nisCode = "12345";
            var municipalityId = Guid.NewGuid();

            _streetNameWasProposedV2 = new StreetNameWasProposedV2(
                municipalityId.ToString(),
                nisCode,
                _names,
                _fixture.Create<StreetNamePersistentLocalId>(),
                _provenance);

            _elasticClientMock = new Mock<IStreetNameElasticsearchClient>();

            _municipalityContext = new FakeMunicipalityConsumerContextFactory().CreateDbContext();
            _municipalityDbContextFactory = new Mock<IDbContextFactory<MunicipalityConsumerContext>>();
            _municipalityDbContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_municipalityContext);

            _municipalityLatestItem = new MunicipalityLatestItem
            {
                NisCode = nisCode,
                MunicipalityId = municipalityId,
                NameDutch = "Gent",
                NameFrench = "Gand"
            };
            _municipalityContext.MunicipalityLatestItems.Add(_municipalityLatestItem);
            _municipalityContext.SaveChanges();
        }

        [Fact]
        public async Task StreetNameWasMigratedToMunicipality()
        {
            var guid = _fixture.Create<Guid>();
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var streetNameStatus = _fixture.Create<StreetNameStatus>();
            var e = new StreetNameWasMigratedToMunicipality(
                _municipalityLatestItem.MunicipalityId.ToString(),
                _fixture.Create<string>(),
                guid.ToString(),
                streetNamePersistentLocalId,
                streetNameStatus.ToString(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _names,
                _homonyms,
                isCompleted: true,
                isRemoved: false,
                _provenance);

            Given(e);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.CreateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.StreetNamePersistentLocalId == e.PersistentLocalId
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.Status == streetNameStatus
                        && doc.Names.Length == _names.Count
                        && doc.HomonymAdditions.Length == _homonyms.Count
                        && doc.FullStreetNames.Length == _names.Count
                        && doc.Municipality.NisCode == _municipalityLatestItem.NisCode
                        && doc.Municipality.Names.Length == 2
                        ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasProposedV2()
        {
            Given(_streetNameWasProposedV2);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.CreateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.StreetNamePersistentLocalId == _streetNameWasProposedV2.PersistentLocalId
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.Status == StreetNameStatus.Proposed
                        && doc.Names.Length == _names.Count
                        && doc.HomonymAdditions.Length == 0
                        && doc.FullStreetNames.Length == _names.Count
                        && doc.Municipality.NisCode == _streetNameWasProposedV2.NisCode
                        && doc.Municipality.Names.Length == 2
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasProposedForMunicipalityMerger()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();
            var streetNameWasProposed = new StreetNameWasProposedForMunicipalityMerger(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.NisCode,
                _fixture.Create<string>(),
                _names,
                _homonyms,
                streetNamePersistentLocalId,
                [streetNamePersistentLocalId+1],
                _provenance);

            Given(streetNameWasProposed);

            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.CreateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.StreetNamePersistentLocalId == streetNamePersistentLocalId
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.Status == StreetNameStatus.Proposed
                        && doc.Names.Length == _names.Count
                        && doc.HomonymAdditions.Length == _homonyms.Count
                        && doc.FullStreetNames.Length == _names.Count
                        && doc.Municipality.NisCode == streetNameWasProposed.NisCode
                        && doc.Municipality.Names.Length == 2
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasApproved()
        {
            var e = new StreetNameWasApproved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Current
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasCorrectedFromApprovedToProposed()
        {
            var approved = new StreetNameWasApproved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var @event = new StreetNameWasCorrectedFromApprovedToProposed(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, approved, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Proposed
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasRejected()
        {
            var @event = new StreetNameWasRejected(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Rejected
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasRejectedBecauseOfMunicipalityMerger()
        {
            var @event = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                [],
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Rejected
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasCorrectedFromRejectedToProposed()
        {
            var streetNameWasRejected = new StreetNameWasRejected(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var @event = new StreetNameWasCorrectedFromRejectedToProposed(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, streetNameWasRejected, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Proposed
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasRetiredV2()
        {
            var approved = new StreetNameWasApproved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var @event = new StreetNameWasRetiredV2(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, approved, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Retired
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasRenamed()
        {
            var approved = new StreetNameWasApproved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var @event = new StreetNameWasRenamed(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _fixture.Create<int>(),
                _provenance);

            Given(_streetNameWasProposedV2, approved, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Retired
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasRetiredBecauseOfMunicipalityMerger()
        {
            var approved = new StreetNameWasApproved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var @event = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                [],
                _provenance);

            Given(_streetNameWasProposedV2, approved, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Retired
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasCorrectedFromRetiredToCurrent()
        {
            var streetNameWasApproved = new StreetNameWasApproved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var streetNameWasRetiredV2 = new StreetNameWasRetiredV2(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            var @event = new StreetNameWasCorrectedFromRetiredToCurrent(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, streetNameWasApproved, streetNameWasRetiredV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.PartialUpdateDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.Is<StreetNameSearchPartialDocument>(doc =>
                        doc.Status == StreetNameStatus.Current
                        && doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameNamesWereCorrected()
        {
            _elasticClientMock.Setup(x => x.GetDocument(_streetNameWasProposedV2.PersistentLocalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StreetNameSearchDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    DateTimeOffset.Now,
                    StreetNameStatus.Proposed,
                    new Municipality(_streetNameWasProposedV2.NisCode, [new Name("Gent", Language.nl), new Name("Gand", Language.fr)]),
                    _names.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray(),
                    _homonyms.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray()
                    ));

            var newNames = _names
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value + " corrected"))
                .ToDictionary(x => x.Key, x => x.Value);

            var @event = new StreetNameNamesWereCorrected(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                newNames,
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.UpdateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.Names.All(name => newNames.ContainsValue(name.Spelling))
                        && doc.Names.Length == newNames.Count
                        && doc.FullStreetNames.Length == newNames.Count
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameNamesWereChanged()
        {
            _elasticClientMock.Setup(x => x.GetDocument(_streetNameWasProposedV2.PersistentLocalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StreetNameSearchDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    DateTimeOffset.Now,
                    StreetNameStatus.Proposed,
                    new Municipality(_streetNameWasProposedV2.NisCode, [new Name("Gent", Language.nl), new Name("Gand", Language.fr)]),
                    _names.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray(),
                    _homonyms.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray()
                ));

            var newNames = _names
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value + " changed"))
                .ToDictionary(x => x.Key, x => x.Value);

            var @event = new StreetNameNamesWereChanged(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                newNames,
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.UpdateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.Names.All(name => newNames.ContainsValue(name.Spelling))
                        && doc.Names.Length == newNames.Count
                        && doc.FullStreetNames.Length == newNames.Count
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameHomonymAdditionsWereCorrected()
        {
            _elasticClientMock.Setup(x => x.GetDocument(_streetNameWasProposedV2.PersistentLocalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StreetNameSearchDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    DateTimeOffset.Now,
                    StreetNameStatus.Proposed,
                    new Municipality(_streetNameWasProposedV2.NisCode, [new Name("Gent", Language.nl), new Name("Gand", Language.fr)]),
                    _names.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray(),
                    _homonyms.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray()
                ));

            var newHomonyms = _homonyms
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value + " corrected"))
                .ToDictionary(x => x.Key, x => x.Value);

            var @event = new StreetNameHomonymAdditionsWereCorrected(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                newHomonyms,
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.UpdateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.HomonymAdditions.All(homonym => newHomonyms.ContainsValue(homonym.Spelling))
                        && doc.Names.Length == newHomonyms.Count
                        && doc.FullStreetNames.Length == newHomonyms.Count
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameHomonymAdditionsWereRemoved()
        {
            _elasticClientMock.Setup(x => x.GetDocument(_streetNameWasProposedV2.PersistentLocalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StreetNameSearchDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    DateTimeOffset.Now,
                    StreetNameStatus.Proposed,
                    new Municipality(_streetNameWasProposedV2.NisCode, [new Name("Gent", Language.nl), new Name("Gand", Language.fr)]),
                    _names.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray(),
                    _homonyms.Select(x => new Name(x.Value, StreetNameSearchProjections.MapToLanguage(x.Key))).ToArray()
                ));

            var @event = new StreetNameHomonymAdditionsWereRemoved(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                ["German"],
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.UpdateDocument(
                    It.Is<StreetNameSearchDocument>(doc =>
                        doc.VersionTimestamp.ToString() == _timestamp.ToBelgianDateTimeOffset().ToString()
                        && doc.HomonymAdditions.Length == _homonyms.Count - 1
                        && doc.FullStreetNames.Length == _names.Count
                    ),
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task StreetNameWasRemovedV2()
        {
            var @event = new StreetNameWasRemovedV2(
                _streetNameWasProposedV2.MunicipalityId,
                _streetNameWasProposedV2.PersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(_ =>
            {
                _elasticClientMock.Verify(x => x.DeleteDocument(
                    _streetNameWasProposedV2.PersistentLocalId,
                    It.IsAny<CancellationToken>()));

                return Task.CompletedTask;
            });
        }

        protected override StreetNameConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<StreetNameConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new StreetNameConsumerContext(options);
        }

        protected override StreetNameSearchProjections CreateProjection()
            => new StreetNameSearchProjections(_elasticClientMock.Object, _municipalityDbContextFactory.Object);
    }
}
