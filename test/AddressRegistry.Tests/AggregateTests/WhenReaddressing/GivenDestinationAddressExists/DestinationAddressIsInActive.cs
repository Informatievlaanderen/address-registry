namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing.GivenDestinationAddressExists
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class DestinationAddressIsInActive: AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public DestinationAddressIsInActive(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void ThenDestinationAddressWasProposed(AddressStatus addressStatus)
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1

            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var destinationAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, addressStatus)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(11))
                .WithHouseNumber(destinationHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(_streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(), Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>(),
                    sourceAddressWasMigrated,
                    destinationAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddressing(_streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            null,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            null,
                            sourceAddressWasMigrated.GeometryMethod,
                            sourceAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                sourceAddressPersistentLocalId,
                                destinationAddressPersistentLocalId,
                                isDestinationNewlyProposed: true,
                                sourceAddressWasMigrated.Status,
                                destinationHouseNumber,
                                boxNumber: null,
                                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                new AddressGeometry(
                                    sourceAddressWasMigrated.GeometryMethod,
                                    sourceAddressWasMigrated.GeometrySpecification,
                                    new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry)),
                                sourceAddressWasMigrated.OfficiallyAssigned),
                            readdressedBoxNumbers: new List<ReaddressedAddressData>(),
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>()))
                }));

            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
