namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressPostalCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressPostalCodeWasCorrected()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var command = Fixture.Create<CorrectAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPostalCodeWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Array.Empty<AddressPersistentLocalId>(),
                        command.PostalCode))));
        }

        [Fact]
        public void WithBoxNumberAddresses_ThenBoxNumberAddressPostalCodesWereAlsoChanged()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasMigrated.HouseNumber))
                .WithStatus(AddressStatus.Current);

            var command = Fixture.Create<CorrectAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Then(new Fact(
                    _streamId,
                    new AddressPostalCodeWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId),
                        new[] { new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId) },
                        command.PostalCode))));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<CorrectAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()))
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WithAddressHasBoxNumber_ThenThrowsAddresHasBoxNumberException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasMigrated.HouseNumber))
                .WithStatus(AddressStatus.Current);

            var command = new CorrectAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new AddressHasBoxNumberException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void OnStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

            var command = Fixture.Create<CorrectAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>()))
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus);

            var command = Fixture.Create<CorrectAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException(command.AddressPersistentLocalId)));
        }

        [Fact]
        public void WithMunicipalityDifferentFromCommand_ThrowsPostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command = new CorrectAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                new MunicipalityId(Guid.NewGuid()),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException()));
        }

        [Fact]
        public void WithNoChangedPostalCode_ThenNone()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command = new CorrectAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new PostalCode(addressWasMigratedToStreetName.PostalCode!),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                    addressWasMigratedToStreetName)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasMigrated.HouseNumber))
                .WithStatus(AddressStatus.Current);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>().WithMunicipalityId(Fixture.Create<MunicipalityId>()),
                houseNumberAddressWasMigrated,
                boxNumberAddressWasMigrated
            });

            var expectedPostalCode = Fixture.Create<PostalCode>();

            // Act
            sut.CorrectAddressPostalCode(
                new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId),
                expectedPostalCode,
                Fixture.Create<MunicipalityId>());

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x =>
                x.AddressPersistentLocalId == new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId));
            var childAddress = sut.StreetNameAddresses.First(x =>
                x.AddressPersistentLocalId == new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId));

            parentAddress.PostalCode.Should().Be(expectedPostalCode);
            childAddress.PostalCode.Should().Be(expectedPostalCode);
        }
    }
}
