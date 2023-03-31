namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing.GivenDestinationAddressDoesNotExist
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
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
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(789);
            var postalCode = Fixture.Create<PostalCode>();

            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var firstBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A1"), sourceAddressPersistentLocalId)
                .Build();

            var secondBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A2"), sourceAddressPersistentLocalId)
                .Build();

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationFirstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationSecondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var destinationHouseNumberAddressWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                null,
                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                destinationHouseNumber,
                null,
                sourceAddressWasMigrated.GeometryMethod,
                sourceAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry));

            var destinationFirstBoxNumberWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationFirstBoxNumberAddressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                destinationHouseNumber,
                new BoxNumber(firstBoxNumberAddressWasMigrated.BoxNumber!),
                firstBoxNumberAddressWasMigrated.GeometryMethod,
                firstBoxNumberAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(firstBoxNumberAddressWasMigrated.ExtendedWkbGeometry));

            var destinationSecondBoxNumberWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationSecondBoxNumberAddressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                destinationHouseNumber,
                new BoxNumber(secondBoxNumberAddressWasMigrated.BoxNumber!),
                secondBoxNumberAddressWasMigrated.GeometryMethod,
                secondBoxNumberAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(secondBoxNumberAddressWasMigrated.ExtendedWkbGeometry));

            var streetNameWasReaddressed = new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                new List<AddressPersistentLocalId>
                {
                    destinationAddressPersistentLocalId,
                    destinationFirstBoxNumberAddressPersistentLocalId,
                    destinationSecondBoxNumberAddressPersistentLocalId
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
                            destinationFirstBoxNumberAddressPersistentLocalId,
                            firstBoxNumberAddressWasMigrated.Status,
                            destinationHouseNumber,
                            boxNumber: new BoxNumber(firstBoxNumberAddressWasMigrated.BoxNumber!),
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            new AddressGeometry(
                                firstBoxNumberAddressWasMigrated.GeometryMethod,
                                firstBoxNumberAddressWasMigrated.GeometrySpecification,
                                new ExtendedWkbGeometry(firstBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                            firstBoxNumberAddressWasMigrated.OfficiallyAssigned,
                            parentAddressPersistentLocalId: destinationAddressPersistentLocalId),
                        new ReaddressedAddressData(
                            currentBoxNumberAddressPersistentLocalId,
                            destinationSecondBoxNumberAddressPersistentLocalId,
                            secondBoxNumberAddressWasMigrated.Status,
                            destinationHouseNumber,
                            boxNumber: new BoxNumber(secondBoxNumberAddressWasMigrated.BoxNumber!),
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            new AddressGeometry(
                                secondBoxNumberAddressWasMigrated.GeometryMethod,
                                secondBoxNumberAddressWasMigrated.GeometrySpecification,
                                new ExtendedWkbGeometry(secondBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                            secondBoxNumberAddressWasMigrated.OfficiallyAssigned,
                            parentAddressPersistentLocalId: destinationAddressPersistentLocalId)
                });

            var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            streetName.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                sourceAddressWasMigrated,
                firstBoxNumberAddressWasMigrated,
                secondBoxNumberAddressWasMigrated,
                destinationHouseNumberAddressWasProposed,
                destinationFirstBoxNumberWasProposed,
                destinationSecondBoxNumberWasProposed,
                streetNameWasReaddressed
            });

            var sourceAddressFirstBoxNumberAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == proposedBoxNumberAddressPersistentLocalId);
            sourceAddressFirstBoxNumberAddress.Should().NotBeNull();
            sourceAddressFirstBoxNumberAddress!.Status.Should().Be(firstBoxNumberAddressWasMigrated.Status);

            var sourceAddressSecondBoxNumberAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == currentBoxNumberAddressPersistentLocalId);
            sourceAddressSecondBoxNumberAddress.Should().NotBeNull();
            sourceAddressSecondBoxNumberAddress!.Status.Should().Be(secondBoxNumberAddressWasMigrated.Status);

            var destinationAddress = streetName.StreetNameAddresses.FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
            destinationAddress.Should().NotBeNull();
            destinationAddress!.HouseNumber.Should().Be(destinationHouseNumber);
            destinationAddress.Status.Should().Be(sourceAddressWasMigrated.Status);
            destinationAddress.Geometry.GeometryMethod.Should().Be(sourceAddressWasMigrated.GeometryMethod);
            destinationAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressWasMigrated.GeometrySpecification);
            destinationAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry));
            destinationAddress.PostalCode.Should().Be(new PostalCode(sourceAddressWasMigrated.PostalCode!));
            destinationAddress.IsOfficiallyAssigned.Should().Be(sourceAddressWasMigrated.OfficiallyAssigned);

            destinationAddress.Children.Should().HaveCount(2);
            var destinationAddressFirstBoxNumberAddress = destinationAddress.Children
                .SingleOrDefault(x => x.AddressPersistentLocalId == destinationFirstBoxNumberAddressPersistentLocalId);
            destinationAddressFirstBoxNumberAddress.Should().NotBeNull();
            destinationAddressFirstBoxNumberAddress!.HouseNumber.Should().Be(destinationHouseNumber);
            destinationAddressFirstBoxNumberAddress.Status.Should().Be(firstBoxNumberAddressWasMigrated.Status);
            destinationAddressFirstBoxNumberAddress.Geometry.GeometryMethod.Should().Be(firstBoxNumberAddressWasMigrated.GeometryMethod);
            destinationAddressFirstBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(firstBoxNumberAddressWasMigrated.GeometrySpecification);
            destinationAddressFirstBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(firstBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
            destinationAddressFirstBoxNumberAddress.PostalCode.Should().Be(new PostalCode(firstBoxNumberAddressWasMigrated.PostalCode!));
            destinationAddressFirstBoxNumberAddress.IsOfficiallyAssigned.Should().Be(firstBoxNumberAddressWasMigrated.OfficiallyAssigned);

            var destinationAddressSecondBoxNumberAddress = destinationAddress.Children
                .SingleOrDefault(x => x.AddressPersistentLocalId == destinationSecondBoxNumberAddressPersistentLocalId);
            destinationAddressSecondBoxNumberAddress.Should().NotBeNull();
            destinationAddressSecondBoxNumberAddress!.HouseNumber.Should().Be(destinationHouseNumber);
            destinationAddressSecondBoxNumberAddress.Status.Should().Be(secondBoxNumberAddressWasMigrated.Status);
            destinationAddressSecondBoxNumberAddress.Geometry.GeometryMethod.Should().Be(secondBoxNumberAddressWasMigrated.GeometryMethod);
            destinationAddressSecondBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(secondBoxNumberAddressWasMigrated.GeometrySpecification);
            destinationAddressSecondBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(secondBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
            destinationAddressSecondBoxNumberAddress.PostalCode.Should().Be(new PostalCode(secondBoxNumberAddressWasMigrated.PostalCode!));
            destinationAddressSecondBoxNumberAddress.IsOfficiallyAssigned.Should().Be(secondBoxNumberAddressWasMigrated.OfficiallyAssigned);
        }
    }
}
