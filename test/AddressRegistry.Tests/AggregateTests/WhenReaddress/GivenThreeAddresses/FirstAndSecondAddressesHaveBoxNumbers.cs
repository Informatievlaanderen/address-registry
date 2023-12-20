namespace AddressRegistry.Tests.AggregateTests.WhenReaddress.GivenThreeAddresses
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventBuilders;
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
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var firstAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var firstAddressCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);

            var secondAddressPersistentLocalId = new AddressPersistentLocalId(103);
            var secondAddressCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(104);
            var secondAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(105);

            var expectedSecondAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedThirdAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedThirdAddressCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var expectedThirdAddressProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(4);

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

            var firstAddressProposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
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
                .WithAddressPersistentLocalId(secondAddressCurrentBoxNumberAddressPersistentLocalId)
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

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    firstAddressProposedBoxNumberAddressWasMigrated,
                    firstAddressCurrentBoxNumberAddressWasMigrated,
                    secondAddressWasMigrated,
                    secondAddressCurrentBoxNumberAddressWasMigrated,
                    secondAddressProposedBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedSecondAddressProposedBoxNumberAddressPersistentLocalId,
                            firstAddressProposedBoxNumberAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            new PostalCode(firstAddressWasMigrated.PostalCode!),
                            secondHouseNumber13,
                            new BoxNumber(firstAddressProposedBoxNumberAddressWasMigrated.BoxNumber!),
                            firstAddressProposedBoxNumberAddressWasMigrated.GeometryMethod,
                            firstAddressProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(firstAddressProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
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
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedThirdAddressCurrentBoxNumberAddressPersistentLocalId,
                            secondAddressCurrentBoxNumberAddressPersistentLocalId,
                            expectedThirdAddressPersistentLocalId,
                            postalCode,
                            thirstHouseNumber15,
                            new BoxNumber(secondAddressCurrentBoxNumberAddressWasMigrated.BoxNumber!),
                            secondAddressCurrentBoxNumberAddressWasMigrated.GeometryMethod,
                            secondAddressCurrentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressCurrentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedThirdAddressProposedBoxNumberAddressPersistentLocalId,
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
                                    firstAddressProposedBoxNumberAddressWasMigrated.Status,
                                    secondHouseNumber13,
                                    new BoxNumber(firstAddressProposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(firstAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        firstAddressProposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        firstAddressProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(firstAddressProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    firstAddressProposedBoxNumberAddressWasMigrated.OfficiallyAssigned),
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
                            })),
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
                                    secondAddressCurrentBoxNumberAddressPersistentLocalId,
                                    expectedThirdAddressCurrentBoxNumberAddressPersistentLocalId,
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
                                    expectedThirdAddressProposedBoxNumberAddressPersistentLocalId,
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
                            })),
                    new Fact(_streamId,
                    new AddressWasRetiredBecauseOfReaddress(
                        _streetNamePersistentLocalId,
                        secondAddressCurrentBoxNumberAddressPersistentLocalId)),
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
                && x.addressPersistentLocalId == expectedThirdAddressCurrentBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdAddressProposedBoxNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == secondAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedThirdAddressPersistentLocalId);
        }

        [Fact]
        public void WithSecondAddressOtherBoxNumbers_ThenDifferingBoxNumbersWereProposedAndRejectedOrRetired()
        {
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(40001932);

            var secondAddressPersistentLocalId = new AddressPersistentLocalId(40003037);
            var secondAddressProposedBoxNumberPersistentLocalId = new AddressPersistentLocalId(40003043);
            var secondAddressCurrentBoxNumberPersistentLocalId = new AddressPersistentLocalId(40003044);

            var thirdAddressPersistentLocalId = new AddressPersistentLocalId(40003039);
            var thirdAddressBoxNumberPersistentLocalId = new AddressPersistentLocalId(40003046);

            var firstHouseNumber = new HouseNumber("3");
            var secondHouseNumber = new HouseNumber("6");
            var thirdHouseNumber = new HouseNumber("7");

            var postalCode = Fixture.Create<PostalCode>();

            var firstAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(firstAddressPersistentLocalId)
                .WithHouseNumber(firstHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.ThirdGmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var secondAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(secondAddressPersistentLocalId)
                .WithHouseNumber(secondHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var secondProposedBoxNumberAddressWasMigrated =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                    .WithAddressPersistentLocalId(secondAddressProposedBoxNumberPersistentLocalId)
                    .WithHouseNumber(secondHouseNumber)
                    .WithBoxNumber(new BoxNumber("A"), secondAddressPersistentLocalId)
                    .WithAddressGeometry(new AddressGeometry(
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                    .Build();

            var secondCurrentBoxNumberAddressWasMigrated =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                    .WithAddressPersistentLocalId(secondAddressCurrentBoxNumberPersistentLocalId)
                    .WithHouseNumber(secondHouseNumber)
                    .WithBoxNumber(new BoxNumber("B"), secondAddressPersistentLocalId)
                    .WithAddressGeometry(new AddressGeometry(
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                    .Build();

            var thirdAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(thirdAddressPersistentLocalId)
                .WithHouseNumber(thirdHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var thirdBoxNumberAddressWasMigrated =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                    .WithAddressPersistentLocalId(thirdAddressBoxNumberPersistentLocalId)
                    .WithHouseNumber(thirdHouseNumber)
                    .WithBoxNumber(new BoxNumber("A"), thirdAddressPersistentLocalId)
                    .WithAddressGeometry(new AddressGeometry(
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                    .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, secondAddressPersistentLocalId,
                        firstHouseNumber),
                    new ReaddressAddressItem(_streetNamePersistentLocalId, thirdAddressPersistentLocalId,
                        secondHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            var expectedProposedBoxNumberAddressPersistentLocalId =
                new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    secondAddressWasMigrated,
                    secondProposedBoxNumberAddressWasMigrated,
                    secondCurrentBoxNumberAddressWasMigrated,
                    thirdAddressWasMigrated,
                    thirdBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            secondAddressProposedBoxNumberPersistentLocalId,
                            firstAddressPersistentLocalId,
                            new PostalCode(secondAddressWasMigrated.PostalCode!),
                            firstHouseNumber,
                            new BoxNumber(secondProposedBoxNumberAddressWasMigrated.BoxNumber!),
                            secondProposedBoxNumberAddressWasMigrated.GeometryMethod,
                            secondProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            secondAddressCurrentBoxNumberPersistentLocalId,
                            firstAddressPersistentLocalId,
                            new PostalCode(secondAddressWasMigrated.PostalCode!),
                            firstHouseNumber,
                            new BoxNumber(secondCurrentBoxNumberAddressWasMigrated.BoxNumber!),
                            secondCurrentBoxNumberAddressWasMigrated.GeometryMethod,
                            secondCurrentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondCurrentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            firstAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                secondAddressPersistentLocalId,
                                firstAddressPersistentLocalId,
                                isDestinationNewlyProposed: false,
                                secondAddressWasMigrated.Status,
                                firstHouseNumber,
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
                                    secondAddressProposedBoxNumberPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    secondProposedBoxNumberAddressWasMigrated.Status,
                                    firstHouseNumber,
                                    new BoxNumber(secondProposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(secondAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        secondProposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        secondProposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(secondProposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    secondProposedBoxNumberAddressWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    secondAddressCurrentBoxNumberPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    secondCurrentBoxNumberAddressWasMigrated.Status,
                                    firstHouseNumber,
                                    new BoxNumber(secondCurrentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(secondAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        secondCurrentBoxNumberAddressWasMigrated.GeometryMethod,
                                        secondCurrentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(secondCurrentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    secondCurrentBoxNumberAddressWasMigrated.OfficiallyAssigned)
                            })),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            secondAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                thirdAddressPersistentLocalId,
                                secondAddressPersistentLocalId,
                                isDestinationNewlyProposed: false,
                                thirdAddressWasMigrated.Status,
                                secondHouseNumber,
                                boxNumber: null,
                                new PostalCode(thirdAddressWasMigrated.PostalCode!),
                                new AddressGeometry(
                                    thirdAddressWasMigrated.GeometryMethod,
                                    thirdAddressWasMigrated.GeometrySpecification,
                                    new ExtendedWkbGeometry(thirdAddressWasMigrated.ExtendedWkbGeometry)),
                                thirdAddressWasMigrated.OfficiallyAssigned),
                            readdressedBoxNumbers: new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    thirdAddressBoxNumberPersistentLocalId,
                                    secondAddressProposedBoxNumberPersistentLocalId,
                                    isDestinationNewlyProposed: false,
                                    thirdBoxNumberAddressWasMigrated.Status,
                                    secondHouseNumber,
                                    new BoxNumber(thirdBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(thirdAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        thirdBoxNumberAddressWasMigrated.GeometryMethod,
                                        thirdBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(thirdBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    thirdBoxNumberAddressWasMigrated.OfficiallyAssigned)
                            })),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            secondAddressCurrentBoxNumberPersistentLocalId))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(2);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedCurrentBoxNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == secondAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == firstAddressPersistentLocalId);
        }
    }
}
