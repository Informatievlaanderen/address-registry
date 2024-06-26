namespace AddressRegistry.Tests.AggregateTests.WhenReaddress.GivenSourceAddressInOtherStreetName
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventBuilders;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class SourceAddressHasNoBoxNumbers: AddressRegistryTest
    {
        public SourceAddressHasNoBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
        }

        [Fact]
        public void ThenSourceAddressWasReaddressed()
        {
            var sourceHouseNumber = new HouseNumber("100");
            var destinationHouseNumber = new HouseNumber("5");

            var sourceStreetNamePersistentLocalId = new StreetNamePersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(2);
            var destinationStreetNameStreamId = new StreetNameStreamId(destinationStreetNamePersistentLocalId);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(101);

            var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(sourceStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
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
            });

            var streetNames = Container.Resolve<IStreetNames>();
            streetNames.Add(new StreetNameStreamId(sourceStreetNamePersistentLocalId), sourceStreetName);

            var destinationAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(destinationStreetNamePersistentLocalId)
                .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
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
                new List<RetireAddressItem>(),
                Fixture.Create<Provenance>());

            var expectedAddressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
                destinationStreetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                readdressedHouseNumber: new ReaddressedAddressData(
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
                readdressedBoxNumbers: new List<ReaddressedAddressData>());

            Assert(new Scenario()
                .Given(destinationStreetNameStreamId,
                    Fixture.Create<StreetNameWasImported>().WithStreetNamePersistentLocalId(destinationStreetNamePersistentLocalId),
                    destinationAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(destinationStreetNameStreamId, expectedAddressHouseNumberWasReaddressed),
                    new Fact(destinationStreetNameStreamId, new StreetNameWasReaddressed(destinationStreetNamePersistentLocalId, new List<AddressHouseNumberWasReaddressed>{ expectedAddressHouseNumberWasReaddressed }))
                }));

            command.ExecutionContext.AddressesAdded.Should().BeEmpty();
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == destinationStreetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
