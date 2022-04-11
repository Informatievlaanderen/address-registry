namespace AddressRegistry.Tests.AggregateTests.WhenImportingMigratedStreetName;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
using global::AutoFixture;
using StreetName;
using StreetName.Commands;
using StreetName.Events;
using Xunit;
using Xunit.Abstractions;

public class GivenStreetNameAlreadyExists : AddressRegistryTest
{
    private readonly StreetNameStreamId _streamId;

    public GivenStreetNameAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        Fixture.Customize(new InfrastructureCustomization());
        Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
        _streamId = Fixture.Create<StreetNameStreamId>();
    }

    [Fact]
    public void ThenStreetNameAlreadyExists()
    {
        var command = Fixture.Create<ImportMigratedStreetName>();

        Assert(new Scenario()
            .Given(_streamId, new object[]
            {
                Fixture.Create<MigratedStreetNameWasImported>()
            })
            .When(command)
            .Throws(new AggregateSourceException($"StreetName with id {command.PersistentLocalId} already exists")));
    }

}
