namespace AddressRegistry.Tests.ProjectionTests.Municipality
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.StreetName;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using NodaTime.Text;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class MunicipalityProjectionsTests : KafkaProjectionTest<MunicipalityConsumerContext, MunicipalityLatestItemProjections>
    {
        private readonly Fixture _fixture;
        private readonly Guid _municipalityId;
        private readonly MunicipalityWasRegistered _registered;
        private readonly Provenance _provenance;

        public MunicipalityProjectionsTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithContractProvenance());
            _fixture.Customize(new WithFixedStreetNameId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _municipalityId = _fixture.Create<Guid>();

            _registered = new MunicipalityWasRegistered(
                _municipalityId.ToString("D"),
                _fixture.Create<string>(),
                _fixture.Create<Provenance>());

            _provenance = new Provenance(_fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(), string.Empty, string.Empty, string.Empty, string.Empty);
        }

        [Fact]
        public async Task MunicipalityWasRegistered()
        {
            Given(_registered);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.MunicipalityId.Should().Be(_municipalityId);
                expected.NisCode.Should().Be(_registered.NisCode);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_registered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityBecameCurrent()
        {
            var becameCurrent = new MunicipalityBecameCurrent(
                _municipalityId.ToString("D"),
                _provenance);

            Given(_registered, becameCurrent);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.Status.Should().Be(MunicipalityStatus.Current);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(becameCurrent.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasCorrectedToCurrent()
        {
            var municipalityWasCorrectedToCurrent = new MunicipalityWasCorrectedToCurrent(
                _municipalityId.ToString("D"),
                _provenance);

            Given(_registered, municipalityWasCorrectedToCurrent);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.Status.Should().Be(MunicipalityStatus.Current);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasCorrectedToCurrent.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasCorrectedToRetired()
        {
            var municipalityWasCorrectedToRetired = new MunicipalityWasCorrectedToRetired(
                _municipalityId.ToString("D"),
                _fixture.Create<Instant>().ToString(),
                _provenance);

            Given(_registered, municipalityWasCorrectedToRetired);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.Status.Should().Be(MunicipalityStatus.Retired);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasCorrectedToRetired.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasRetired()
        {
            var municipalityWasRetired = new MunicipalityWasRetired(
                _municipalityId.ToString("D"),
                _fixture.Create<Instant>().ToString(),
                _provenance);

            Given(_registered, municipalityWasRetired);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.Status.Should().Be(MunicipalityStatus.Retired);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasRetired.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasDrawn()
        {
            var extendedWkbGeometry = _fixture.Create<ExtendedWkbGeometry>();
            var municipalityWasDrawn = new MunicipalityWasDrawn(
                _municipalityId.ToString("D"),
                extendedWkbGeometry.ToString(),
                _provenance);

            Given(_registered, municipalityWasDrawn);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.ExtendedWkbGeometry.Should().BeEquivalentTo((byte[]?)extendedWkbGeometry);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasDrawn.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityGeometryWasCorrectedToCleared()
        {
            var municipalityWasDrawn = new MunicipalityWasDrawn(
                _municipalityId.ToString("D"),
                _fixture.Create<ExtendedWkbGeometry>().ToString(),
                _provenance);

            var municipalityGeometryWasCorrectedToCleared = new MunicipalityGeometryWasCorrectedToCleared(
                _municipalityId.ToString("D"),
                _provenance);

            Given(_registered, municipalityWasDrawn, municipalityGeometryWasCorrectedToCleared);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.ExtendedWkbGeometry.Should().BeEquivalentTo((byte[]?)null);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityGeometryWasCorrectedToCleared.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityGeometryWasCleared()
        {
            var municipalityWasDrawn = new MunicipalityWasDrawn(
                _municipalityId.ToString("D"),
                _fixture.Create<ExtendedWkbGeometry>().ToString(),
                _provenance);

            var municipalityGeometryWasCleared = new MunicipalityGeometryWasCleared(
                _municipalityId.ToString("D"),
                _provenance);

            Given(_registered, municipalityGeometryWasCleared);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.ExtendedWkbGeometry.Should().BeEquivalentTo((byte[]?)null);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityGeometryWasCleared.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityGeometryWasCorrected()
        {
            var municipalityWasDrawn = new MunicipalityWasDrawn(
                _municipalityId.ToString("D"),
                _fixture.Create<ExtendedWkbGeometry>().ToString(),
                _provenance);

            var extendedWkbGeometry = _fixture.Create<ExtendedWkbGeometry>();
            var municipalityGeometryWasCorrected = new MunicipalityGeometryWasCorrected(
                _municipalityId.ToString("D"),
                extendedWkbGeometry.ToString(),
                _provenance);

            Given(_registered, municipalityWasDrawn, municipalityGeometryWasCorrected);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.ExtendedWkbGeometry.Should().BeEquivalentTo((byte[]?)extendedWkbGeometry);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasDrawn.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasNamed_NL()
        {
            var municipalityWasNamed = new MunicipalityWasNamed(
                _municipalityId.ToString("D"),
                _fixture.Create<string>(),
                "Dutch",
                _provenance);

            Given(_registered, municipalityWasNamed);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.NameDutch.Should().Be(municipalityWasNamed.Name);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasNamed.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNameWasCorrected_NL()
        {
            var municipalityWasNamed = new MunicipalityWasNamed(
                _municipalityId.ToString("D"),
                _fixture.Create<string>(),
                "Dutch",
                _provenance);

            var municipalityNameWasCorrected = new MunicipalityNameWasCorrected(
                _municipalityId.ToString("D"),
                _fixture.Create<string>(),
                "Dutch",
                _provenance);

            Given(_registered, municipalityWasNamed, municipalityNameWasCorrected);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.NameDutch.Should().Be(municipalityNameWasCorrected.Name);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasNamed.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNameWasCorrectedToCleared_NL()
        {
            var municipalityWasNamed = new MunicipalityWasNamed(
                _municipalityId.ToString("D"),
                _fixture.Create<string>(),
                "Dutch",
                _provenance);

            var municipalityNameWasCorrectedToCleared = new MunicipalityNameWasCorrectedToCleared(
                _municipalityId.ToString("D"),
                "Dutch",
                _provenance);

            Given(_registered, municipalityWasNamed, municipalityNameWasCorrectedToCleared);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.NameDutch.Should().Be(null);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityWasNamed.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNisCodeWasDefined()
        {
            var niscodeWasDefined = new MunicipalityNisCodeWasDefined(
                _municipalityId.ToString("D"),
                _fixture.Create<NisCode>().ToString(),
                _provenance);

            Given(_registered, niscodeWasDefined);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.NisCode.Should().Be(niscodeWasDefined.NisCode);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(niscodeWasDefined.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNisCodeWasCorrected()
        {
            var niscodeWasDefined = new MunicipalityNisCodeWasDefined(
                _municipalityId.ToString("D"),
                _fixture.Create<NisCode>().ToString(),
                _provenance);

            var municipalityNisCodeWasCorrected = new MunicipalityNisCodeWasCorrected(
                _municipalityId.ToString("D"),
                _fixture.Create<NisCode>().ToString(),
                _provenance);

            Given(_registered, niscodeWasDefined, municipalityNisCodeWasCorrected);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.NisCode.Should().Be(municipalityNisCodeWasCorrected.NisCode);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(municipalityNisCodeWasCorrected.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityOfficialLanguageWasAdded()
        {
            var officialLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(
                _municipalityId.ToString("D"),
                _fixture.Create<string>(),
                _provenance);

            Given(_registered, officialLanguageWasAdded);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.OfficialLanguages.FirstOrDefault().Should().Be(officialLanguageWasAdded.Language);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(officialLanguageWasAdded.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityOfficialLanguageWasRemoved()
        {
            var language = "dutch";

            var officialLanguageWasAdded = new MunicipalityOfficialLanguageWasAdded(
                _municipalityId.ToString("D"),
                language,
                _provenance);

            var officialLanguageWasRemoved = new MunicipalityOfficialLanguageWasRemoved(
                _municipalityId.ToString("D"),
                language,
                _provenance);

            Given(_registered, officialLanguageWasAdded, officialLanguageWasRemoved);
            await Then(async ct =>
            {
                var expected = await ct.MunicipalityLatestItems.FindAsync(_municipalityId);
                expected.Should().NotBeNull();
                expected.OfficialLanguages.FirstOrDefault().Should().NotBe(officialLanguageWasRemoved.Language);
                expected.VersionTimestamp.Should().Be(InstantPattern.General.Parse(officialLanguageWasRemoved.Provenance.Timestamp).Value);
            });
        }

        protected override MunicipalityConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MunicipalityConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new MunicipalityConsumerContext(options);
        }

        protected override MunicipalityLatestItemProjections CreateProjection()
            => new MunicipalityLatestItemProjections();
    }
}
