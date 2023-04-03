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
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
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

            var destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(2);
            var destinationStreetNameStreamId = new StreetNameStreamId(destinationStreetNamePersistentLocalId);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(2);
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

            Assert(new Scenario()
                .Given(destinationStreetNameStreamId,
                    Fixture.Create<StreetNameWasImported>().WithStreetNamePersistentLocalId(destinationStreetNamePersistentLocalId),
                    destinationAddressWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(destinationStreetNameStreamId,
                        new StreetNameWasReaddressed(
                            destinationStreetNamePersistentLocalId,
                            new List<AddressPersistentLocalId>(),
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
                                    parentAddressPersistentLocalId: null)
                            }))
                }));

            command.ExecutionContext.AddressesAdded.Should().BeEmpty();
            command.ExecutionContext.AddressesUpdated.Should().ContainSingle(x =>
                x.streetNamePersistentLocalId == destinationStreetNamePersistentLocalId
                && x.addressPersistentLocalId == destinationAddressPersistentLocalId);
        }
    }
}
