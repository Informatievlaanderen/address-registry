namespace AddressRegistry.Tests.AggregateTests.WhenRemovingStreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using ProjectionTests.Legacy.Extensions;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;
    using StreetNameWasRemoved = StreetName.Events.StreetNameWasRemoved;

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
        public void ThenStreetNameAndAddressesWereRemoved()
        {
            var command = Fixture.Create<RemoveStreetName>();

            var parentAddressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(123),
                parentPersistentLocalId: null,
                new PostalCode("1337"),
                new HouseNumber("11"),
                boxNumber: null,
                Fixture.Create<GeometryMethod>(),
                Fixture.Create<GeometrySpecification>(),
                Fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)parentAddressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(456),
                parentPersistentLocalId: new AddressPersistentLocalId(parentAddressWasProposedV2.AddressPersistentLocalId),
                new PostalCode("1337"),
                new HouseNumber("11"),
                boxNumber: new BoxNumber("A1"),
                Fixture.Create<GeometryMethod>(),
                Fixture.Create<GeometrySpecification>(),
                Fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)childAddressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var childAddress2WasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(789),
                parentPersistentLocalId: new AddressPersistentLocalId(parentAddressWasProposedV2.AddressPersistentLocalId),
                new PostalCode("1337"),
                new HouseNumber("11"),
                boxNumber: new BoxNumber("A1"),
                Fixture.Create<GeometryMethod>(),
                Fixture.Create<GeometrySpecification>(),
                Fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)childAddress2WasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var childAddress2WasRemovedV2 = new AddressWasRemovedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(789));
            ((ISetProvenance)childAddress2WasRemovedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposedV2,
                    childAddressWasProposedV2,
                    childAddress2WasProposedV2,
                    childAddress2WasRemovedV2)
                .When(command)
                .Then(new[]
                {
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRemovedBecauseStreetNameWasRemoved(command.PersistentLocalId,
                            new AddressPersistentLocalId(childAddressWasProposedV2.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRemovedBecauseStreetNameWasRemoved(command.PersistentLocalId,
                            new AddressPersistentLocalId(parentAddressWasProposedV2.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRemoved(command.PersistentLocalId))
                }));
        }

        [Fact]
        public void ThenStreetNameWasRemoved()
        {
            var command = Fixture.Create<RemoveStreetName>();

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRemoved(command.PersistentLocalId))));
        }

        [Fact]
        public void WithAlreadyRemovedStreetName_ThenNone()
        {
            var command = Fixture.Create<RemoveStreetName>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    Fixture.Create<StreetNameWasRemoved>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().WithParentAddressPersistentLocalId(null);
            var addressWasRemovedBecauseStreetNameWasRemoved = new AddressWasRemovedBecauseStreetNameWasRemoved(
                Fixture.Create<StreetNamePersistentLocalId>(), new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId));
            ((ISetProvenance)addressWasRemovedBecauseStreetNameWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                streetNameWasProposedV2,
                addressWasProposedV2,
                addressWasRemovedBecauseStreetNameWasRemoved,
                Fixture.Create<StreetNameWasRemoved>()
            });

            var address = sut.StreetNameAddresses.Single();

            sut.IsRemoved.Should().BeTrue();
            address.IsRemoved.Should().BeTrue();
            address.LastEventHash.Should().Be(addressWasRemovedBecauseStreetNameWasRemoved.GetHash());
        }
    }
}
