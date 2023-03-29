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

        [Fact]
        public void WithSourceAddressHasBoxNumbers_ThenDestinationAddressHasBoxNumbers()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var sourceProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var sourceCurrentBoxNumberAddressAddressPersistentLocalId = new AddressPersistentLocalId(12);

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

            var migratedProposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithBoxNumber(new BoxNumber("A"), sourceAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedCurrentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(sourceCurrentBoxNumberAddressAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithBoxNumber(new BoxNumber("B"), sourceAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
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

            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedProposedAddressPersistentLocalId = new AddressPersistentLocalId(3);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedProposedBoxNumberAddress,
                    migratedCurrentBoxNumberAddress,
                    migratedSourceAndDestinationAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            sourceAndDestinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber13,
                            new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber!),
                            migratedProposedBoxNumberAddress.GeometryMethod,
                            migratedProposedBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedProposedBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRejected(
                            _streetNamePersistentLocalId,
                            sourceProposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            sourceAndDestinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber13,
                            new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber!),
                            migratedCurrentBoxNumberAddress.GeometryMethod,
                            migratedCurrentBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                            _streetNamePersistentLocalId,
                            sourceCurrentBoxNumberAddressAddressPersistentLocalId)),
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
                            new List<AddressPersistentLocalId>
                            {
                                expectedProposedBoxNumberAddressPersistentLocalId,
                                expectedCurrentBoxNumberAddressPersistentLocalId,
                                expectedProposedAddressPersistentLocalId
                            },
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
                                    sourceProposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    migratedProposedBoxNumberAddress.Status,
                                    destinationHouseNumber13,
                                    new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedProposedBoxNumberAddress.GeometryMethod,
                                        migratedProposedBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedProposedBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedProposedBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: sourceAndDestinationAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    sourceCurrentBoxNumberAddressAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    migratedCurrentBoxNumberAddress.Status,
                                    destinationHouseNumber13,
                                    new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedCurrentBoxNumberAddress.GeometryMethod,
                                        migratedCurrentBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedCurrentBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: sourceAndDestinationAddressPersistentLocalId),
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

            command.ExecutionContext.AddressesAdded.Should().HaveCount(3);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedCurrentBoxNumberAddressPersistentLocalId);
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

        [Fact]
        public void WithSourceAndDestinationAddressHasBoxNumbers_ThenLastDestinationAddressHasBoxNumbers()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var sourceAndDestinationAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var sourceAndDestinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var sourceAndDestinationCurrentBoxNumberAddressAddressPersistentLocalId = new AddressPersistentLocalId(12);

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

            var migratedProposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceAndDestinationProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber13)
                .WithBoxNumber(new BoxNumber("A"), sourceAndDestinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedCurrentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(sourceAndDestinationCurrentBoxNumberAddressAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber13)
                .WithBoxNumber(new BoxNumber("B"), sourceAndDestinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
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

            var expectedProposedAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedSourceAndDestinationAddress,
                    migratedProposedBoxNumberAddress,
                    migratedCurrentBoxNumberAddress)
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
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            expectedProposedAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber15,
                            new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber!),
                            migratedProposedBoxNumberAddress.GeometryMethod,
                            migratedProposedBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedProposedBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRejected(
                            _streetNamePersistentLocalId,
                            sourceAndDestinationProposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            expectedProposedAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber15,
                            new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber!),
                            migratedCurrentBoxNumberAddress.GeometryMethod,
                            migratedCurrentBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                            _streetNamePersistentLocalId,
                            sourceAndDestinationCurrentBoxNumberAddressAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                expectedProposedAddressPersistentLocalId,
                                expectedProposedBoxNumberAddressPersistentLocalId,
                                expectedCurrentBoxNumberAddressPersistentLocalId
                            },
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
                                    parentAddressPersistentLocalId: null),
                                 new ReaddressedAddressData(
                                    sourceAndDestinationProposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    migratedProposedBoxNumberAddress.Status,
                                    destinationHouseNumber15,
                                    new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedProposedBoxNumberAddress.GeometryMethod,
                                        migratedProposedBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedProposedBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedProposedBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: expectedProposedAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    sourceAndDestinationCurrentBoxNumberAddressAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    migratedCurrentBoxNumberAddress.Status,
                                    destinationHouseNumber15,
                                    new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedCurrentBoxNumberAddress.GeometryMethod,
                                        migratedCurrentBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedCurrentBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: expectedProposedAddressPersistentLocalId)
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
