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

    public class GivenDestinationAddressExists: AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public GivenDestinationAddressExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenSourceAddressWasReaddressed()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var destinationHouseNumber = new HouseNumber("13");

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedDestinationAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedDestinationAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>(),
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
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
                                    parentAddressPersistentLocalId: null)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().BeEmpty();
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WithNonActiveStatusDestinationAddress_ThenDestinationAddressWasProposed(AddressStatus addressStatus)
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var destinationHouseNumber = new HouseNumber("13");

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedDestinationAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, addressStatus)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(11))
                .WithHouseNumber(destinationHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedDestinationAddress)
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
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId> { destinationAddressPersistentLocalId },
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
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
                                    parentAddressPersistentLocalId: null)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }

        [Fact]
        public void WithRemovedDestinationAddress_ThenDestinationAddressWasProposed()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var destinationHouseNumber = new HouseNumber("13");

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(new HouseNumber("11"))
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedDestinationAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(11))
                .WithHouseNumber(destinationHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .WithIsRemoved()
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedDestinationAddress)
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
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId> { destinationAddressPersistentLocalId },
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
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
                                    parentAddressPersistentLocalId: null)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }

        [Fact]
        public void WithSourceAddressHasBoxNumbersAndDestinationAddressHasNoBoxNumbers_ThenSourceAddressWasReaddressedAndBoxNumbersWereProposed()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var sourceProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(11);
            var sourceCurrentBoxNumberAddressAddressPersistentLocalId = new AddressPersistentLocalId(12);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(13);
            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");
            var postalCode = Fixture.Create<PostalCode>();

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedProposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(sourceProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithBoxNumber(new BoxNumber("A"), sourceAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedCurrentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(sourceCurrentBoxNumberAddressAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithBoxNumber(new BoxNumber("B"), sourceAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedDestinationAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            var expectedProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);  // FakePersistentLocalIdGenerator starts with id 1
            var expectedCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedProposedBoxNumberAddress,
                    migratedCurrentBoxNumberAddress,
                    migratedDestinationAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            _streetNamePersistentLocalId,
                            expectedProposedBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber),
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
                            destinationAddressPersistentLocalId,
                            new PostalCode(migratedSourceAddress.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber),
                            migratedCurrentBoxNumberAddress.GeometryMethod,
                            migratedCurrentBoxNumberAddress.GeometrySpecification,
                            new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                            _streetNamePersistentLocalId,
                            sourceCurrentBoxNumberAddressAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId> { expectedProposedBoxNumberAddressPersistentLocalId, expectedCurrentBoxNumberAddressPersistentLocalId },
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
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
                                    sourceProposedBoxNumberAddressPersistentLocalId,
                                    expectedProposedBoxNumberAddressPersistentLocalId,
                                    migratedProposedBoxNumberAddress.Status,
                                    destinationHouseNumber,
                                    new BoxNumber(migratedProposedBoxNumberAddress.BoxNumber),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedProposedBoxNumberAddress.GeometryMethod,
                                        migratedProposedBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedProposedBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedProposedBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: destinationAddressPersistentLocalId),
                                new ReaddressedAddressData(
                                    sourceCurrentBoxNumberAddressAddressPersistentLocalId,
                                    expectedCurrentBoxNumberAddressPersistentLocalId,
                                    migratedCurrentBoxNumberAddress.Status,
                                    destinationHouseNumber,
                                    new BoxNumber(migratedCurrentBoxNumberAddress.BoxNumber),
                                    new PostalCode(migratedSourceAddress.PostalCode!),
                                    new AddressGeometry(
                                        migratedCurrentBoxNumberAddress.GeometryMethod,
                                        migratedCurrentBoxNumberAddress.GeometrySpecification,
                                        new ExtendedWkbGeometry(migratedCurrentBoxNumberAddress.ExtendedWkbGeometry)),
                                    migratedCurrentBoxNumberAddress.OfficiallyAssigned,
                                    parentAddressPersistentLocalId: destinationAddressPersistentLocalId)

                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().HaveCount(2);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedProposedBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedCurrentBoxNumberAddressPersistentLocalId);
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }

        [Fact]
        public void WithSourceAddressHasNoBoxNumbersAndDestinationAddressHasBoxNumbers_ThenDestinationBoxNumbersWereRejectedOrRetired()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(10);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(13);
            var destinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(14);
            var destinationCurrentBoxNumberAddressAddressPersistentLocalId = new AddressPersistentLocalId(15);
            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");
            var postalCode = Fixture.Create<PostalCode>();

            var migratedSourceAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();
            
            var migratedDestinationAddress = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedProposedBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(destinationProposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithBoxNumber(new BoxNumber("A"), destinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var migratedCurrentBoxNumberAddress = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(destinationCurrentBoxNumberAddressAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithBoxNumber(new BoxNumber("B"), destinationAddressPersistentLocalId)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem> { new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber) },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migratedSourceAddress,
                    migratedDestinationAddress,
                    migratedProposedBoxNumberAddress,
                    migratedCurrentBoxNumberAddress)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasRejected(
                            _streetNamePersistentLocalId,
                            destinationProposedBoxNumberAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                            _streetNamePersistentLocalId,
                            destinationCurrentBoxNumberAddressAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new StreetNameWasReaddressed(_streetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>(),
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    sourceAddressPersistentLocalId,
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
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().BeEmpty();
            
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == _streetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
