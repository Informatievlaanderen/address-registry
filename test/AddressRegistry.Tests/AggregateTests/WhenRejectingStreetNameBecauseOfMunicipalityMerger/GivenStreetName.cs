namespace AddressRegistry.Tests.AggregateTests.WhenRejectingStreetNameBecauseOfMunicipalityMerger
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
        public void ThenStreetNameWasRejected()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = new RejectStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Proposed);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, streetNameWasImported)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            []))));
        }

        [Fact]
        public void WithAlreadyRejectedStreetName_ThenNone()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = new RejectStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Rejected);
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

            var proposedHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var proposedBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var currentHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var currentBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(4);

            var newHouseNumberAddressPersistentLocalId = new AddressPersistentLocalId(5);
            var newBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(6);

            var newStreetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            newStreetName.Initialize(new List<object>
            {
                new StreetNameWasImported(newStreetNamePersistentLocalId, Fixture.Create<MunicipalityId>(), StreetNameStatus.Proposed),
                Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                    .AsHouseNumberAddress()
                    .WithStreetNamePersistentLocalId(newStreetNamePersistentLocalId)
                    .WithAddressPersistentLocalId(newHouseNumberAddressPersistentLocalId)
                    .WithMergedAddressPersistentLocalId(currentHouseNumberAddressPersistentLocalId),
                Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                    .AsBoxNumberAddress(newHouseNumberAddressPersistentLocalId)
                    .WithStreetNamePersistentLocalId(newStreetNamePersistentLocalId)
                    .WithAddressPersistentLocalId(newBoxNumberAddressPersistentLocalId)
                    .WithMergedAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
            });

            var streetNames = Container.Resolve<IStreetNames>();
            streetNames.Add(new StreetNameStreamId(newStreetNamePersistentLocalId), newStreetName);

            var command = new RejectStreetNameBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                [newStreetNamePersistentLocalId],
                Fixture.Create<Provenance>());

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Proposed);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(proposedHouseNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasProposedV2>()
                        .AsBoxNumberAddress(proposedHouseNumberAddressPersistentLocalId)
                        .WithAddressPersistentLocalId(proposedBoxNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasProposedV2>()
                        .AsHouseNumberAddress()
                        .WithAddressPersistentLocalId(currentHouseNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasProposedV2>()
                        .AsBoxNumberAddress(currentHouseNumberAddressPersistentLocalId)
                        .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(currentHouseNumberAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(currentBoxNumberAddressPersistentLocalId)
                    )
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            proposedBoxNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            currentBoxNumberAddressPersistentLocalId,
                            newBoxNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            proposedHouseNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            currentHouseNumberAddressPersistentLocalId,
                            newHouseNumberAddressPersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            [newStreetNamePersistentLocalId]))
                    ));
        }

        [Fact]
        public void WithInactiveAddressess_ThenStreetNameWasRejected()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var rejectedAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var retiredAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RejectStreetNameBecauseOfMunicipalityMerger(
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
                        new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                            streetNamePersistentLocalId,
                            []))
                    ));
        }

        [Fact]
        public void WithActiveAddressessAndNewAddressNotFound_ThenThrowsMunicipalityMergerAddressIsNotFoundException()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var currentAddressPersistentLocalId = new AddressPersistentLocalId(1);

            var command = new RejectStreetNameBecauseOfMunicipalityMerger(
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
                        .WithAddressPersistentLocalId(currentAddressPersistentLocalId),
                    Fixture.Create<AddressWasApproved>()
                        .WithAddressPersistentLocalId(currentAddressPersistentLocalId))
                .When(command)
                .Throws(new MunicipalityMergerAddressIsNotFoundException(currentAddressPersistentLocalId)));
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
                StreetNameStatus.Proposed);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var streetNameWasRejected = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                []);
            ((ISetProvenance)streetNameWasRejected).SetProvenance(Fixture.Create<Provenance>());

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
                        addressPersistentLocalIdTwo),
                streetNameWasRejected
            });

            sut.Status.Should().Be(StreetNameStatus.Rejected);

            var retiredAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == addressPersistentLocalIdOne);
            var rejectedAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == addressPersistentLocalIdTwo);

            retiredAddress.Status.Should().Be(AddressStatus.Retired);
            rejectedAddress.Status.Should().Be(AddressStatus.Rejected);
        }
    }
}
