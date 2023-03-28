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

    public class GivenSourceAddressIsNotActive : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenSourceAddressIsNotActive(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void ThrowsAddressHasInvalidStatusException(AddressStatus status)
        {
            var houseNumber = new HouseNumber("11");

            var houseNumberAddressPersistentLocalId = new AddressPersistentLocalId(123);

            var houseNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, status)
                .WithAddressPersistentLocalId(houseNumberAddressPersistentLocalId)
                .WithHouseNumber(houseNumber)
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, houseNumberAddressPersistentLocalId , houseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddress)
                .When(command)
                .Throws(new AddressHasInvalidStatusException(houseNumberAddressPersistentLocalId)));
        }
    }
}
