namespace AddressRegistry.Tests.AggregateTests.WhenRenamingStreetName
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class SourceAddressHasBoxNumbers : AddressRegistryTest
    {
        private readonly StreetNameStreamId _sourceStreetNameStreamId;
        private readonly StreetNameStreamId _destinationStreetNameStreamId;

        private readonly StreetNamePersistentLocalId _sourceStreetNamePersistentLocalId;
        private readonly StreetNamePersistentLocalId _destinationStreetNamePersistentLocalId;

        public SourceAddressHasBoxNumbers(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedHouseNumber());
            Fixture.Customize(new WithFixedPostalCode());
            Fixture.Customizations.Add(new WithUniqueInteger());

            _sourceStreetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            _destinationStreetNamePersistentLocalId = new StreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId + 1);

            _sourceStreetNameStreamId = new StreetNameStreamId(_sourceStreetNamePersistentLocalId);
            _destinationStreetNameStreamId = new StreetNameStreamId(_destinationStreetNamePersistentLocalId);
        }

        [Fact]
        public void WithHouseNumberDoesNotExistOnDestinationStreetName_ThenBothHouseNumberAndBoxNumberWereProposed()
        {
            var houseNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithStreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId);

            var boxNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberWasMigrated.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Current)
                .WithStreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId);

            var streetNames = Container.Resolve<IStreetNames>();
            var sourceStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sourceStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(_sourceStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(), StreetNameStatus.Current),
                houseNumberWasMigrated,
                boxNumberWasMigrated
            });

            streetNames.Add(_sourceStreetNameStreamId, sourceStreetName);

            var command = new RenameStreetName(
                _sourceStreetNamePersistentLocalId,
                _destinationStreetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var destinationHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(1); // FakePersistentLocalIdGenerator starts with 1.
            var destinationBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);

            Assert(new Scenario()
                .Given(_destinationStreetNameStreamId,
                    Fixture.Create<StreetNameWasImported>()
                        .WithStreetNamePersistentLocalId(_destinationStreetNamePersistentLocalId))
                .When(command)
                .Then(new[]
                {
                    new Fact(
                        _destinationStreetNameStreamId,
                        CreateAddressWasProposedBecauseOfReaddress(
                            houseNumberWasMigrated, destinationHouseNumberAddressPersistentLocalId)),
                    new Fact(
                        _destinationStreetNameStreamId,
                        CreateAddressWasProposedBecauseOfReaddress(
                            boxNumberWasMigrated, destinationBoxNumberAddressPersistentLocalId, destinationHouseNumberAddressPersistentLocalId)),
                    new Fact(
                        _destinationStreetNameStreamId,
                        new AddressHouseNumberWasReaddressed(
                            _destinationStreetNamePersistentLocalId,
                            destinationHouseNumberAddressPersistentLocalId,
                            CreateReaddressedDataFrom(
                                houseNumberWasMigrated,
                                destinationHouseNumberAddressPersistentLocalId,
                                true),
                            new List<ReaddressedAddressData>
                            {
                                CreateReaddressedDataFrom(
                                    boxNumberWasMigrated,
                                    destinationBoxNumberAddressPersistentLocalId,
                                    true)
                            }))
                }));
        }

        [Fact]
        public void WithHouseNumberExistsButWithoutBoxNumberOnDestinationStreetName_ThenBoxNumberIsProposed()
        {
            var houseNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithStreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId);

            var boxNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberWasMigrated.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Current)
                .WithStreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId);

            var streetNames = Container.Resolve<IStreetNames>();
            var sourceStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sourceStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(_sourceStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(), StreetNameStatus.Current),
                houseNumberWasMigrated,
                boxNumberWasMigrated
            });

            streetNames.Add(_sourceStreetNameStreamId, sourceStreetName);

            var command = new RenameStreetName(
                _sourceStreetNamePersistentLocalId,
                _destinationStreetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var destinationHouseNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStreetNamePersistentLocalId(_destinationStreetNamePersistentLocalId);

            Assert(new Scenario()
                .Given(_destinationStreetNameStreamId,
                    Fixture.Create<StreetNameWasImported>()
                        .WithStreetNamePersistentLocalId(_destinationStreetNamePersistentLocalId),
                    destinationHouseNumberWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(
                        _destinationStreetNameStreamId,
                        CreateAddressWasProposedBecauseOfReaddress(
                            boxNumberWasMigrated, new AddressPersistentLocalId(1), new AddressPersistentLocalId(destinationHouseNumberWasMigrated.AddressPersistentLocalId))),
                    new Fact(
                        _destinationStreetNameStreamId,
                        new AddressHouseNumberWasReaddressed(
                            _destinationStreetNamePersistentLocalId,
                            new AddressPersistentLocalId(destinationHouseNumberWasMigrated.AddressPersistentLocalId),
                            CreateReaddressedDataFrom(
                                houseNumberWasMigrated,
                                new AddressPersistentLocalId(destinationHouseNumberWasMigrated.AddressPersistentLocalId),
                                false),
                            new List<ReaddressedAddressData>
                            {
                                CreateReaddressedDataFrom(
                                    boxNumberWasMigrated,
                                    new AddressPersistentLocalId(1),
                                    true)
                            }))
                }));
        }

        [Fact]
        public void WithHouseNumberAndBoxNumberExistOnDestinationStreetName_ThenBothAreReaddressed()
        {
            var houseNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithStreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId);

            var boxNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberWasMigrated.AddressPersistentLocalId))
                .WithStatus(AddressStatus.Current)
                .WithStreetNamePersistentLocalId(_sourceStreetNamePersistentLocalId);

            var streetNames = Container.Resolve<IStreetNames>();
            var sourceStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sourceStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(_sourceStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(), StreetNameStatus.Current),
                houseNumberWasMigrated,
                boxNumberWasMigrated
            });

            streetNames.Add(_sourceStreetNameStreamId, sourceStreetName);

            var command = new RenameStreetName(
                _sourceStreetNamePersistentLocalId,
                _destinationStreetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var destinationHouseNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStreetNamePersistentLocalId(_destinationStreetNamePersistentLocalId);

            var destinationBoxNumberWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                    .AsBoxNumberAddress(
                        new AddressPersistentLocalId(destinationHouseNumberWasMigrated.AddressPersistentLocalId),
                        new BoxNumber(boxNumberWasMigrated.BoxNumber!))
                    .WithStreetNamePersistentLocalId(_destinationStreetNamePersistentLocalId);

            Assert(new Scenario()
                .Given(_destinationStreetNameStreamId,
                    Fixture.Create<StreetNameWasImported>()
                        .WithStreetNamePersistentLocalId(_destinationStreetNamePersistentLocalId),
                    destinationHouseNumberWasMigrated,
                    destinationBoxNumberWasMigrated)
                .When(command)
                .Then(new[]
                {
                    new Fact(
                        _destinationStreetNameStreamId,
                        new AddressHouseNumberWasReaddressed(
                            _destinationStreetNamePersistentLocalId,
                            new AddressPersistentLocalId(destinationHouseNumberWasMigrated.AddressPersistentLocalId),
                            CreateReaddressedDataFrom(
                                houseNumberWasMigrated,
                                new AddressPersistentLocalId(destinationHouseNumberWasMigrated.AddressPersistentLocalId),
                                false),
                            new List<ReaddressedAddressData>
                            {
                                CreateReaddressedDataFrom(
                                    boxNumberWasMigrated,
                                    new AddressPersistentLocalId(destinationBoxNumberWasMigrated.AddressPersistentLocalId),
                                    false)
                            }))
                }));
        }

        private AddressWasProposedBecauseOfReaddress CreateAddressWasProposedBecauseOfReaddress(
            AddressWasMigratedToStreetName sourceAddressWasMigrated,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            AddressPersistentLocalId? destinationParentAddressPersistentLocalId = null)
        {
            return new AddressWasProposedBecauseOfReaddress(
                _destinationStreetNamePersistentLocalId,
                destinationAddressPersistentLocalId,
                new AddressPersistentLocalId(sourceAddressWasMigrated.AddressPersistentLocalId),
                destinationParentAddressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                sourceAddressWasMigrated.BoxNumber != null ? new BoxNumber(sourceAddressWasMigrated.BoxNumber) : null,
                sourceAddressWasMigrated.GeometryMethod,
                sourceAddressWasMigrated.GeometrySpecification,
                new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry));
        }

        private ReaddressedAddressData CreateReaddressedDataFrom(
            AddressWasMigratedToStreetName sourceAddressWasMigrated,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            bool isDestinationNewlyProposed)
        {
            return new ReaddressedAddressData(
                new AddressPersistentLocalId(sourceAddressWasMigrated.AddressPersistentLocalId),
                destinationAddressPersistentLocalId,
                isDestinationNewlyProposed,
                sourceAddressWasMigrated.Status,
                Fixture.Create<HouseNumber>(),
                sourceAddressWasMigrated.BoxNumber != null ? new BoxNumber(sourceAddressWasMigrated.BoxNumber) : null,
                Fixture.Create<PostalCode>(),
                new AddressGeometry(
                    sourceAddressWasMigrated.GeometryMethod,
                    sourceAddressWasMigrated.GeometrySpecification,
                    new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry)),
                sourceAddressWasMigrated.OfficiallyAssigned);
        }
    }
}
