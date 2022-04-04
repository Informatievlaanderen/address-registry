namespace AddressRegistry.Tests.AggregateTests.WhenRemovingStreetName;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
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
    public void ThenStreetNameWasApproved()
    {
        var command = Fixture.Create<RemoveStreetName>();

        Assert(new Scenario()
            .Given(_streamId, Fixture.Create<StreetNameWasImported>())
            .When(command)
            .Then(
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasRemoved(command.PersistentLocalId))));
    }

    [Fact]
    public void WithAlreadyRemovedStreetName_ThenNone()
    {
        var command = Fixture.Create<RemoveStreetName>();

        Assert(new Scenario()
            .Given(_streamId,
                Fixture.Create<StreetNameWasImported>(),
                Fixture.Create<StreetNameWasRemoved>())
            .When(command)
            .ThenNone());
    }
}
