namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressRejection
{
    using System.Collections.Generic;
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
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasCorrectedFromRejectedToProposed()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();
            var addressWasRejected = Fixture.Create<AddressWasRejected>();

            var command = Fixture.Create<CorrectAddressRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRejected)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressWasCorrectedFromRejectedToProposed(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>()))));
        }

        [Fact]
        public void WhenAddressIsRemoved_ThrowAddressRemovedException()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();
            var addressWasRemovedV2 = Fixture.Create<AddressWasRemovedV2>();

            var command = Fixture.Create<CorrectAddressRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRemovedV2)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WhenAddressIsProposed_ThenDoNothing()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var command = Fixture.Create<CorrectAddressRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithParentHouseNumberDiffersFromChildHouseNumber_ThenThrowsBoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithHouseNumber(new HouseNumber("403"));

            var boxNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber("404"));

            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasMigratedToStreetName)
                .When(command)
                .Throws(new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()));
        }


        [Fact]
        public void WithParentPostalCodeDiffersFromChildPostalCode_ThenThrowsBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithPostalCode(new PostalCode("9000"));

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithPostalCode(new PostalCode("2000"));

            var command = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .Throws(new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void OnStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>().AsHouseNumberAddress();

            var command = Fixture.Create<CorrectAddressRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Retired)]
        [InlineData(AddressStatus.Unknown)]
        public void WhenAddressHasInvalidStatus_ThrowAddressRejectionCannotBeCorrectedException(AddressStatus status)
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(status);

            var command = Fixture.Create<CorrectAddressRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("1A", "1A")]
        [InlineData("1A", "1a")]
        public void WhenAddressAlreadyExists_ThrowAddressAlreadyExistsException(string firstHouseNumber, string secondHouseNumber)
        {
            var firstAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress(new HouseNumber(firstHouseNumber));
            var firstAddressWasRejected = Fixture.Create<AddressWasRejected>();

            var secondAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress(new HouseNumber(secondHouseNumber))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(firstAddressWasProposed.AddressPersistentLocalId + 1));

            var correctAddressRejection = Fixture.Create<CorrectAddressRejection>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasProposed,
                    firstAddressWasRejected,
                    secondAddressWasProposed)
                .When(correctAddressRejection)
                .Throws(new AddressAlreadyExistsException(new HouseNumber(firstHouseNumber), null)));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WhenParentAddressHasInvalidStatus_ThrowParentAddressHasInvalidStatusException(AddressStatus invalidStatus)
        {
            var houseNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(invalidStatus);

            var boxNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1));

            var correctChildAddressRejection = new CorrectAddressRejection(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigratedToStreetName,
                    boxNumberAddressWasProposed)
                .When(correctChildAddressRejection)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }

        [Fact]
        public void StateCheck()
        {
            var addressWasCorrectedFromRejectedToProposed = Fixture.Create<AddressWasCorrectedFromRejectedToProposed>();

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasProposedV2>(),
                Fixture.Create<AddressWasProposedV2>().WithParentAddressPersistentLocalId(null),
                Fixture.Create<AddressWasRejected>(),
                addressWasCorrectedFromRejectedToProposed
            });

            var address = sut.StreetNameAddresses.Single(x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.Status.Should().Be(AddressStatus.Proposed);
            address.LastEventHash.Should().Be(addressWasCorrectedFromRejectedToProposed.GetHash());
        }
    }
}
