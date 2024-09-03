namespace AddressRegistry.Tests.ProjectionTests.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.StreetName;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.StreetName;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using NodaTime.Text;
    using Xunit;
    using Xunit.Abstractions;
    using StreetNameStatus = AddressRegistry.StreetName.StreetNameStatus;

    public class StreetNameLatestItemProjectionsTests : KafkaProjectionTest<StreetNameConsumerContext, StreetNameLatestItemProjections>
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

        public StreetNameLatestItemProjectionsTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithContractProvenance());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _provenance = new Provenance(
                _fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(), string.Empty, string.Empty, string.Empty, string.Empty);

            _streetNameWasProposedV2 = new StreetNameWasProposedV2(
                _fixture.Create<Guid>().ToString(),
                _fixture.Create<string>(),
                _names,
                _fixture.Create<StreetNamePersistentLocalId>(),
                _provenance);
        }

        [Fact]
        public async Task StreetNameWasMigratedToMunicipality()
        {
            var guid = _fixture.Create<Guid>();
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameWasMigratedToMunicipality(
                guid.ToString(),
                _fixture.Create<string>(),
                guid.ToString(),
                streetNamePersistentLocalId,
                _fixture.Create<StreetNameStatus>().ToString(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _names,
                _homonyms,
                isCompleted: true,
                isRemoved: true,
                _provenance);

            Given(e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.NisCode.Should().Be(e.NisCode);
                result.Status.Should().Be(StreetNameLatestItem.ConvertStringToStatus(e.Status));

                AssertNames(result);
                AssertHomonyms(result);

                result.IsRemoved.Should().Be(e.IsRemoved);

                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasProposedV2()
        {
            Given(_streetNameWasProposedV2);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync(_streetNameWasProposedV2.PersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(_streetNameWasProposedV2.PersistentLocalId);
                result.NisCode.Should().Be(_streetNameWasProposedV2.NisCode);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);

                AssertNames(result);

                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_streetNameWasProposedV2.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasProposedForMunicipalityMerger()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();
            var streetNameWasProposed = new StreetNameWasProposedForMunicipalityMerger(
                _fixture.Create<Guid>().ToString(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _names,
                _homonyms,
                streetNamePersistentLocalId,
                [streetNamePersistentLocalId+1],
                _provenance);

            Given(streetNameWasProposed);

            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync(
                    streetNameWasProposed.PersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(streetNameWasProposed.PersistentLocalId);
                result.NisCode.Should().Be(streetNameWasProposed.NisCode);
                result.Status.Should().Be(
                    AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);

                AssertNames(result);
                AssertHomonyms(result);

                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_streetNameWasProposedV2.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasApproved()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameWasApproved(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Current);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasCorrectedFromApprovedToProposed()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            var @event = new StreetNameWasCorrectedFromApprovedToProposed(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, streetNameWasApproved, @event);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(@event.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(@event.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasRejected()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameWasRejected(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Rejected);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasRejectedBecauseOfMunicipalityMerger()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var @event = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                [streetNamePersistentLocalId+1, streetNamePersistentLocalId+2],
                _provenance);

            Given(_streetNameWasProposedV2, @event);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(@event.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Rejected);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(@event.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasCorrectedFromRejectedToProposed()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasRejected = new StreetNameWasRejected(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            var @event = new StreetNameWasCorrectedFromRejectedToProposed(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, streetNameWasRejected, @event);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(@event.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(@event.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasRetiredV2()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameWasRetiredV2(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Retired);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasRetiredBecauseOfMunicipalityMerger()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                [streetNamePersistentLocalId+1, streetNamePersistentLocalId+2],
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Retired);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasCorrectedFromRetiredToCurrent()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasApproved = new StreetNameWasApproved(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            var streetNameWasRetiredV2 = new StreetNameWasRetiredV2(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            var @event = new StreetNameWasCorrectedFromRetiredToCurrent(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2,streetNameWasApproved, streetNameWasRetiredV2, @event);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(@event.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Current);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(@event.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameNamesWereCorrected()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameNamesWereCorrected(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _names,
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);

                AssertNames(result);
            });
        }

        [Fact]
        public async Task StreetNameHomonymAdditionsWereCorrected()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var @event = new StreetNameHomonymAdditionsWereCorrected(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                new Dictionary<string, string> { { "Dutch", "Corrected" } },
                _provenance);

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                streetNamePersistentLocalId,
                StreetNameStatus.Current.ToString(),
                "Dutch",
                null,
                new Dictionary<string, string> { {"Dutch", "STRAAT"}, },
                new Dictionary<string, string> { {"Dutch", "DEF"}, },
                true,
                false,
                new Provenance(
                    _fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));

            Given(streetNameWasMigratedToMunicipality, @event);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(@event.PersistentLocalId);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(@event.Provenance.Timestamp).Value);

                result.HomonymAdditionDutch.Should().Be("Corrected");
            });
        }

        [Fact]
        public async Task StreetNameHomonymAdditionsWereRemoved()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var @event = new StreetNameHomonymAdditionsWereRemoved(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                new List<string>() { "Dutch" },
                _provenance);

            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipality(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                Fixture.Create<StreetNameId>(),
                streetNamePersistentLocalId,
                StreetNameStatus.Current.ToString(),
                "Dutch",
                null,
                new Dictionary<string, string> { {"Dutch", "STRAAT"}, },
                new Dictionary<string, string> { {"Dutch", "DEF"}, {"French", "QSD"}, },
                true,
                false,
                new Provenance(
                    _fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));

            Given(streetNameWasMigratedToMunicipality, @event);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(@event.PersistentLocalId);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(@event.Provenance.Timestamp).Value);

                result.HomonymAdditionDutch.Should().BeNull();
                result.HomonymAdditionFrench.Should().Be("QSD");
            });
        }

        [Fact]
        public async Task StreetNameWasRemovedV2()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasRemovedV2 = new StreetNameWasRemovedV2(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                _provenance);

            Given(_streetNameWasProposedV2, streetNameWasRemovedV2);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.IsRemoved.Should().BeTrue();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(streetNameWasRemovedV2.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasRenamed()
        {
            var streetNamePersistentLocalId = _fixture.Create<StreetNamePersistentLocalId>();

            var e = new StreetNameWasRenamed(
                _fixture.Create<Guid>().ToString(),
                streetNamePersistentLocalId,
                streetNamePersistentLocalId + 1,
                _provenance);

            Given(_streetNameWasProposedV2, e);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameLatestItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result!.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Retired);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        private static void AssertNames(StreetNameLatestItem item)
        {
            item.NameDutch.Should().Be("nl-name");
            item.NameDutchSearch.Should().Be("nl-name");
            item.NameFrench.Should().Be("fr-name");
            item.NameFrenchSearch.Should().Be("fr-name");
            item.NameGerman.Should().Be("ger-name");
            item.NameGermanSearch.Should().Be("ger-name");
            item.NameEnglish.Should().Be("en-name");
            item.NameEnglishSearch.Should().Be("en-name");
        }

        private static void AssertHomonyms(StreetNameLatestItem result)
        {
            result.HomonymAdditionDutch.Should().Be("nl hom");
            result.HomonymAdditionFrench.Should().Be("fr hom");
            result.HomonymAdditionGerman.Should().Be("ger hom");
            result.HomonymAdditionEnglish.Should().Be("en hom");
        }

        protected override StreetNameConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<StreetNameConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new StreetNameConsumerContext(options);
        }

        protected override StreetNameLatestItemProjections CreateProjection()
            => new StreetNameLatestItemProjections();
    }
}
