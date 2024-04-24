namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressRemoval
{
    using System.Linq;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AddressRegistry.StreetName.Events;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.AutoFixture;
    using AddressRegistry.Tests.EventExtensions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithRemovedAddress_ThenAddressRemovalWasCorrected()
        {
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressRemoval>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .Then(new Fact(
                    _streamId,
                    new AddressRemovalWasCorrected(
                        command.StreetNamePersistentLocalId,
                        command.AddressPersistentLocalId,
                        addressWasMigrated.Status,
                        !string.IsNullOrEmpty(addressWasMigrated.PostalCode) ? new PostalCode(addressWasMigrated.PostalCode) : null,
                        new HouseNumber(addressWasMigrated.HouseNumber),
                        !string.IsNullOrEmpty(addressWasMigrated.BoxNumber) ? new BoxNumber(addressWasMigrated.BoxNumber) : null,
                        addressWasMigrated.GeometryMethod,
                        addressWasMigrated.GeometrySpecification,
                        new ExtendedWkbGeometry(addressWasMigrated.ExtendedWkbGeometry.ToByteArray()),
                        addressWasMigrated.OfficiallyAssigned,
                        addressWasMigrated.ParentPersistentLocalId.HasValue ? new AddressPersistentLocalId(addressWasMigrated.ParentPersistentLocalId.Value) : null
                        ))));
        }

        [Fact]
        public void WithNotRemovedAddress_ThenNothing()
        {
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress();

            var command = Fixture.Create<CorrectAddressRemoval>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithNonExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<CorrectAddressRemoval>();

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WithDifferentHouseNumberThanParentAddress_ThenThrowsBoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithHouseNumber(new HouseNumber("1"));

            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Current)
                .WithHouseNumber(new HouseNumber("2"))
                .WithRemoved();

            var command = new CorrectAddressRemoval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()));
        }

        [Fact]
        public void WithDifferentPostalCodeFromParentAddress_ThenThrowsBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithPostalCode(new PostalCode("1000"));

            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Current)
                .WithPostalCode(new PostalCode("2000"))
                .WithRemoved();

            var command = new CorrectAddressRemoval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()));
        }

        [Fact]
        public void WithRemovedParentAddress_ThenThrowsParentAddressNotFoundException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var boxNumberAddressPersistentLocalId = new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(boxNumberAddressPersistentLocalId)
                .WithRemoved();

            var command = new CorrectAddressRemoval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new ParentAddressNotFoundException(Fixture.Create<StreetNamePersistentLocalId>(), Fixture.Create<HouseNumber>())));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressRemoval>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    addressWasMigrated)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Proposed, AddressStatus.Current)]
        [InlineData(AddressStatus.Proposed, AddressStatus.Retired)]
        [InlineData(AddressStatus.Rejected, AddressStatus.Retired)]
        public void WithHouseNumberAddressHasInvalidStatus_ThenThrowsParentAddressHasInvalidStatusException(
            AddressStatus houseNumberAddressStatus,
            AddressStatus boxNumberAddressStatus)
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(houseNumberAddressStatus);

            var boxNumberAddressPersistentLocalId = new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(boxNumberAddressPersistentLocalId)
                .WithStatus(boxNumberAddressStatus)
                .WithRemoved();

            var command = new CorrectAddressRemoval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                boxNumberAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithHouseNumberBoxNumberCombinationIsNotUnique_ThrowAddressAlreadyExistsException()
        {
            var houseNumber = new HouseNumber("1");

            var firstAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(houseNumber)
                .WithRemoved();

            var secondAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(houseNumber)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(firstAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Current);

            var command = Fixture.Create<CorrectAddressRemoval>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    secondAddressWasMigrated)
                .When(command)
                .Throws(new AddressAlreadyExistsException(houseNumber, null)));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var addressRemovalWasCorrected = Fixture.Create<AddressRemovalWasCorrected>();

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new object[]
            {
                addressWasMigrated,
                addressRemovalWasCorrected
            });

            // Assert
            var address = sut.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.IsRemoved.Should().BeFalse();
            address.Status.Should().Be(addressRemovalWasCorrected.Status);
            address.PostalCode.Should().Be(!string.IsNullOrEmpty(addressRemovalWasCorrected.PostalCode) ? new PostalCode(addressRemovalWasCorrected.PostalCode) : null);
            address.HouseNumber.Should().Be(new HouseNumber(addressRemovalWasCorrected.HouseNumber));
            address.BoxNumber.Should().Be(!string.IsNullOrEmpty(addressRemovalWasCorrected.BoxNumber) ? new BoxNumber(addressRemovalWasCorrected.BoxNumber) : null);
            address.Geometry.Should().Be(new AddressGeometry(
                addressRemovalWasCorrected.GeometryMethod,
                addressRemovalWasCorrected.GeometrySpecification,
                new ExtendedWkbGeometry(addressRemovalWasCorrected.ExtendedWkbGeometry)));
            address.IsOfficiallyAssigned.Should().Be(addressRemovalWasCorrected.OfficiallyAssigned);
        }
    }
}
