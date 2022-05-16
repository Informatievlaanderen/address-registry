namespace AddressRegistry.Tests.AggregateTests.WhenProposingAddress
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithExistingParent_ThenAddressWasProposed()
        {
            var houseNumber = Fixture.Create<HouseNumber>();

            var parentAddressWasProposed = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
                boxNumber: null);

            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                Fixture.Create<BoxNumber>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed)
                .When(proposeChildAddress)
                .Then(
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            proposeChildAddress.StreetNamePersistentLocalId,
                            proposeChildAddress.AddressPersistentLocalId,
                            new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                            proposeChildAddress.PostalCode,
                            proposeChildAddress.HouseNumber,
                            proposeChildAddress.BoxNumber))));
        }


        [Fact]
        public void WithExistingParent_ThenChildAddressWasAddedToStreetNameAddresses()
        {
            var aggregateId = Fixture.Create<StreetNamePersistentLocalId>();
            var aggregate = StreetName.Factory();

            var houseNumber = Fixture.Create<HouseNumber>();

            var parentAddressWasProposed = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
                boxNumber: null);

            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MigratedStreetNameWasImported>(),
                parentAddressWasProposed,
            });

            var childPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var childBoxNumber = Fixture.Create<BoxNumber>();

            // Act
            aggregate.ProposeAddress(
                aggregateId,
                childPersistentLocalId,
                Fixture.Create<PostalCode>(),
                houseNumber,
                childBoxNumber);

            // Assert
            var result = aggregate.StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId));
            result.Should().NotBeNull();
            result.Children.Count.Should().Be(1);
            var child = result.Children.Single();
            child.AddressPersistentLocalId.Should().Be(childPersistentLocalId);
            child.HouseNumber.Should().Be(houseNumber);
            child.Status.Should().Be(AddressStatus.Proposed);
            child.BoxNumber.Should().Be(childBoxNumber);
            child.IsOfficiallyAssigned.Should().BeTrue();
        }

        [Fact]
        public void ChildAddressWithoutExistingParent_ThenThrowParentNotFoundException()
        {
            var houseNumber = Fixture.Create<string>();

            var proposeChildAddress = new ProposeAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber(houseNumber),
                Fixture.Create<BoxNumber>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(proposeChildAddress)
                .Throws(new ParentAddressNotFoundException()));
        }

        [Fact]
        public void ParentAddress_ThenAddressWasProposed()
        {
            var houseNumber = Fixture.Create<string>();

            var proposeParentAddress = new ProposeAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber(houseNumber),
                boxNumber: null,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(proposeParentAddress)
                .Then(
                    new Fact(_streamId,
                        new AddressWasProposedV2(
                            proposeParentAddress.StreetNamePersistentLocalId,
                            proposeParentAddress.AddressPersistentLocalId,
                            parentPersistentLocalId: null,
                            proposeParentAddress.PostalCode,
                            proposeParentAddress.HouseNumber,
                            boxNumber: null))));
        }
    }
}
