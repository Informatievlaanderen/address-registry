namespace AddressRegistry.Tests.AggregateTests.WhenRejectingOrRetiringForReaddressing
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsInactive : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenAddressIsInactive(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void ByStatus_ThenNone(AddressStatus status)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var addressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, status)
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .Build();

            var command = new RejectOrRetireAddressForReaddressing(
                _streetNamePersistentLocalId,
                new StreetNamePersistentLocalId(_streetNamePersistentLocalId + 1),
                addressPersistentLocalId,
                new AddressPersistentLocalId(addressPersistentLocalId + 1),
                new List<BoxNumberAddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData(AddressStatus.Proposed)]
        [InlineData(AddressStatus.Current)]
        public void WithIsRemoved_ThenNone(AddressStatus status)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var addressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, status)
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithIsRemoved()
                .Build();

            var command = new RejectOrRetireAddressForReaddressing(
                _streetNamePersistentLocalId,
                new StreetNamePersistentLocalId(_streetNamePersistentLocalId + 1),
                addressPersistentLocalId,
                new AddressPersistentLocalId(addressPersistentLocalId + 1),
                new List<BoxNumberAddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .ThenNone());
        }
    }
}
