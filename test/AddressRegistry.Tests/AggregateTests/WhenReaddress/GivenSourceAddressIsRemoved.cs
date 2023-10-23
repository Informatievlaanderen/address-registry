namespace AddressRegistry.Tests.AggregateTests.WhenReaddress
{
    using System.Collections.Generic;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AddressRegistry.StreetName.Events;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventBuilders;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSourceAddressIsRemoved : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenSourceAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThrowsAddressIsRemovedException()
        {
            var houseNumberAddressPersistentLocalId = new AddressPersistentLocalId(123);

            var houseNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(houseNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithIsRemoved()
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, houseNumberAddressPersistentLocalId , new HouseNumber("11"))
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddress)
                .When(command)
                .Throws(new AddressIsRemovedException(houseNumberAddressPersistentLocalId)));
        }
    }
}
