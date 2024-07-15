namespace AddressRegistry.Tests.ProjectionTests.Postal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Postal;
    using AddressRegistry.Consumer.Read.Postal.Projections;
    using AddressRegistry.StreetName;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.PostalRegistry;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using NodaTime.Text;
    using Xunit;
    using Xunit.Abstractions;

    public class MunicipalityLatestItemProjectionsTests : KafkaProjectionTest<PostalConsumerContext, PostalLatestItemProjections>
    {
        private readonly Provenance _provenance;
        private readonly PostalCode _postalCode;
        private string PostalCodeString => _postalCode;

        private readonly PostalInformationWasRegistered _postalInformationWasRegistered;

        public MunicipalityLatestItemProjectionsTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithContractProvenance());
            Fixture.Customize(new WithFixedPostalCode());

            _provenance = new Provenance(Fixture.Create<Instant>().Plus(Duration.FromMinutes(1)).ToString(), string.Empty, string.Empty, string.Empty, string.Empty);

            _postalCode = Fixture.Create<PostalCode>();

            _postalInformationWasRegistered = new PostalInformationWasRegistered(
                _postalCode,
                _provenance);
        }

        [Fact]
        public async Task PostalInformationWasRegistered()
        {
            Given(_postalInformationWasRegistered);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.NisCode.Should().BeNull();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task PostalInformationWasRealized()
        {
            var e = new PostalInformationWasRealized(
                _postalCode,
                _provenance);

            Given(_postalInformationWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.Status.Should().Be(PostalStatus.Current);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task PostalInformationWasRetired()
        {
            var e = new PostalInformationWasRetired(
                _postalCode,
                _provenance);

            Given(_postalInformationWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.Status.Should().Be(PostalStatus.Retired);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task PostalInformationNameWasAdded()
        {
            var e = new PostalInformationPostalNameWasAdded(
                _postalCode,
                Fixture.Create<PostalLanguage>().ToString(),
                Fixture.Create<string>(),
                _provenance);

            Given(_postalInformationWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.PostalNames.Should().BeEquivalentTo(new List<PostalInfoPostalName>
                {
                    new PostalInfoPostalName
                    {
                        Language = (PostalLanguage)Enum.Parse(typeof(PostalLanguage), e.Language, true),
                        PostalName = e.Name,
                        PostalCode = _postalCode
                    }
                });
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task PostalInformationNameWasRemoved()
        {
            var added = new PostalInformationPostalNameWasAdded(
                _postalCode,
                Fixture.Create<PostalLanguage>().ToString(),
                Fixture.Create<string>(),
                _provenance);

            var removed = new PostalInformationPostalNameWasRemoved(
                _postalCode,
                added.Language,
                added.Name,
                _provenance);

            Given(_postalInformationWasRegistered, added, removed);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.PostalNames.Should().BeEmpty();
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task PostalInformationWasMunicipalityWasAttached()
        {
            var nisCode = "11001";
            var e = new MunicipalityWasAttached(
                _postalCode,
                nisCode,
                _provenance);

            Given(_postalInformationWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.NisCode.Should().Be(nisCode);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        [Fact]
        public async Task PostalInformationWasMunicipalityWasRelinked()
        {
            var nisCode = "11001";
            var e = new MunicipalityWasRelinked(
                _postalCode,
                nisCode,
                "11000",
                _provenance);

            Given(_postalInformationWasRegistered, e);
            await Then(async ctx =>
            {
                var result = await ctx.PostalLatestItems.FindAsync(PostalCodeString);
                result.Should().NotBeNull();
                result.NisCode.Should().Be(nisCode);
                result.VersionTimestamp.Should().Be(InstantPattern.General.Parse(_postalInformationWasRegistered.Provenance.Timestamp).Value);
            });
        }

        protected override PostalConsumerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PostalConsumerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new PostalConsumerContext(options);
        }

        protected override PostalLatestItemProjections CreateProjection()
            => new PostalLatestItemProjections();
    }
}
