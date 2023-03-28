namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using FluentAssertions;
    using StreetName;
    using global::AutoFixture;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenDestinationAddressDoesntExist : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenDestinationAddressDoesntExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenDestinationAddressWasProposed()
        {
            var migrateSourceAddressPersistentLocalId = new AddressPersistentLocalId(123);

            var migrateSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(migrateSourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator will always generate id 1
            var destinationHouseNumber = new HouseNumber("13");

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, migrateSourceAddressPersistentLocalId , destinationHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migrateSourceAddress)
                .When(command)
                .Then(new []
                {
                    new Fact(_streamId,
                    new AddressWasProposedV2(
                        _streetNamePersistentLocalId,
                        destinationAddressPersistentLocalId,
                        null,
                        new PostalCode(migrateSourceAddress.PostalCode!),
                        destinationHouseNumber,
                        null,
                        migrateSourceAddress.GeometryMethod,
                        migrateSourceAddress.GeometrySpecification,
                        new ExtendedWkbGeometry(migrateSourceAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                 destinationAddressPersistentLocalId
                            },
                            new List<ReaddressAddressData>
                            {
                                new ReaddressAddressData(
                                    migrateSourceAddressPersistentLocalId,
                                    destinationAddressPersistentLocalId,
                                    migrateSourceAddress.Status,
                                    destinationHouseNumber,
                                    new PostalCode(migrateSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migrateSourceAddress.GeometryMethod,
                                        migrateSourceAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migrateSourceAddress.ExtendedWkbGeometry)),
                                    migrateSourceAddress.OfficiallyAssigned)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().BeEmpty();
        }

        [Fact]
        public void StateCheck()
        {
            var migrateSourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var sourceAddressWasMigratedToStreetName = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(migrateSourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator will always generate id 1
            var destinationHouseNumber = new HouseNumber("13");

            var destinationAddressWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                null,
                new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                destinationHouseNumber,
                null,
                sourceAddressWasMigratedToStreetName.GeometryMethod,
                sourceAddressWasMigratedToStreetName.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressWasMigratedToStreetName.ExtendedWkbGeometry));

            var streetNameWasReaddressed = new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                new List<AddressPersistentLocalId>
                {
                    destinationAddressPersistentLocalId
                },
                new List<ReaddressAddressData>
                {
                    new ReaddressAddressData(
                        migrateSourceAddressPersistentLocalId,
                        destinationAddressPersistentLocalId,
                        sourceAddressWasMigratedToStreetName.Status,
                        destinationHouseNumber,
                        new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                        new AddressGeometry(
                            sourceAddressWasMigratedToStreetName.GeometryMethod,
                            sourceAddressWasMigratedToStreetName.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressWasMigratedToStreetName.ExtendedWkbGeometry)),
                        sourceAddressWasMigratedToStreetName.OfficiallyAssigned)
                });

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                sourceAddressWasMigratedToStreetName,
                destinationAddressWasProposed,
                streetNameWasReaddressed
            });

            var destinationAddress = sut.StreetNameAddresses.FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
            destinationAddress.Should().NotBeNull();
            destinationAddress.HouseNumber.Should().Be(destinationHouseNumber);
            destinationAddress.Status.Should().Be(sourceAddressWasMigratedToStreetName.Status);
            destinationAddress.Geometry.GeometryMethod.Should().Be(sourceAddressWasMigratedToStreetName.GeometryMethod);
            destinationAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressWasMigratedToStreetName.GeometrySpecification);
            destinationAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressWasMigratedToStreetName.ExtendedWkbGeometry));
            destinationAddress.PostalCode.Should().Be(new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode));
            destinationAddress.IsOfficiallyAssigned.Should()
                .Be(sourceAddressWasMigratedToStreetName.OfficiallyAssigned);
        }
    }
}
