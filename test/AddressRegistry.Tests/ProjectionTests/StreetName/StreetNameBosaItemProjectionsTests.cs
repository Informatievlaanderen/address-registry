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

    public class StreetNameBosaItemProjectionsTests : KafkaProjectionTest<StreetNameConsumerContext, StreetNameBosaItemProjections>
    {
        private readonly Fixture _fixture;
        private readonly Provenance _provenance;

        private readonly StreetNameWasProposedV2 _streetNameWasProposedV2;

        private Dictionary<string, string> _names = new Dictionary<string, string>
        {
            { StreetNameBosaItemProjections.Dutch, "nl-name" },
            { StreetNameBosaItemProjections.French, "fr-name" },
            { StreetNameBosaItemProjections.German, "ger-name" },
            { StreetNameBosaItemProjections.English, "en-name" },
        };

        private Dictionary<string, string> _homonyms = new Dictionary<string, string>
        {
            { StreetNameBosaItemProjections.Dutch, "nl hom" },
            { StreetNameBosaItemProjections.French, "fr hom" },
            { StreetNameBosaItemProjections.German, "ger hom" },
            { StreetNameBosaItemProjections.English, "en hom" },
        };

        public StreetNameBosaItemProjectionsTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithContractProvenance());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _provenance = new Provenance(_fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(), string.Empty, string.Empty, string.Empty, string.Empty);

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
                var result = await ctx.StreetNameBosaItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.NisCode.Should().Be(e.NisCode);
                result.Status.Should().Be(StreetNameBosaItem.ConvertStringToStatus(e.Status));

                AssertNames(result);
                AssertHomonyms(result);

                result.IsRemoved.Should().Be(e.IsRemoved);

                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task StreetNameWasProposed()
        {
            Given(_streetNameWasProposedV2);
            await Then(async ctx =>
            {
                var result = await ctx.StreetNameBosaItems.FindAsync(_streetNameWasProposedV2.PersistentLocalId);
                result.Should().NotBeNull();
                result.PersistentLocalId.Should().Be(_streetNameWasProposedV2.PersistentLocalId);
                result.NisCode.Should().Be(_streetNameWasProposedV2.NisCode);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);

                AssertNames(result);

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
                var result = await ctx.StreetNameBosaItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Current);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
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
                var result = await ctx.StreetNameBosaItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Rejected);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
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
                var result = await ctx.StreetNameBosaItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Retired);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);
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
                var result = await ctx.StreetNameBosaItems.FindAsync((int)streetNamePersistentLocalId);
                result.Should().NotBeNull();
                result.PersistentLocalId.Should().Be(e.PersistentLocalId);
                result.Status.Should().Be(AddressRegistry.Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(e.Provenance.Timestamp).Value);

                AssertNames(result);
            });
        }

        private static void AssertNames(StreetNameBosaItem item)
        {
            item.NameDutch.Should().Be("nl-name");
            item.NameDutchSearch.Should().Be("nlname");
            item.NameFrench.Should().Be("fr-name");
            item.NameFrenchSearch.Should().Be("frname");
            item.NameGerman.Should().Be("ger-name");
            item.NameGermanSearch.Should().Be("gername");
            item.NameEnglish.Should().Be("en-name");
            item.NameEnglishSearch.Should().Be("enname");
        }

        private static void AssertHomonyms(StreetNameBosaItem result)
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

        protected override StreetNameBosaItemProjections CreateProjection()
            => new StreetNameBosaItemProjections();
    }
}
