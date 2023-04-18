namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing.GivenSourceAddressInOtherStreetName
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class SourceAddressHasBoxNumbers: AddressRegistryTest
    {
        public SourceAddressHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
        }

        [Fact]
        public void ThenSourceAddressAndItsBoxNumbersWereReaddressed()
        {
            var sourceHouseNumber = new HouseNumber("100");
            var destinationHouseNumber = new HouseNumber("5");
            var sharedBoxNumber = new BoxNumber("A");
            var postalCode = Fixture.Create<PostalCode>();

            var sourceStreetNamePersistentLocalId = new StreetNamePersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);
            var firstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(101);
            var secondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(2);
            var destinationStreetNameStreamId = new StreetNameStreamId(destinationStreetNamePersistentLocalId);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(103);
            var destinationBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(104);

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
                .WithBoxNumber(sharedBoxNumber, sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var sourceAddressSecondBoxNumberWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(sourceStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(secondBoxNumberAddressPersistentLocalId)
                .WithBoxNumber(new BoxNumber("B"), sourceAddressPersistentLocalId)
                .WithHouseNumber(sourceHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var sourceStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sourceStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(sourceStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(), StreetNameStatus.Current),
                sourceAddressWasMigrated,
                sourceAddressFirstBoxNumberWasMigrated,
                sourceAddressSecondBoxNumberWasMigrated
            });

            var streetNames = Container.Resolve<IStreetNames>();
            streetNames.Add(new StreetNameStreamId(sourceStreetNamePersistentLocalId), sourceStreetName);

            var destinationAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(destinationStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithPostalCode(postalCode)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var destinationAddressBoxNumberWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(destinationStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(destinationBoxNumberAddressPersistentLocalId)
                .WithBoxNumber(sharedBoxNumber, destinationAddressPersistentLocalId)
                .WithHouseNumber(destinationHouseNumber)
                .WithAddressGeometry(new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.SecondGmlPointGeometry.ToExtendedWkbGeometry()))
                .Build();

            var command = new Readdress(
                destinationStreetNamePersistentLocalId,
                new List<ReaddressAddressItem>
                {
                    new ReaddressAddressItem(sourceStreetNamePersistentLocalId, sourceAddressPersistentLocalId , destinationHouseNumber)
                },
                new List<RetireAddressItem>
                {
                    new RetireAddressItem(sourceStreetNamePersistentLocalId, sourceAddressPersistentLocalId)
                },
                Fixture.Create<Provenance>());

            var expectedBoxNumberPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with 1.

            Assert(new Scenario()
                .Given(destinationStreetNameStreamId,
                    Fixture.Create<StreetNameWasImported>().WithStreetNamePersistentLocalId(destinationStreetNamePersistentLocalId),
                    destinationAddressWasMigrated,
                    destinationAddressBoxNumberWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(destinationStreetNameStreamId,
                        new AddressWasProposedBecauseOfReaddressing(
                            destinationStreetNamePersistentLocalId,
                            expectedBoxNumberPersistentLocalId,
                            secondBoxNumberAddressPersistentLocalId,
                            destinationAddressPersistentLocalId,
                            postalCode,
                            destinationHouseNumber,
                            new BoxNumber(sourceAddressSecondBoxNumberWasMigrated.BoxNumber!),
                            sourceAddressSecondBoxNumberWasMigrated.GeometryMethod,
                            sourceAddressSecondBoxNumberWasMigrated.GeometrySpecification,
                            new ExtendedWkbGeometry(sourceAddressSecondBoxNumberWasMigrated.ExtendedWkbGeometry))),
                    new Fact(destinationStreetNameStreamId,
                        new AddressHouseNumberWasReaddressed(
                            destinationStreetNamePersistentLocalId,
                            destinationAddressPersistentLocalId,
                            new ReaddressedAddressData(
                                sourceAddressPersistentLocalId,
                                destinationAddressPersistentLocalId,
                                isDestinationNewlyProposed: false,
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
                                    destinationBoxNumberAddressPersistentLocalId,
                                    isDestinationNewlyProposed: false,
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
                                    expectedBoxNumberPersistentLocalId,
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
                            },
                            rejectedBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>(),
                            retiredBoxNumberAddressPersistentLocalIds: new List<AddressPersistentLocalId>()))
                }));

            command.ExecutionContext.AddressesAdded.Should().ContainSingle();
            command.ExecutionContext.AddressesAdded.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == destinationStreetNamePersistentLocalId
                && x.addressPersistentLocalId == expectedBoxNumberPersistentLocalId);

            command.ExecutionContext.AddressesAdded.Should().ContainSingle();
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == destinationStreetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
