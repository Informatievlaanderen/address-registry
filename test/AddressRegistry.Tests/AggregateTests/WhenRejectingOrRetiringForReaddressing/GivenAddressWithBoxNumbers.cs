namespace AddressRegistry.Tests.AggregateTests.WhenRejectingOrRetiringForReaddressing
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressWithBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenAddressWithBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenAddressIsRejected()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(20);

            var addressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .Build();

            var boxNumberA = new BoxNumber("A");
            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber(addressWasMigrated.HouseNumber))
                .WithBoxNumber(boxNumberA, addressPersistentLocalId)
                .Build();

            var boxNumberB = new BoxNumber("B");
            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber(addressWasMigrated.HouseNumber))
                .WithBoxNumber(boxNumberB, addressPersistentLocalId)
                .Build();

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(_streetNamePersistentLocalId + 1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId + 1);

            var command = new RejectOrRetireAddressForReaddress(
                _streetNamePersistentLocalId,
                destinationStreetNamePersistentLocalId,
                addressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new List<BoxNumberAddressPersistentLocalId>
                {
                    new BoxNumberAddressPersistentLocalId(boxNumberA, new AddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId + 1)),
                    new BoxNumberAddressPersistentLocalId(boxNumberB, new AddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId + 1))
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressHouseNumberWasReplacedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            destinationStreetNamePersistentLocalId,
                            addressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new List<AddressBoxNumberReplacedBecauseOfReaddressData>
                            {
                                new AddressBoxNumberReplacedBecauseOfReaddressData(
                                    proposedBoxNumberAddressPersistentLocalId,
                                    new AddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId + 1)),
                                new AddressBoxNumberReplacedBecauseOfReaddressData(
                                    currentBoxNumberAddressPersistentLocalId,
                                    new AddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId + 1))
                            })),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            currentBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                           proposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                           addressPersistentLocalId))
                }));
        }
    }
}
