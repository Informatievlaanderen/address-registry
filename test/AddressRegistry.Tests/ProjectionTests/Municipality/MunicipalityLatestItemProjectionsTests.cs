namespace AddressRegistry.Tests.ProjectionTests.Municipality
{
    using System;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.StreetName;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using NodaTime.Text;
    using Xunit;
    using Xunit.Abstractions;

    public class MunicipalityLatestItemProjectionsTests : KafkaProjectionTest<MunicipalityConsumerContext, MunicipalityLatestItemProjections>
    {
        private readonly Provenance _provenance;
        private readonly MunicipalityId _municipalityId;
        private Guid _municipalityGuid => (Guid)_municipalityId;

        private readonly MunicipalityWasRegistered _municipalityWasRegistered;

        public MunicipalityLatestItemProjectionsTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithContractProvenance());
            Fixture.Customize(new WithFixedMunicipalityId());

            _provenance = new Provenance(Fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(), string.Empty, string.Empty, string.Empty, string.Empty);

            _municipalityId = Fixture.Create<MunicipalityId>();

            _municipalityWasRegistered = new MunicipalityWasRegistered(
                _municipalityId,
                Fixture.Create<string>(),
                _provenance);
        }

        [Fact]
        public async Task MunicipalityWasRegistered()
        {
            Given(_municipalityWasRegistered);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NisCode.Should().Be(_municipalityWasRegistered.NisCode);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_municipalityWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityBecameCurrent()
        {
            var e = new MunicipalityBecameCurrent(_municipalityId, _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasCorrectedToCurrent()
        {
            var e = new MunicipalityWasCorrectedToCurrent(_municipalityId, _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasCorrectedToRetired()
        {
            var e = new MunicipalityWasCorrectedToRetired(_municipalityId, null, _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasRetired()
        {
            var e = new MunicipalityWasRetired(_municipalityId, null, _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasDrawn()
        {
            var e = new MunicipalityWasDrawn(_municipalityId, Fixture.Create<ExtendedWkbGeometry>(), _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.ExtendedWkbGeometry.Should().BeEquivalentTo(e.ExtendedWkbGeometry.ToByteArray());
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityGeometryWasCorrectedToCleared()
        {
            var e = new MunicipalityGeometryWasCorrectedToCleared(_municipalityId, _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.ExtendedWkbGeometry.Should().BeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityGeometryWasCleared()
        {
            var e = new MunicipalityGeometryWasCleared(_municipalityId, _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.ExtendedWkbGeometry.Should().BeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityGeometryWasCorrected()
        {
            var e = new MunicipalityGeometryWasCorrected(_municipalityId, Fixture.Create<ExtendedWkbGeometry>(), _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.ExtendedWkbGeometry.Should().BeEquivalentTo(e.ExtendedWkbGeometry.ToByteArray());
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityWasNamed()
        {
            var e = new MunicipalityWasNamed(_municipalityId, "nl-name", "dutch", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NameDutch.Should().Be(e.Name);
                result.NameDutchSearch.Should().Be(e.Name);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNameWasCleared()
        {
            var e = new MunicipalityNameWasCleared(_municipalityId, "dutch", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NameDutch.Should().BeNull();
                result.NameDutchSearch.Should().BeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNameWasCorrected()
        {
            var e = new MunicipalityNameWasCorrected(_municipalityId, "dutch-name","dutch", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NameDutch.Should().Be(e.Name);
                result.NameDutchSearch.Should().Be(e.Name);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNameWasCorrectedToCleared()
        {
            var e = new MunicipalityNameWasCorrectedToCleared(_municipalityId, "dutch", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NameDutch.Should().BeNull();
                result.NameDutchSearch.Should().BeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNisCodeWasDefined()
        {
            var e = new MunicipalityNisCodeWasDefined(_municipalityId, "newnis", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NisCode.Should().Be(e.NisCode);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityNisCodeWasCorrected()
        {
            var e = new MunicipalityNisCodeWasCorrected(_municipalityId, "newnis", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.NisCode.Should().Be(e.NisCode);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityOfficialLanguageWasAdded()
        {
            var e = new MunicipalityOfficialLanguageWasAdded(_municipalityId, "german", _provenance);

            Given(_municipalityWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.OfficialLanguages.Contains(e.Language).Should().BeTrue();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task MunicipalityOfficialLanguageWasRemoved()
        {
            var e = new MunicipalityOfficialLanguageWasRemoved(_municipalityId, "german", _provenance);

            Given(_municipalityWasRegistered,
                new MunicipalityOfficialLanguageWasAdded(_municipalityId, "german", _provenance),
                e);
            await Then(async ctx =>
            {
                var result = await ctx.MunicipalityLatestItems.FindAsync(_municipalityGuid);
                result.Should().NotBeNull();
                result.OfficialLanguages.Contains(e.Language).Should().BeFalse();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_provenance.Timestamp).Value);
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
