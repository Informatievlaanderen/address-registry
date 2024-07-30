namespace AddressRegistry.Tests.AggregateTests.WhenRetiringStreetNameBecauseOfMunicipalityMerger
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
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
        public void ThenStreetNameWasRetired()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = new RetireStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, streetNameWasImported)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            []))));
        }

        [Fact]
        public void WithAlreadyRetiredStreetName_ThenNone()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = new RetireStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Retired);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithActiveAddressess_ThenAddressesAreRejectedAndRetiredInOrder()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var newStreetNamePersistentLocalId = new StreetNamePersistentLocalId(streetNamePersistentLocalId + 1);

            var oldProposedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var oldProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var oldCurrentHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var oldCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(4);

            var oldProposedNotMergedAddressPersistentLocalId = new AddressPersistentLocalId(20);
            var oldCurrentNotMergedAddressPersistentLocalId = new AddressPersistentLocalId(21);

            var newProposedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(5);
            var newProposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(6);
            var newCurrentHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(7);
            var newCurrentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(8);

            var newStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            newStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(newStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(), StreetNameStatus.Current),

                Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                    .AsHouseNumberAddress()
                    .WithStreetNamePersistentLocalId(newStreetNamePersistentLocalId)
                    .WithAddressPersistentLocalId(newProposedHouseNumberAddressPersistentLocalId)
                    .WithMergedAddressPersistentLocalId(oldProposedHouseNumberAddressPersistentLocalId)
                    .WithDesiredStatus(AddressStatus.Proposed),
                Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                    .AsBoxNumberAddress(newProposedHouseNumberAddressPersistentLocalId)
                    .WithStreetNamePersistentLocalId(newStreetNamePersistentLocalId)
                    .WithAddressPersistentLocalId(newProposedBoxNumberAddressPersistentLocalId)
                    .WithMergedAddressPersistentLocalId(oldProposedBoxNumberAddressPersistentLocalId)
                    .WithDesiredStatus(AddressStatus.Proposed),

                Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                    .AsHouseNumberAddress()
                    .WithStreetNamePersistentLocalId(newStreetNamePersistentLocalId)
                    .WithAddressPersistentLocalId(newCurrentHouseNumberAddressPersistentLocalId)
                    .WithMergedAddressPersistentLocalId(oldCurrentHouseNumberAddressPersistentLocalId)
                    .WithDesiredStatus(AddressStatus.Current),
                Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                    .AsBoxNumberAddress(newCurrentHouseNumberAddressPersistentLocalId)
                    .WithStreetNamePersistentLocalId(newStreetNamePersistentLocalId)
                    .WithAddressPersistentLocalId(newCurrentBoxNumberAddressPersistentLocalId)
                    .WithMergedAddressPersistentLocalId(oldCurrentBoxNumberAddressPersistentLocalId)
                    .WithDesiredStatus(AddressStatus.Current)
            });

            var streetNames = Container.Resolve<IStreetNames>();
            streetNames.Add(new StreetNameStreamId(newStreetNamePersistentLocalId), newStreetName);

            var command = new RetireStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [newStreetNamePersistentLocalId],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(oldProposedHouseNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasProposedV2>()
                        .AsBoxNumberAddress(oldProposedHouseNumberAddressPersistentLocalId)
                        .WithAddressPersistentLocalId(oldProposedBoxNumberAddressPersistentLocalId),

                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(oldCurrentHouseNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(oldCurrentHouseNumberAddressPersistentLocalId),

                    Fixture.Create<AddressWasProposedV2>()
                        .AsBoxNumberAddress(oldCurrentHouseNumberAddressPersistentLocalId)
                        .WithAddressPersistentLocalId(oldCurrentBoxNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(oldCurrentBoxNumberAddressPersistentLocalId),

                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(oldProposedNotMergedAddressPersistentLocalId),

                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(oldCurrentNotMergedAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(oldCurrentNotMergedAddressPersistentLocalId)
                    )
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            oldProposedBoxNumberAddressPersistentLocalId,
                            newProposedBoxNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            oldCurrentBoxNumberAddressPersistentLocalId,
                            newCurrentBoxNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            oldProposedHouseNumberAddressPersistentLocalId,
                            newProposedHouseNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            oldProposedNotMergedAddressPersistentLocalId,
                            null)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            oldCurrentHouseNumberAddressPersistentLocalId,
                            newCurrentHouseNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            oldCurrentNotMergedAddressPersistentLocalId,
                            null)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            [newStreetNamePersistentLocalId]))
                    ));
        }

        [Fact]
        public void WithInactiveAddressess_ThenStreetNameWasRetired()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var rejectedAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var retiredAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RetireStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(rejectedAddressPersistentLocalId),
                    Fixture.Create<AddressWasRejected>()
                        .WithAddressPersistentLocalId(rejectedAddressPersistentLocalId),
                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(retiredAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(retiredAddressPersistentLocalId),
                    Fixture.Create<AddressWasRetiredV2>()
                        .WithAddressPersistentLocalId(retiredAddressPersistentLocalId))
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            []))
                    ));
        }

        [Fact]
        public void StateCheck()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalIdOne = new AddressPersistentLocalId(1);
            var addressPersistentLocalIdTwo = new AddressPersistentLocalId(2);

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasRetired = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                []);
            ((ISetProvenance)streetNameWasRetired).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                streetNameWasImported,
                Fixture.Create<AddressWasProposedV2>()
                    .AsHouseNumberAddress()
                    .WithAddressPersistentLocalId(addressPersistentLocalIdOne),
                Fixture.Create<AddressWasProposedV2>()
                    .AsHouseNumberAddress()
                    .WithAddressPersistentLocalId(addressPersistentLocalIdTwo),
                    new AddressWasRetiredBecauseOfMunicipalityMerger(
                        streetNamePersistentLocalId,
                        addressPersistentLocalIdOne,
                        Fixture.Create<AddressPersistentLocalId>()),
                    new AddressWasRejectedBecauseOfMunicipalityMerger(
                        streetNamePersistentLocalId,
                        addressPersistentLocalIdTwo,
                        Fixture.Create<AddressPersistentLocalId>()),
                streetNameWasRetired
            });

            sut.Status.Should().Be(StreetNameStatus.Retired);

            var retiredAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == addressPersistentLocalIdOne);
            var rejectedAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == addressPersistentLocalIdTwo);

            retiredAddress.Status.Should().Be(AddressStatus.Retired);
            rejectedAddress.Status.Should().Be(AddressStatus.Rejected);
        }
    }
}
