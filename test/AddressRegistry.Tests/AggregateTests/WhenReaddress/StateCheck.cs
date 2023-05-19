namespace AddressRegistry.Tests.AggregateTests.WhenReaddress
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class StateCheck : AddressRegistryTest
    {
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public StateCheck(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenValidState()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var sourceAddressFirstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var sourceAddressSecondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressFirstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationAddressSecondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var postalCode = Fixture.Create<PostalCode>();
            var houseNumberEleven = new HouseNumber("11");
            var houseNumberThirteen = new HouseNumber("13");

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(houseNumberEleven)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var sourceAddressFirstBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceAddressFirstBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(houseNumberEleven)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A1"), sourceAddressPersistentLocalId)
                .Build();

            var sourceAddressSecondBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(sourceAddressSecondBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(houseNumberEleven)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A2"), sourceAddressPersistentLocalId)
                .Build();

            var destinationHouseNumberAddressWasProposed = new AddressWasProposedBecauseOfReaddress(
                _streetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                sourceAddressPersistentLocalId,
                null,
                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                houseNumberThirteen,
                null,
                sourceAddressWasMigrated.GeometryMethod,
                sourceAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry));

            var destinationFirstBoxNumberWasProposed = new AddressWasProposedBecauseOfReaddress(
                _streetNamePersistentLocalId,
                destinationAddressFirstBoxNumberAddressPersistentLocalId,
                sourceAddressFirstBoxNumberAddressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                houseNumberThirteen,
                new BoxNumber(sourceAddressFirstBoxNumberAddressWasMigrated.BoxNumber!),
                sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod,
                sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry));

            var destinationSecondBoxNumberWasProposed = new AddressWasProposedBecauseOfReaddress(
                _streetNamePersistentLocalId,
                destinationAddressSecondBoxNumberAddressPersistentLocalId,
                sourceAddressSecondBoxNumberAddressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                houseNumberThirteen,
                new BoxNumber(sourceAddressSecondBoxNumberAddressWasMigrated.BoxNumber!),
                sourceAddressSecondBoxNumberAddressWasMigrated.GeometryMethod,
                sourceAddressSecondBoxNumberAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressSecondBoxNumberAddressWasMigrated.ExtendedWkbGeometry));

            var sourceAddressWasRejected = new AddressWasRejectedBecauseOfReaddress(
                _streetNamePersistentLocalId,
                sourceAddressPersistentLocalId);

            var sourceAddressFirstBoxNumberWasRejected = new AddressWasRejectedBecauseOfReaddress(
                _streetNamePersistentLocalId,
                sourceAddressFirstBoxNumberAddressPersistentLocalId);

            var sourceAddressSecondBoxNumberWasRetired = new AddressWasRetiredBecauseOfReaddress(
                _streetNamePersistentLocalId,
                sourceAddressSecondBoxNumberAddressPersistentLocalId);

            var addressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
                _streetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                readdressedHouseNumber: new ReaddressedAddressData(
                    sourceAddressPersistentLocalId,
                    destinationAddressPersistentLocalId,
                    isDestinationNewlyProposed: true,
                    sourceAddressWasMigrated.Status,
                    houseNumberThirteen,
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
                            sourceAddressFirstBoxNumberAddressPersistentLocalId,
                            destinationAddressFirstBoxNumberAddressPersistentLocalId,
                            isDestinationNewlyProposed: true,
                            sourceAddressFirstBoxNumberAddressWasMigrated.Status,
                            houseNumberThirteen,
                            new BoxNumber(sourceAddressFirstBoxNumberAddressWasMigrated.BoxNumber!),
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            new AddressGeometry(
                                sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod,
                                sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification,
                                new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                            sourceAddressFirstBoxNumberAddressWasMigrated.OfficiallyAssigned),
                        new ReaddressedAddressData(
                            sourceAddressSecondBoxNumberAddressPersistentLocalId,
                            destinationAddressSecondBoxNumberAddressPersistentLocalId,
                            isDestinationNewlyProposed: true,
                            sourceAddressSecondBoxNumberAddressWasMigrated.Status,
                            houseNumberThirteen,
                            new BoxNumber(sourceAddressSecondBoxNumberAddressWasMigrated.BoxNumber!),
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            new AddressGeometry(
                                sourceAddressSecondBoxNumberAddressWasMigrated.GeometryMethod,
                                sourceAddressSecondBoxNumberAddressWasMigrated.GeometrySpecification,
                                new ExtendedWkbGeometry(sourceAddressSecondBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                            sourceAddressSecondBoxNumberAddressWasMigrated.OfficiallyAssigned)
                });
            ((ISetProvenance)addressHouseNumberWasReaddressed).SetProvenance(Fixture.Create<Provenance>());

            var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            streetName.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                sourceAddressWasMigrated,
                sourceAddressFirstBoxNumberAddressWasMigrated,
                sourceAddressSecondBoxNumberAddressWasMigrated,
                destinationHouseNumberAddressWasProposed,
                destinationFirstBoxNumberWasProposed,
                destinationSecondBoxNumberWasProposed,
                sourceAddressWasRejected,
                sourceAddressFirstBoxNumberWasRejected,
                sourceAddressSecondBoxNumberWasRetired,
                addressHouseNumberWasReaddressed
            });

            var sourceAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == sourceAddressPersistentLocalId);
            sourceAddress.Should().NotBeNull();
            sourceAddress!.Status.Should().Be(AddressStatus.Rejected);

            var sourceAddressFirstBoxNumberAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == sourceAddressFirstBoxNumberAddressPersistentLocalId);
            sourceAddressFirstBoxNumberAddress.Should().NotBeNull();
            sourceAddressFirstBoxNumberAddress!.Status.Should().Be(AddressStatus.Rejected);

            var sourceAddressSecondBoxNumberAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == sourceAddressSecondBoxNumberAddressPersistentLocalId);
            sourceAddressSecondBoxNumberAddress.Should().NotBeNull();
            sourceAddressSecondBoxNumberAddress!.Status.Should().Be(AddressStatus.Retired);

            var destinationAddress = streetName.StreetNameAddresses.FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
            destinationAddress.Should().NotBeNull();
            destinationAddress!.HouseNumber.Should().Be(houseNumberThirteen);
            destinationAddress.Status.Should().Be(sourceAddressWasMigrated.Status);
            destinationAddress.Geometry.GeometryMethod.Should().Be(sourceAddressWasMigrated.GeometryMethod);
            destinationAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressWasMigrated.GeometrySpecification);
            destinationAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry));
            destinationAddress.PostalCode.Should().Be(new PostalCode(sourceAddressWasMigrated.PostalCode!));
            destinationAddress.IsOfficiallyAssigned.Should().Be(sourceAddressWasMigrated.OfficiallyAssigned);

            destinationAddress.Children.Should().HaveCount(2);
            var destinationAddressFirstBoxNumberAddress = destinationAddress.Children
                .SingleOrDefault(x => x.AddressPersistentLocalId == destinationAddressFirstBoxNumberAddressPersistentLocalId);
            destinationAddressFirstBoxNumberAddress.Should().NotBeNull();
            destinationAddressFirstBoxNumberAddress!.HouseNumber.Should().Be(houseNumberThirteen);
            destinationAddressFirstBoxNumberAddress.Status.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.Status);
            destinationAddressFirstBoxNumberAddress.Geometry.GeometryMethod.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod);
            destinationAddressFirstBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification);
            destinationAddressFirstBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
            destinationAddressFirstBoxNumberAddress.PostalCode.Should().Be(new PostalCode(sourceAddressFirstBoxNumberAddressWasMigrated.PostalCode!));
            destinationAddressFirstBoxNumberAddress.IsOfficiallyAssigned.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.OfficiallyAssigned);

            var destinationAddressSecondBoxNumberAddress = destinationAddress.Children
                .SingleOrDefault(x => x.AddressPersistentLocalId == destinationAddressSecondBoxNumberAddressPersistentLocalId);
            destinationAddressSecondBoxNumberAddress.Should().NotBeNull();
            destinationAddressSecondBoxNumberAddress!.HouseNumber.Should().Be(houseNumberThirteen);
            destinationAddressSecondBoxNumberAddress.Status.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.Status);
            destinationAddressSecondBoxNumberAddress.Geometry.GeometryMethod.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.GeometryMethod);
            destinationAddressSecondBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.GeometrySpecification);
            destinationAddressSecondBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressSecondBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
            destinationAddressSecondBoxNumberAddress.PostalCode.Should().Be(new PostalCode(sourceAddressSecondBoxNumberAddressWasMigrated.PostalCode!));
            destinationAddressSecondBoxNumberAddress.IsOfficiallyAssigned.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.OfficiallyAssigned);

            destinationAddress.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
            destinationAddressFirstBoxNumberAddress.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
            destinationAddressSecondBoxNumberAddress.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
        }
    }
}
