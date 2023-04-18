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

    public class SourceAddressHasBoxNumbers_DestinationHasNoBoxNumbers: AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public SourceAddressHasBoxNumbers_DestinationHasNoBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenSourceAddressWasReaddressedAndBoxNumbersWereProposed()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var currentBoxNumberAddressAddressPersistentLocalId = new AddressPersistentLocalId(102);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(103);

            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");
            var postalCode = Fixture.Create<PostalCode>();

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithBoxNumber(new BoxNumber("A"), sourceAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithBoxNumber(new BoxNumber("B"), sourceAddressPersistentLocalId)
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

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    sourceAddressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated,
                    destinationAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            proposedBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                            proposedBoxNumberAddressWasMigrated.GeometryMethod,
                            proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            currentBoxNumberAddressAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                            currentBoxNumberAddressWasMigrated.GeometryMethod,
                            currentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
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
                            readdressedBoxNumbers: new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    proposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    proposedBoxNumberAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber),
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        proposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    proposedBoxNumberAddressWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    currentBoxNumberAddressAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    currentBoxNumberAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber),
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        currentBoxNumberAddressWasMigrated.GeometryMethod,
                                        currentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    currentBoxNumberAddressWasMigrated.OfficiallyAssigned)
                            },
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>()))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(2);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedCurrentBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
