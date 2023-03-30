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

            var expectedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with 1

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
                            expectedHouseNumberAddressPersistentLocalId,
                            null,
                            postalCode,
                            destinationHouseNumber15,
                            null,
                            migratedSourceAndDestinationAddress.GeometryMethod,
                            migratedSourceAndDestinationAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedSourceAndDestinationAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId> { expectedHouseNumberAddressPersistentLocalId },
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
                                    expectedHouseNumberAddressPersistentLocalId,
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
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == sourceAndDestinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);
        }

        [Fact]
        public void WithSourceAddressHasBoxNumbers_ThenDestinationAddressHasBoxNumbers()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var sourceProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var sourceCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(12);

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
                .WithAddressPersistentLocalId(sourceCurrentBoxNumberAddressPersistentLocalId)
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
            var expectedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

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
                            sourceCurrentBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasProposedV2(_streetNamePersistentLocalId,
                            expectedHouseNumberAddressPersistentLocalId,
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
                                expectedHouseNumberAddressPersistentLocalId
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
                                    sourceCurrentBoxNumberAddressPersistentLocalId,
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
                                    expectedHouseNumberAddressPersistentLocalId,
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
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == sourceAndDestinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);
        }

        [Fact]
        public void WithSourceAndDestinationAddressHasBoxNumbers_ThenLastDestinationAddressHasBoxNumbers()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var sourceAndDestinationAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var sourceAndDestinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(12);

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
                .WithAddressPersistentLocalId(sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId)
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

            var expectedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
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
                            expectedHouseNumberAddressPersistentLocalId,
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
                            expectedHouseNumberAddressPersistentLocalId,
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
                            expectedHouseNumberAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber15,
                            new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber!),
                            migratedCurrentBoxNumberAddress.GeometryMethod,
                            migratedCurrentBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                            _streetNamePersistentLocalId,
                            sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                expectedHouseNumberAddressPersistentLocalId,
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
                                    expectedHouseNumberAddressPersistentLocalId,
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
                                    parentAddressPersistentLocalId: expectedHouseNumberAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId,
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
                                    parentAddressPersistentLocalId: expectedHouseNumberAddressPersistentLocalId)
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
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == sourceAndDestinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);
        }

        [Fact]
        public void WithBothSourceAddressHasBoxNumbersAndSourceAndDestinationAddressHasBoxNumbers_ThenBoxNumbersAreReadressed()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var sourceProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var sourceCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(12);

            var sourceAndDestinationAddressPersistentLocalId = new AddressPersistentLocalId(13);
            var sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(14);
            var sourceAndDestinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(15);

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
                .WithAddressPersistentLocalId(sourceCurrentBoxNumberAddressPersistentLocalId)
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

            var migratedSourceAndDestinationCurrentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber13)
                .WithBoxNumber(new BoxNumber("A1"), sourceAndDestinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedSourceAndDestinationProposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceAndDestinationProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber13)
                .WithBoxNumber(new BoxNumber("B"), sourceAndDestinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
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

            var expectedSourceAndDestinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(4);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedProposedBoxNumberAddress,
                    migratedCurrentBoxNumberAddress,
                    migratedSourceAndDestinationAddress,
                    migratedSourceAndDestinationCurrentBoxNumberAddress,
                    migratedSourceAndDestinationProposedBoxNumberAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedSourceAndDestinationProposedBoxNumberAddressPersistentLocalId,
                            sourceAndDestinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber13,
                            new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber!),
                            migratedProposedBoxNumberAddress.GeometryMethod,
                            migratedProposedBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedProposedBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRejected(_streetNamePersistentLocalId,
                            sourceProposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(_streetNamePersistentLocalId,
                            sourceCurrentBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasProposedV2(_streetNamePersistentLocalId,
                            expectedHouseNumberAddressPersistentLocalId,
                            null,
                            postalCode,
                            destinationHouseNumber15,
                            null,
                            migratedSourceAndDestinationAddress.GeometryMethod,
                            migratedSourceAndDestinationAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedSourceAndDestinationAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedV2(_streetNamePersistentLocalId,
                            expectedCurrentBoxNumberAddressPersistentLocalId,
                            expectedHouseNumberAddressPersistentLocalId,
                            postalCode,
                            destinationHouseNumber15,
                            new BoxNumber(migratedSourceAndDestinationCurrentBoxNumberAddress.BoxNumber),
                            migratedSourceAndDestinationCurrentBoxNumberAddress.GeometryMethod,
                            migratedSourceAndDestinationCurrentBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedSourceAndDestinationCurrentBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(_streetNamePersistentLocalId,
                            sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasProposedV2(_streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            expectedHouseNumberAddressPersistentLocalId,
                            postalCode,
                            destinationHouseNumber15,
                            new BoxNumber(migratedSourceAndDestinationProposedBoxNumberAddress.BoxNumber),
                            migratedSourceAndDestinationProposedBoxNumberAddress.GeometryMethod,
                            migratedSourceAndDestinationProposedBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedSourceAndDestinationProposedBoxNumberAddress.ExtendedWkbGeometry))),

                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>
                            {
                                expectedSourceAndDestinationProposedBoxNumberAddressPersistentLocalId,
                                expectedHouseNumberAddressPersistentLocalId,
                                expectedCurrentBoxNumberAddressPersistentLocalId,
                                expectedProposedBoxNumberAddressPersistentLocalId
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
                                    expectedSourceAndDestinationProposedBoxNumberAddressPersistentLocalId,
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
                                    sourceCurrentBoxNumberAddressPersistentLocalId,
                                    sourceAndDestinationProposedBoxNumberAddressPersistentLocalId,
                                    migratedCurrentBoxNumberAddress.Status,
                                    destinationHouseNumber13,
                                    new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber!),
                                    new PostalCode(migratedSourceAndDestinationAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedCurrentBoxNumberAddress.GeometryMethod,
                                        migratedCurrentBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedCurrentBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: sourceAndDestinationAddressPersistentLocalId),

                                new ReaddressedAddressData(
                                    sourceAndDestinationAddressPersistentLocalId,
                                    expectedHouseNumberAddressPersistentLocalId,
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
                                    sourceAndDestinationCurrentBoxNumberAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    migratedSourceAndDestinationCurrentBoxNumberAddress.Status,
                                    destinationHouseNumber15,
                                    new BoxNumber(migratedSourceAndDestinationCurrentBoxNumberAddress.BoxNumber),
                                    new PostalCode(migratedSourceAndDestinationAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedSourceAndDestinationCurrentBoxNumberAddress.GeometryMethod,
                                        migratedSourceAndDestinationCurrentBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedSourceAndDestinationCurrentBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedSourceAndDestinationCurrentBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: expectedHouseNumberAddressPersistentLocalId),

                                new ReaddressedAddressData(
                                    sourceAndDestinationProposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    migratedSourceAndDestinationProposedBoxNumberAddress.Status,
                                    destinationHouseNumber15,
                                    new BoxNumber(migratedSourceAndDestinationProposedBoxNumberAddress.BoxNumber),
                                    new PostalCode(migratedSourceAndDestinationAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedSourceAndDestinationProposedBoxNumberAddress.GeometryMethod,
                                        migratedSourceAndDestinationProposedBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedSourceAndDestinationProposedBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedSourceAndDestinationProposedBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: expectedHouseNumberAddressPersistentLocalId),
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(4);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedSourceAndDestinationProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedCurrentBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedBoxNumberAddressPersistentLocalId);

            command.ExecutionContext.AddressesUpdated.Should().HaveCount(2);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == sourceAndDestinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedHouseNumberAddressPersistentLocalId);
        }
    }
}
