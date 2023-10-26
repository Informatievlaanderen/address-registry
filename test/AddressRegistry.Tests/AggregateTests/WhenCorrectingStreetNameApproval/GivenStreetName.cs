namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingStreetNameApproval
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenStreetNameWasCorrectedFromApprovedToProposed()
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var command = Fixture.Create<CorrectStreetNameApproval>();

            Assert(new Scenario()
                .Given(_streamId, streetNameWasImported)
                .When(command)
                .Then(new Fact(
                    new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasCorrectedFromApprovedToProposed(Fixture.Create<StreetNamePersistentLocalId>()))));
        }

        [Fact]
        public void WithAlreadyProposedStreetName_ThenNone()
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Proposed);

            var command = Fixture.Create<CorrectStreetNameApproval>();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { migratedStreetNameWasImported });

            // Act
            sut.CorrectStreetNameApproval();

            // Assert
            sut.Status.Should().Be(StreetNameStatus.Proposed);
        }
    }
}
