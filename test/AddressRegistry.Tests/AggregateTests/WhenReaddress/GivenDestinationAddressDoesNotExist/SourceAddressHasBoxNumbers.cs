namespace AddressRegistry.Tests.AggregateTests.WhenReaddress.GivenDestinationAddressDoesNotExist
{
    using System.Collections.Generic;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AddressRegistry.StreetName.DataStructures;
    using AddressRegistry.StreetName.Events;
    using AddressRegistry.Tests.AutoFixture;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventBuilders;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class SourceAddressHasBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;
        private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public SourceAddressHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
            _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        }

        [Fact]
        public void ThenDestinationAddressHasBoxNumbers()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);

            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with id 1
            var destinationProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var destinationCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var sourceHouseNumber = new HouseNumber("11");
            var destinationHouseNumber = new HouseNumber("13");

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var proposedBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A1"), sourceAddressPersistentLocalId)
                .Build();

            var currentBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .WithBoxNumber(new BoxNumber("A2"), sourceAddressPersistentLocalId)
                .Build();

            var command = new Readdress(
                _streetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(_streetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber)
                },
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    sourceAddressWasMigrated,
                    proposedBoxNumberAddressWasMigrated,
                    currentBoxNumberAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            sourceAddressPersistentLocalId,
                            null,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            null,
                            sourceAddressWasMigrated.GeometryMethod,
                            sourceAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            destinationProposedBoxNumberAddressPersistentLocalId,
                            proposedBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                            proposedBoxNumberAddressWasMigrated.GeometryMethod,
                            proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressWasProposedBecauseOfReaddress(
                            _streetNamePersistentLocalId,
                            destinationCurrentBoxNumberAddressPersistentLocalId,
                            currentBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new PostalCode(sourceAddressWasMigrated.PostalCode!),
                            destinationHouseNumber,
                            new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                            currentBoxNumberAddressWasMigrated.GeometryMethod,
                            currentBoxNumberAddressWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry))),
                    new Fact(_streamId,
                        new AddressHouseNumberWasReaddressed(
                            _streetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            readdressedHouseNumber: new ReaddressedAddressData(
                                sourceAddressPersistentLocalId,
                                destinationAddressPersistentLocalId,
                                isDestinationNewlyProposed: true,
                                sourceAddressWasMigrated.Status,
                                destinationHouseNumber,
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
                                    proposedBoxNumberAddressPersistentLocalId,
                                    destinationProposedBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    proposedBoxNumberAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    boxNumber: new BoxNumber(proposedBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        proposedBoxNumberAddressWasMigrated.GeometryMethod,
                                        proposedBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(proposedBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    proposedBoxNumberAddressWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    currentBoxNumberAddressPersistentLocalId,
                                    destinationCurrentBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    currentBoxNumberAddressWasMigrated.Status,
                                    destinationHouseNumber,
                                    boxNumber: new BoxNumber(currentBoxNumberAddressWasMigrated.BoxNumber!),
                                    new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                    new AddressGeometry(
                                        currentBoxNumberAddressWasMigrated.GeometryMethod,
                                        currentBoxNumberAddressWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(currentBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
                                    currentBoxNumberAddressWasMigrated.OfficiallyAssigned)
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
        public void WithIdenticalSourceAndDestinationHouseNumbers_ThenSourceAddressAndItsBoxNumbersWereReaddressed()
        {
            var sourceHouseNumber = new HouseNumber("1550");
            var destinationHouseNumber = new HouseNumber("1550");
            var sourceBoxNumber1 = new BoxNumber("1");
            var sourceBoxNumber2 = new BoxNumber("2");
            var postalCode = Fixture.Create<PostalCode>();

            var sourceStreetNamePersistentLocalId = new StreetNamePersistentLocalId(30587);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(40001044);
            var firstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(40001047);
            var secondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(40001048);

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(30591);
            var destinationStreetNameStreamId = new StreetNameStreamId(destinationStreetNamePersistentLocalId);

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(sourceStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var sourceAddressFirstBoxNumberWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(sourceStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(firstBoxNumberAddressPersistentLocalId)
                .WithBoxNumber(sourceBoxNumber1, sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var sourceAddressSecondBoxNumberWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(sourceStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(secondBoxNumberAddressPersistentLocalId)
                .WithBoxNumber(sourceBoxNumber2, sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var sourceStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sourceStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(sourceStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(),
                    StreetNameStatus.Current),
                sourceAddressWasMigrated,
                sourceAddressFirstBoxNumberWasMigrated,
                sourceAddressSecondBoxNumberWasMigrated
            });

            var destinationStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            destinationStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(destinationStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(),
                    StreetNameStatus.Current)
            });

            var streetNames = Container.Resolve<IStreetNames>();
            streetNames.Add(new StreetNameStreamId(sourceStreetNamePersistentLocalId), sourceStreetName);
            streetNames.Add(new StreetNameStreamId(destinationStreetNamePersistentLocalId), destinationStreetName);

            var command = new Readdress(
                destinationStreetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(sourceStreetNamePersistentLocalId, sourceAddressPersistentLocalId,
                        destinationHouseNumber)
                },
                new List<RetireAddressItem>
                {
                    new RetireAddressItem(sourceStreetNamePersistentLocalId, sourceAddressPersistentLocalId)
                },
                Fixture.Create<Provenance>());

            var expectedParentPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with 1
            var expectedBoxNumber1PersistentLocalId = new AddressPersistentLocalId(2);
            var expectedBoxNumber2PersistentLocalId  = new AddressPersistentLocalId(3);

            Assert(new Scenario()
                .Given(destinationStreetNameStreamId)
                .When(command)
                 .Then(new[]
                {
                    new Fact(destinationStreetNameStreamId,
                        new AddressWasProposedBecauseOfReaddress(
                            destinationStreetNamePersistentLocalId,
                            addressPersistentLocal: expectedParentPersistentLocalId,
                            sourceAddressPersistentLocalId: sourceAddressPersistentLocalId,
                            parentPersistentLocalId: null,
                            postalCode,
                            destinationHouseNumber,
                            null,
                            sourceAddressSecondBoxNumberWasMigrated.GeometryMethod,
                            sourceAddressSecondBoxNumberWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressSecondBoxNumberWasMigrated.ExtendedWkbGeometry))),
                    new Fact(destinationStreetNameStreamId,
                        new AddressWasProposedBecauseOfReaddress(
                            destinationStreetNamePersistentLocalId,
                            addressPersistentLocal: expectedBoxNumber1PersistentLocalId,
                            sourceAddressPersistentLocalId: firstBoxNumberAddressPersistentLocalId,
                            parentPersistentLocalId: expectedParentPersistentLocalId,
                            postalCode,
                            destinationHouseNumber,
                            new BoxNumber(sourceAddressFirstBoxNumberWasMigrated.BoxNumber!),
                            sourceAddressSecondBoxNumberWasMigrated.GeometryMethod,
                            sourceAddressSecondBoxNumberWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressSecondBoxNumberWasMigrated.ExtendedWkbGeometry))),
                    new Fact(destinationStreetNameStreamId,
                        new AddressWasProposedBecauseOfReaddress(
                            destinationStreetNamePersistentLocalId,
                            addressPersistentLocal: expectedBoxNumber2PersistentLocalId,
                            sourceAddressPersistentLocalId: secondBoxNumberAddressPersistentLocalId,
                            parentPersistentLocalId: expectedParentPersistentLocalId,
                            postalCode,
                            destinationHouseNumber,
                            new BoxNumber(sourceAddressSecondBoxNumberWasMigrated.BoxNumber!),
                            sourceAddressSecondBoxNumberWasMigrated.GeometryMethod,
                            sourceAddressSecondBoxNumberWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressSecondBoxNumberWasMigrated.ExtendedWkbGeometry))),
                    new Fact(destinationStreetNameStreamId,
                        new AddressHouseNumberWasReaddressed(
                            destinationStreetNamePersistentLocalId,
                            addressPersistentLocalId: expectedParentPersistentLocalId,
                            new ReaddressedAddressData(
                                sourceAddressPersistentLocalId: sourceAddressPersistentLocalId,
                                destinationAddressPersistentLocalId: expectedParentPersistentLocalId,
                                isDestinationNewlyProposed: true,
                                sourceAddressWasMigrated.Status,
                                destinationHouseNumber,
                                boxNumber: null,
                                new PostalCode(sourceAddressWasMigrated.PostalCode!),
                                new AddressGeometry(
                                    sourceAddressWasMigrated.GeometryMethod,
                                    sourceAddressWasMigrated.GeometrySpecification,
                                    new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry)),
                                sourceAddressWasMigrated.OfficiallyAssigned),
                            new List<ReaddressedAddressData>
                            {
                                new ReaddressedAddressData(
                                    firstBoxNumberAddressPersistentLocalId,
                                    expectedBoxNumber1PersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    sourceAddressFirstBoxNumberWasMigrated.Status,
                                    destinationHouseNumber,
                                    new BoxNumber(sourceAddressFirstBoxNumberWasMigrated.BoxNumber!),
                                    postalCode,
                                    new AddressGeometry(
                                        sourceAddressFirstBoxNumberWasMigrated.GeometryMethod,
                                        sourceAddressFirstBoxNumberWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(sourceAddressFirstBoxNumberWasMigrated.ExtendedWkbGeometry)),
                                    sourceAddressFirstBoxNumberWasMigrated.OfficiallyAssigned),
                                new ReaddressedAddressData(
                                    secondBoxNumberAddressPersistentLocalId,
                                    expectedBoxNumber2PersistentLocalId,
                                    isDestinationNewlyProposed: true,
                                    sourceAddressSecondBoxNumberWasMigrated.Status,
                                    destinationHouseNumber,
                                    new BoxNumber(sourceAddressSecondBoxNumberWasMigrated.BoxNumber!),
                                    postalCode,
                                    new AddressGeometry(
                                        sourceAddressSecondBoxNumberWasMigrated.GeometryMethod,
                                        sourceAddressSecondBoxNumberWasMigrated.GeometrySpecification,
                                        new ExtendedWkbGeometry(sourceAddressSecondBoxNumberWasMigrated.ExtendedWkbGeometry)),
                                    sourceAddressSecondBoxNumberWasMigrated.OfficiallyAssigned),
                            }))
                }));
        }
    }
}
