namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingStreetNameNames
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
        public void ThenStreetNameNamesWereCorrected()
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var command = Fixture.Create<CorrectStreetNameNames>();

            Assert(new Scenario()
                .Given(_streamId, streetNameWasImported, addressWasProposedV2)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameNamesWereCorrected(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            command.StreetNameNames,
                            new List<AddressPersistentLocalId> { new(addressWasProposedV2.AddressPersistentLocalId) })
                        )
                    ));
        }

        [Fact]
        public void WithRemovedAddress_ThenOnlyNonRemovedAddressesAreAffected()
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var command = Fixture.Create<CorrectStreetNameNames>();

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();
            var removedAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Current)
                .WithRemoved();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    addressWasProposedV2,
                    removedAddressWasMigrated)
                .When(command)
                .Then(new Fact(
                    new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameNamesWereCorrected(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        command.StreetNameNames,
                        new List<AddressPersistentLocalId> { new (addressWasProposedV2.AddressPersistentLocalId) }))
                ));
        }

        [Fact]
        public void StateCheck()
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrected(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<Dictionary<string, string>>(),
                new List<AddressPersistentLocalId> { new(addressWasProposedV2.AddressPersistentLocalId) });
            ((ISetProvenance)streetNameNamesWereCorrected).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();

            // Act
            sut.Initialize(new List<object>
            {
                migratedStreetNameWasImported,
                addressWasProposedV2,
                streetNameNamesWereCorrected
            });

            // Assert
            foreach (var streetNameAddress in sut.StreetNameAddresses)
            {
                streetNameAddress.LastEventHash.Should().Be(streetNameNamesWereCorrected.GetHash());
            }
        }
    }
}
