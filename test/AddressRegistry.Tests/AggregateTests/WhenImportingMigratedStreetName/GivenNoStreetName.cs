namespace AddressRegistry.Tests.AggregateTests.WhenImportingMigratedStreetName
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoStreetName : AddressRegistryTest
    {
        public GivenNoStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenStreetNameWasImported()
        {
            var command = Fixture.Create<ImportMigratedStreetName>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new MigratedStreetNameWasImported(
                            command.StreetNameId,
                            command.PersistentLocalId,
                            command.MunicipalityId,
                            command.NisCode,
                            command.StreetNameStatus))));
        }

        [Fact]
        public void StateCheck()
        {
            var @event = Fixture.Create<MigratedStreetNameWasImported>();

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                @event
            });

            sut.PersistentLocalId.Should().Be(new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId));
            sut.MunicipalityId.Should().Be(new MunicipalityId(@event.MunicipalityId));
            sut.MigratedNisCode.Should().Be(new NisCode(@event.NisCode));
            sut.Status.Should().Be(@event.StreetNameStatus);
        }
    }
}
