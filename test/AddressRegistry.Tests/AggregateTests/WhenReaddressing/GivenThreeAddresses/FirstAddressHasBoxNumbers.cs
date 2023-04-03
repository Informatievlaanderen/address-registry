﻿namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing.GivenThreeAddresses
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

    public class FirstAddressHasBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public FirstAddressHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenTheBoxNumbersAreProposedOnTheSecondAddress()
        {
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(12);

            var secondAddressPersistentLocalId = new AddressPersistentLocalId(456);

            var firstHouseNumber = new HouseNumber("11");
            var secondHouseNumber = new HouseNumber("13");
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

            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(firstHouseNumber)
                .WithBoxNumber(new BoxNumber("A"), firstAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(firstHouseNumber)
                .WithBoxNumber(new BoxNumber("B"), firstAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var secondAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(secondAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, firstAddressPersistentLocalId , secondHouseNumber),
                    new ReaddressAddressItem(_streetNamePersistentLocalId, secondAddressPersistentLocalId, thirdHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedThirdAddressPersistentLocalId = new AddressPersistentLocalId(3);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated,
                    secondAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            new PostalCode(firstAddressWasMigrated.PostalCode!),
                            secondHouseNumber,
                            new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                            proposedBoxNumberAddressWasMigrated.GeometryMethod,
                            proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            new PostalCode(firstAddressWasMigrated.PostalCode!),
                            secondHouseNumber,
                            new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                            currentBoxNumberAddressWasMigrated.GeometryMethod,
                            currentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(_streetNamePersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            null,
                            postalCode,
                            thirdHouseNumber,
                            null,
                            secondAddressWasMigrated.GeometryMethod,
                            secondAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                expectedProposedBoxNumberAddressPersistentLocalId,
                                expectedCurrentBoxNumberAddressPersistentLocalId,
                                expectedThirdAddressPersistentLocalId
                            },
                            new List<AddressPersistentLocalId>(),
                            new List<AddressPersistentLocalId>(),
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    firstAddressPersistentLocalId,
                                    secondAddressPersistentLocalId,
                                    firstAddressWasMigrated.Status,
                                    secondHouseNumber,
                                    boxNumber: null,
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        firstAddressWasMigrated.GeometryMethod,
                                        firstAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(firstAddressWasMigrated.ExtendedWkbGeometry)),
                                    firstAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null),
                                new ReaddressedAddressData(
                                    proposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    proposedBoxNumberAddressWasMigrated.Status,
                                    secondHouseNumber,
                                    new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        proposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    proposedBoxNumberAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: secondAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    currentBoxNumberAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    currentBoxNumberAddressWasMigrated.Status,
                                    secondHouseNumber,
                                    new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        currentBoxNumberAddressWasMigrated.GeometryMethod,
                                        currentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    currentBoxNumberAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: secondAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    secondAddressPersistentLocalId,
                                    expectedThirdAddressPersistentLocalId,
                                    secondAddressWasMigrated.Status,
                                    thirdHouseNumber,
                                    boxNumber: null,
                                    new PostalCode(secondAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        secondAddressWasMigrated.GeometryMethod,
                                        secondAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(secondAddressWasMigrated.ExtendedWkbGeometry)),
                                    secondAddressWasMigrated.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null)
                            }))
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
