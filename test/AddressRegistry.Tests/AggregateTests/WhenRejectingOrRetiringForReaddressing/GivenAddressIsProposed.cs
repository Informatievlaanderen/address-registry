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

    public class GivenAddressIsProposed : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenAddressIsProposed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenAddressIsRejected()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var addressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .Build();

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(_streetNamePersistentLocalId + 1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId + 1);

            var command = new RejectOrRetireAddressForReaddress(
                _streetNamePersistentLocalId,
                destinationStreetNamePersistentLocalId,
                addressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new List<BoxNumberAddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                           addressPersistentLocalId))
                }));
        }
    }
}
