namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing
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

    public class GivenAddressIsSourceAndDestination : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenAddressIsSourceAndDestination(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        // 11 & 13 exists, 15 does not
        // 11 --> 13 --> 15
        public void ThenSourceAddressWasReaddressed()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var sourceAndDestinationAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var destinationHouseNumber13 = new HouseNumber("13");
            var destinationHouseNumber15 = new HouseNumber("15");

            var postalCode = Fixture.Create<PostalCode>();

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithPostalCode(postalCode)
                .Build();

            var migratedSourceAndDestinationAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(sourceAndDestinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber13)
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
                    new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber13),
                    new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAndDestinationAddressPersistentLocalId, destinationHouseNumber15)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            var expectedProposedAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with 1

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedSourceAndDestinationAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(_streetNamePersistentLocalId,
                            expectedProposedAddressPersistentLocalId,
                            null,
                            postalCode,
                            destinationHouseNumber15,
                            null,
                            migratedSourceAndDestinationAddress.GeometryMethod,
                            migratedSourceAndDestinationAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedSourceAndDestinationAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId> { expectedProposedAddressPersistentLocalId },
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
                                    sourceAndDestinationAddressPersistentLocalId,
                                    migratedSourceAddress.Status,
                                    destinationHouseNumber13,
                                    boxNumber: null,
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedSourceAddress.GeometryMethod,
                                        migratedSourceAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedSourceAddress.ExtendedWkbGeometry)),
                                    migratedSourceAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null),
                                new ReaddressedAddressData(
                                    sourceAndDestinationAddressPersistentLocalId,
                                    expectedProposedAddressPersistentLocalId,
                                    migratedSourceAndDestinationAddress.Status,
                                    destinationHouseNumber15,
                                    boxNumber: null,
                                    new PostalCode(migratedSourceAndDestinationAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedSourceAndDestinationAddress.GeometryMethod,
                                        migratedSourceAndDestinationAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedSourceAndDestinationAddress.ExtendedWkbGeometry)),
                                    migratedSourceAndDestinationAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().ContainSingle();
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == sourceAndDestinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedAddressPersistentLocalId);
        }
    }
}
