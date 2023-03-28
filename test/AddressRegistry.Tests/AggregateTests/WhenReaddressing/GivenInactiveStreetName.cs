namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing
{
    using Api.BackOffice.Abstractions;
    using StreetName;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInactiveStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenInactiveStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedValidHouseNumber());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithRemovedStreetName_ThenThrowsStreetNameIsRemovedException()
        {
            Assert(new Scenario()
                 .Given(_streamId,
                     Fixture.Create<MigratedStreetNameWasImported>(),
                     Fixture.Create<StreetNameWasRemoved>())
                 .When(Fixture.Create<Readdress>())
                 .Throws(new StreetNameIsRemovedException(Fixture.Create<StreetNamePersistentLocalId>())));
        }

        [Fact]
        public void WithRetiredStreetName_ThenThrowsStreetNameHasInvalidStatusException()
        {
            Assert(new Scenario()
                 .Given(_streamId,
                     Fixture.Create<MigratedStreetNameWasImported>(),
                     Fixture.Create<StreetNameWasRetired>())
                 .When(Fixture.Create<Readdress>())
                 .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithRejectedStreetName_ThenThrowsStreetNameHasInvalidStatusException()
        {
            Assert(new Scenario()
                 .Given(_streamId,
                     Fixture.Create<MigratedStreetNameWasImported>(),
                     Fixture.Create<StreetNameWasRejected>())
                 .When(Fixture.Create<Readdress>())
                 .Throws(new StreetNameHasInvalidStatusException()));
        }
    }
}
