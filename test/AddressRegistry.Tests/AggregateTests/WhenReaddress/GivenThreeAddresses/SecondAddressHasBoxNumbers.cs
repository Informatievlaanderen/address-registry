namespace AddressRegistry.Tests.AggregateTests.WhenReaddress.GivenThreeAddresses
{
    using System.Collections.Generic;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AddressRegistry.StreetName.DataStructures;
    using AddressRegistry.StreetName.Events;
    using AddressRegistry.Tests.AggregateTests.Builders;
    using AddressRegistry.Tests.AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class SecondAddressHasBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public SecondAddressHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenTheBoxNumbersAreInactivedOnTheSecondAddressAndTheBoxNumbersAreProposedOnTheThirdAddress()
        {
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var secondAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(103);

            var expectedThirdAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var firstHouseNumber = new HouseNumber("11");
            var secondHouseNumber13 = new HouseNumber("13");
            var thirdHouseNumber = new HouseNumber("15");

            var postalCode = Fixture.Create<PostalCode>();

            var firstAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(firstAddressPersistentLocalId)
                .WithHouseNumber(firstHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var secondAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(secondAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber13)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber13)
                .WithBoxNumber(new BoxNumber("A"), secondAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber13)
                .WithBoxNumber(new BoxNumber("B"), secondAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, firstAddressPersistentLocalId , secondHouseNumber13),
                    new ReaddressAddressItem(_streetNamePersistentLocalId, secondAddressPersistentLocalId, thirdHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    secondAddressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            null,
                            postalCode,
                            thirdHouseNumber,
                            null,
                            secondAddressWasMigrated.GeometryMethod,
                            secondAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            proposedBoxNumberAddressPersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            new PostalCode(firstAddressWasMigrated.PostalCode!),
                            thirdHouseNumber,
                            new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                            proposedBoxNumberAddressWasMigrated.GeometryMethod,
                            proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            currentBoxNumberAddressPersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            new PostalCode(firstAddressWasMigrated.PostalCode!),
                            thirdHouseNumber,
                            new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                            currentBoxNumberAddressWasMigrated.GeometryMethod,
                            currentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            secondAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                firstAddressPersistentLocalId,
                                secondAddressPersistentLocalId,
                                isDestinationNewlyProposed: false,
                                firstAddressWasMigrated.Status,
                                secondHouseNumber13,
                                boxNumber: null,
                                new PostalCode(firstAddressWasMigrated.PostalCode!),
                                new AddressGeometry(
                                    firstAddressWasMigrated.GeometryMethod,
                                    firstAddressWasMigrated.GeometrySpecification,
                                    new ExtendedWkbGeometry(firstAddressWasMigrated.ExtendedWkbGeometry)),
                                firstAddressWasMigrated.OfficiallyAssigned),
                            readdressedBoxNumbers: new List<ReaddressedAddressData>())),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                secondAddressPersistentLocalId,
                                expectedThirdAddressPersistentLocalId,
                                isDestinationNewlyProposed: true,
                                secondAddressWasMigrated.Status,
                                thirdHouseNumber,
                                boxNumber: null,
                                new PostalCode(secondAddressWasMigrated.PostalCode!),
                                new AddressGeometry(
                                    secondAddressWasMigrated.GeometryMethod,
                                    secondAddressWasMigrated.GeometrySpecification,
                                    new ExtendedWkbGeometry(secondAddressWasMigrated.ExtendedWkbGeometry)),
                                secondAddressWasMigrated.OfficiallyAssigned),
                            readdressedBoxNumbers: new List<ReaddressedAddressData>
                            {
                                 new ReaddressedAddressData(
                                    proposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    proposedBoxNumberAddressWasMigrated.Status,
                                    thirdHouseNumber,
                                    new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        proposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    proposedBoxNumberAddressWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    currentBoxNumberAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    currentBoxNumberAddressWasMigrated.Status,
                                    thirdHouseNumber,
                                    new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        currentBoxNumberAddressWasMigrated.GeometryMethod,
                                        currentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    currentBoxNumberAddressWasMigrated.OfficiallyAssigned)
                            })),
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            proposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            currentBoxNumberAddressPersistentLocalId))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(3);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedCurrentBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == secondAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdAddressPersistentLocalId);
        }
    }
}
