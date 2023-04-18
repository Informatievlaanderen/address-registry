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

    public class SourceAddressHasNoBoxNumbers_DestinationHasBoxNumbers: AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public SourceAddressHasNoBoxNumbers_DestinationHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenDestinationBoxNumbersWereRejectedOrRetired()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);
            var currentBoxNumberAddressAddressPersistentLocalId = new AddressPersistentLocalId(103);

            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");
            var postalCode = Fixture.Create<PostalCode>();

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var destinationAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithBoxNumber(new BoxNumber("A"), destinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithBoxNumber(new BoxNumber("B"), destinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem> { new RetireAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId) },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    sourceAddressWasMigrated,
                    destinationAddressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            proposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            currentBoxNumberAddressAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            sourceAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
                                    destinationAddressPersistentLocalId,
                                    isDestinationNewlyProposed: false,
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
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId> { proposedBoxNumberAddressPersistentLocalId },
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId> { currentBoxNumberAddressAddressPersistentLocalId }))
                }));

            command.ExecutionContext.AddressesAdded.Should().BeEmpty();

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == sourceAddressPersistentLocalId);
        }
    }
}
