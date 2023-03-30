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
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenDestinationAddressDoesNotExist : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenDestinationAddressDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
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
                .Then(new[]
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
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    migrateSourceAddressPersistentLocalId,
                                    destinationAddressPersistentLocalId,
                                    migrateSourceAddress.Status,
                                    destinationHouseNumber,
                                    boxNumber: null,
                                    new PostalCode(migrateSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migrateSourceAddress.GeometryMethod,
                                        migrateSourceAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migrateSourceAddress.ExtendedWkbGeometry)),
                                    migrateSourceAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should()
                .ContainSingle(x =>
                    x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                    && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }

        [Fact]
        public void WithSourceAddressHasBoxNumbers_ThenDestinationAddressHasBoxNumbers()
        {
            var migrateSourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(789);

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(migrateSourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var proposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A1"), migrateSourceAddressPersistentLocalId)
                .Build();

            var currentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A2"), migrateSourceAddressPersistentLocalId)
                .Build();

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var destinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
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
                    migratedSourceAddress,
                    proposedBoxNumberAddress,
                    currentBoxNumberAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            null,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber,
                            null,
                            migratedSourceAddress.GeometryMethod,
                            migratedSourceAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedSourceAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            destinationProposedBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(proposedBoxNumberAddress.BoxNumber!),
                            proposedBoxNumberAddress.GeometryMethod,
                            proposedBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(proposedBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            destinationCurrentBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(currentBoxNumberAddress.BoxNumber!),
                            currentBoxNumberAddress.GeometryMethod,
                            currentBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(currentBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                 destinationAddressPersistentLocalId,
                                 destinationProposedBoxNumberAddressPersistentLocalId,
                                 destinationCurrentBoxNumberAddressPersistentLocalId,
                            },
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    migrateSourceAddressPersistentLocalId,
                                    destinationAddressPersistentLocalId,
                                    migratedSourceAddress.Status,
                                    destinationHouseNumber,
                                    boxNumber: null,
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedSourceAddress.GeometryMethod,
                                        migratedSourceAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedSourceAddress.ExtendedWkbGeometry)),
                                    migratedSourceAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: null),
                                new ReaddressedAddressData(
                                    proposedBoxNumberAddressPersistentLocalId,
                                    destinationProposedBoxNumberAddressPersistentLocalId,
                                    proposedBoxNumberAddress.Status,
                                    destinationHouseNumber,
                                    boxNumber: new BoxNumber(proposedBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        proposedBoxNumberAddress.GeometryMethod,
                                        proposedBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(proposedBoxNumberAddress.ExtendedWkbGeometry)),
                                    proposedBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: destinationAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    currentBoxNumberAddressPersistentLocalId,
                                    destinationCurrentBoxNumberAddressPersistentLocalId,
                                    currentBoxNumberAddress.Status,
                                    destinationHouseNumber,
                                    boxNumber: new BoxNumber(currentBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        currentBoxNumberAddress.GeometryMethod,
                                        currentBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(currentBoxNumberAddress.ExtendedWkbGeometry)),
                                    currentBoxNumberAddress.OfficiallyAssigned,
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

        [Fact]
        public void StateCheck()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(789);
            var postalCode = Fixture.Create<PostalCode>();

            var sourceAddressWasMigratedToStreetName = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var proposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A1"), sourceAddressPersistentLocalId)
                .Build();

            var currentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A2"), sourceAddressPersistentLocalId)
                .Build();

            var destinationHouseNumber = new HouseNumber("13");
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var destinationHouseNumberAddressWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                null,
                new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                destinationHouseNumber,
                null,
                sourceAddressWasMigratedToStreetName.GeometryMethod,
                sourceAddressWasMigratedToStreetName.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressWasMigratedToStreetName.ExtendedWkbGeometry));

            var destinationFirstBoxNumberWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationProposedBoxNumberAddressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                destinationHouseNumber,
                new BoxNumber(proposedBoxNumberAddress.BoxNumber!),
                proposedBoxNumberAddress.GeometryMethod,
                proposedBoxNumberAddress.GeometrySpecification,
                new ExtendedWkbGeometry(proposedBoxNumberAddress.ExtendedWkbGeometry));

            var destinationSecondBoxNumberWasProposed = new AddressWasProposedV2(
                _streetNamePersistentLocalId,
                destinationCurrentBoxNumberAddressPersistentLocalId,
                destinationAddressPersistentLocalId,
                new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                destinationHouseNumber,
                new BoxNumber(currentBoxNumberAddress.BoxNumber!),
                currentBoxNumberAddress.GeometryMethod,
                currentBoxNumberAddress.GeometrySpecification,
                new ExtendedWkbGeometry(currentBoxNumberAddress.ExtendedWkbGeometry));

            var streetNameWasReaddressed = new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                new List<AddressPersistentLocalId>
                {
                    destinationAddressPersistentLocalId,
                    destinationProposedBoxNumberAddressPersistentLocalId,
                    destinationCurrentBoxNumberAddressPersistentLocalId
                },
                new List<ReaddressedAddressData>
                {
                    new ReaddressedAddressData(
                        sourceAddressPersistentLocalId,
                        destinationAddressPersistentLocalId,
                        sourceAddressWasMigratedToStreetName.Status,
                        destinationHouseNumber,
                        boxNumber: null,
                        new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                        new AddressGeometry(
                            sourceAddressWasMigratedToStreetName.GeometryMethod,
                            sourceAddressWasMigratedToStreetName.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressWasMigratedToStreetName.ExtendedWkbGeometry)),
                        sourceAddressWasMigratedToStreetName.OfficiallyAssigned,
                        parentAddressPersistentLocalId: null),
                        new ReaddressedAddressData(
                            proposedBoxNumberAddressPersistentLocalId,
                            destinationProposedBoxNumberAddressPersistentLocalId,
                            proposedBoxNumberAddress.Status,
                            destinationHouseNumber,
                            boxNumber: new BoxNumber(proposedBoxNumberAddress.BoxNumber!),
                            new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                            new AddressGeometry(
                                proposedBoxNumberAddress.GeometryMethod,
                                proposedBoxNumberAddress.GeometrySpecification,
                                new ExtendedWkbGeometry(proposedBoxNumberAddress.ExtendedWkbGeometry)),
                            proposedBoxNumberAddress.OfficiallyAssigned,
                            parentAddressPersistentLocalId: destinationAddressPersistentLocalId),
                        new ReaddressedAddressData(
                            currentBoxNumberAddressPersistentLocalId,
                            destinationCurrentBoxNumberAddressPersistentLocalId,
                            currentBoxNumberAddress.Status,
                            destinationHouseNumber,
                            boxNumber: new BoxNumber(currentBoxNumberAddress.BoxNumber!),
                            new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!),
                            new AddressGeometry(
                                currentBoxNumberAddress.GeometryMethod,
                                currentBoxNumberAddress.GeometrySpecification,
                                new ExtendedWkbGeometry(currentBoxNumberAddress.ExtendedWkbGeometry)),
                            currentBoxNumberAddress.OfficiallyAssigned,
                            parentAddressPersistentLocalId: destinationAddressPersistentLocalId)
                });

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                sourceAddressWasMigratedToStreetName,
                proposedBoxNumberAddress,
                currentBoxNumberAddress,
                destinationHouseNumberAddressWasProposed,
                destinationFirstBoxNumberWasProposed,
                destinationSecondBoxNumberWasProposed,
                streetNameWasReaddressed
            });

            var destinationAddress = sut.StreetNameAddresses.FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
            destinationAddress.Should().NotBeNull();
            destinationAddress!.HouseNumber.Should().Be(destinationHouseNumber);
            destinationAddress.Status.Should().Be(sourceAddressWasMigratedToStreetName.Status);
            destinationAddress.Geometry.GeometryMethod.Should().Be(sourceAddressWasMigratedToStreetName.GeometryMethod);
            destinationAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressWasMigratedToStreetName.GeometrySpecification);
            destinationAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressWasMigratedToStreetName.ExtendedWkbGeometry));
            destinationAddress.PostalCode.Should().Be(new PostalCode(sourceAddressWasMigratedToStreetName.PostalCode!));
            destinationAddress.IsOfficiallyAssigned.Should().Be(sourceAddressWasMigratedToStreetName.OfficiallyAssigned);

            destinationAddress.Children.Should().HaveCount(2);
            var destinationProposedBoxNumberAddress = destinationAddress.Children
                .SingleOrDefault(x => x.AddressPersistentLocalId == destinationProposedBoxNumberAddressPersistentLocalId);
            destinationProposedBoxNumberAddress.Should().NotBeNull();
            destinationProposedBoxNumberAddress!.HouseNumber.Should().Be(destinationHouseNumber);
            destinationProposedBoxNumberAddress.Status.Should().Be(proposedBoxNumberAddress.Status);
            destinationProposedBoxNumberAddress.Geometry.GeometryMethod.Should().Be(proposedBoxNumberAddress.GeometryMethod);
            destinationProposedBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(proposedBoxNumberAddress.GeometrySpecification);
            destinationProposedBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(proposedBoxNumberAddress.ExtendedWkbGeometry));
            destinationProposedBoxNumberAddress.PostalCode.Should().Be(new PostalCode(proposedBoxNumberAddress.PostalCode!));
            destinationProposedBoxNumberAddress.IsOfficiallyAssigned.Should().Be(proposedBoxNumberAddress.OfficiallyAssigned);

            var destinationCurrentBoxNumberAddress = destinationAddress.Children
                .SingleOrDefault(x => x.AddressPersistentLocalId == destinationCurrentBoxNumberAddressPersistentLocalId);
            destinationCurrentBoxNumberAddress.Should().NotBeNull();
            destinationCurrentBoxNumberAddress!.HouseNumber.Should().Be(destinationHouseNumber);
            destinationCurrentBoxNumberAddress.Status.Should().Be(currentBoxNumberAddress.Status);
            destinationCurrentBoxNumberAddress.Geometry.GeometryMethod.Should().Be(currentBoxNumberAddress.GeometryMethod);
            destinationCurrentBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(currentBoxNumberAddress.GeometrySpecification);
            destinationCurrentBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(currentBoxNumberAddress.ExtendedWkbGeometry));
            destinationCurrentBoxNumberAddress.PostalCode.Should().Be(new PostalCode(currentBoxNumberAddress.PostalCode!));
            destinationCurrentBoxNumberAddress.IsOfficiallyAssigned.Should().Be(currentBoxNumberAddress.OfficiallyAssigned);

            var expectedProposedBoxNumberAddress = sut.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == proposedBoxNumberAddressPersistentLocalId);
            expectedProposedBoxNumberAddress.Should().NotBeNull();
            expectedProposedBoxNumberAddress.Status.Should().Be(AddressStatus.Proposed);

            var expectedCurrentBoxNumberAddress = sut.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == currentBoxNumberAddressPersistentLocalId);
            expectedCurrentBoxNumberAddress.Should().NotBeNull();
            expectedCurrentBoxNumberAddress.Status.Should().Be(AddressStatus.Current);
        }
    }
}
