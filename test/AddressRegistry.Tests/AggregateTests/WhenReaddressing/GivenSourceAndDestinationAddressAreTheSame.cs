namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using StreetName;
    using global::AutoFixture;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSourceAndDestinationAddressAreTheSame : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenSourceAndDestinationAddressAreTheSame(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThrowsSourceAndDestinationAddressAreTheSameException()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationHouseNumber = new HouseNumber("11");

            var houseNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, addressPersistentLocalId , destinationHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddress)
                .When(command)
                .Throws(new SourceAndDestinationAddressAreTheSameException(addressPersistentLocalId, destinationHouseNumber)));
        }
    }
}
