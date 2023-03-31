namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing.GivenDestinationAddressDoesNotExist
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

    public class SourceAddressHasBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public SourceAddressHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenDestinationAddressHasBoxNumbers()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(789);

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

            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A1"), sourceAddressPersistentLocalId)
                .Build();

            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A2"), sourceAddressPersistentLocalId)
                .Build();

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var destinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    sourceAddressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            null,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            null,
                            sourceAddressWasMigrated.GeometryMethod,
                            sourceAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            destinationProposedBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                            proposedBoxNumberAddressWasMigrated.GeometryMethod,
                            proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            destinationCurrentBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                            currentBoxNumberAddressWasMigrated.GeometryMethod,
                            currentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                 destinationAddressPersistentLocalId,
                                 destinationProposedBoxNumberAddressPersistentLocalId,
                                 destinationCurrentBoxNumberAddressPersistentLocalId,
                            },
                            new List<AddressPersistentLocalId>(),
                            new List<AddressPersistentLocalId>(),
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
                                    destinationAddressPersistentLocalId,
                                    sourceAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    boxNumber: null,
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        sourceAddressWasMigrated.GeometryMethod,
                                        sourceAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry)),
                                    sourceAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null),
                                new ReaddressedAddressData(
                                    proposedBoxNumberAddressPersistentLocalId,
                                    destinationProposedBoxNumberAddressPersistentLocalId,
                                    proposedBoxNumberAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    boxNumber: new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        proposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    proposedBoxNumberAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: destinationAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    currentBoxNumberAddressPersistentLocalId,
                                    destinationCurrentBoxNumberAddressPersistentLocalId,
                                    currentBoxNumberAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    boxNumber: new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        currentBoxNumberAddressWasMigrated.GeometryMethod,
                                        currentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    currentBoxNumberAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: destinationAddressPersistentLocalId)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(3);
            command.ExecutionContext.AddressesAdded.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationCurrentBoxNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(1);
            command.ExecutionContext.AddressesUpdated.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
