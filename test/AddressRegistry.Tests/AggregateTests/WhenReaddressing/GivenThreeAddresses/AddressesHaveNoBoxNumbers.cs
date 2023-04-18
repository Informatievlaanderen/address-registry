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

    public class AddressesHaveNoBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public AddressesHaveNoBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenAllAddressesWereReaddressed()
        {
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(123);
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

            var thirdAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with 1

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    secondAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddressing(
                            _streetNamePersistentLocalId,
                            thirdAddressPersistentLocalId,
                            secondAddressPersistentLocalId,
                            null,
                            postalCode,
                            thirdHouseNumber,
                            null,
                            secondAddressWasMigrated.GeometryMethod,
                            secondAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(secondAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            secondAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                firstAddressPersistentLocalId,
                                secondAddressPersistentLocalId,
                                isDestinationNewlyProposed: false,
                                firstAddressWasMigrated.Status,
                                secondHouseNumber,
                                boxNumber: null,
                                new PostalCode(firstAddressWasMigrated.PostalCode!),
                                new AddressGeometry(
                                    firstAddressWasMigrated.GeometryMethod,
                                    firstAddressWasMigrated.GeometrySpecification,
                                    new ExtendedWkbGeometry(firstAddressWasMigrated.ExtendedWkbGeometry)),
                                firstAddressWasMigrated.OfficiallyAssigned),
                            readdressedBoxNumbers: new List<ReaddressedAddressData>(),
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>())),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            thirdAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                secondAddressPersistentLocalId,
                                thirdAddressPersistentLocalId,
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
                            readdressedBoxNumbers: new List<ReaddressedAddressData>(),
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>()))
                }));

            command.ExecutionContext.AddressesAdded.Should().ContainSingle();
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == thirdAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == secondAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == thirdAddressPersistentLocalId);
        }
    }
}
