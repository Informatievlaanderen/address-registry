namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing.GivenThreeAddresses
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

    public class FirstAndSecondAddressesHaveBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public FirstAndSecondAddressesHaveBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenBoxNumbersAreReadressed()
        {
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var firstAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var firstAddressCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(12);

            var secondAddressPersistentLocalId = new AddressPersistentLocalId(13);
            var secondCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(14);
            var secondAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(15);

            var firstHouseNumber = new HouseNumber("11");
            var secondHouseNumber13 = new HouseNumber("13");
            var thirstHouseNumber15 = new HouseNumber("15");

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

            var firstProposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(firstAddressProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(firstHouseNumber)
                .WithBoxNumber(new BoxNumber("A"), firstAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var firstAddressCurrentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(firstAddressCurrentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(firstHouseNumber)
                .WithBoxNumber(new BoxNumber("B"), firstAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
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

            var secondAddressCurrentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(secondCurrentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber13)
                .WithBoxNumber(new BoxNumber("A1"), secondAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var secondAddressProposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(secondAddressProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber13)
                .WithBoxNumber(new BoxNumber("B"), secondAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, firstAddressPersistentLocalId , secondHouseNumber13),
                    new ReaddressAddressItem(_streetNamePersistentLocalId, secondAddressPersistentLocalId, thirstHouseNumber15)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            var expectedSecondAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedThirdAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedThirdCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var expectedThirdProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(4);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    firstProposedBoxNumberAddressWasMigrated,
                    firstAddressCurrentBoxNumberAddressWasMigrated,
                    secondAddressWasMigrated,
                    secondAddressCurrentBoxNumberAddressWasMigrated,
                    secondAddressProposedBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddressing(
                            _streetNamePersistentLocalId,
                            expectedSecondAddressProposedBoxNumberAddressPersistentLocalId,
                            firstAddressProposedBoxNumberAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            new PostalCode(firstAddressWasMigrated.PostalCode!),
                            secondHouseNumber13,
                            new BoxNumber(firstProposedBoxNumberAddressWasMigrated.BoxNumber!),
                            firstProposedBoxNumberAddressWasMigrated.GeometryMethod,
                            firstProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(firstProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddressing(
                            _streetNamePersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            null,
                            postalCode,
                            thirstHouseNumber15,
                            null,
                            secondAddressWasMigrated.GeometryMethod,
                            secondAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddressing(
                            _streetNamePersistentLocalId,
                            expectedThirdCurrentBoxNumberAddressPersistentLocalId,
                            secondCurrentBoxNumberAddressPersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            postalCode,
                            thirstHouseNumber15,
                            new BoxNumber(secondAddressCurrentBoxNumberAddressWasMigrated.BoxNumber!),
                            secondAddressCurrentBoxNumberAddressWasMigrated.GeometryMethod,
                            secondAddressCurrentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressCurrentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(_streetNamePersistentLocalId,
                            secondCurrentBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddressing(
                            _streetNamePersistentLocalId,
                            expectedThirdProposedBoxNumberAddressPersistentLocalId,
                            secondAddressProposedBoxNumberAddressPersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            postalCode,
                            thirstHouseNumber15,
                            new BoxNumber(secondAddressProposedBoxNumberAddressWasMigrated.BoxNumber!),
                            secondAddressProposedBoxNumberAddressWasMigrated.GeometryMethod,
                            secondAddressProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
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
                            readdressedBoxNumbers: new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    firstAddressProposedBoxNumberAddressPersistentLocalId,
                                    expectedSecondAddressProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    firstProposedBoxNumberAddressWasMigrated.Status,
                                    secondHouseNumber13,
                                    new BoxNumber(firstProposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        firstProposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        firstProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(firstProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    firstProposedBoxNumberAddressWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    firstAddressCurrentBoxNumberAddressPersistentLocalId,
                                    secondAddressProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: false,
                                    firstAddressCurrentBoxNumberAddressWasMigrated.Status,
                                    secondHouseNumber13,
                                    new BoxNumber(firstAddressCurrentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(secondAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        firstAddressCurrentBoxNumberAddressWasMigrated.GeometryMethod,
                                        firstAddressCurrentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(firstAddressCurrentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    firstAddressCurrentBoxNumberAddressWasMigrated.OfficiallyAssigned),
                            },
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId> { secondCurrentBoxNumberAddressPersistentLocalId })),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                secondAddressPersistentLocalId,
                                expectedThirdAddressPersistentLocalId,
                                isDestinationNewlyProposed: true,
                                secondAddressWasMigrated.Status,
                                thirstHouseNumber15,
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
                                    secondCurrentBoxNumberAddressPersistentLocalId,
                                    expectedThirdCurrentBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    secondAddressCurrentBoxNumberAddressWasMigrated.Status,
                                    thirstHouseNumber15,
                                    new BoxNumber(secondAddressCurrentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(secondAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        secondAddressCurrentBoxNumberAddressWasMigrated.GeometryMethod,
                                        secondAddressCurrentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(secondAddressCurrentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    secondAddressCurrentBoxNumberAddressWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    secondAddressProposedBoxNumberAddressPersistentLocalId,
                                    expectedThirdProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    secondAddressProposedBoxNumberAddressWasMigrated.Status,
                                    thirstHouseNumber15,
                                    new BoxNumber(secondAddressProposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(secondAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        secondAddressProposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        secondAddressProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(secondAddressProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    secondAddressProposedBoxNumberAddressWasMigrated.OfficiallyAssigned)
                            },
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>()))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(4);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedSecondAddressProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdCurrentBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdProposedBoxNumberAddressPersistentLocalId);

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
