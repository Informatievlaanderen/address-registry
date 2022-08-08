namespace AddressRegistry.Tests.AggregateTests.WhenRetiringStreetName;

using System.Collections.Generic;
using System.Linq;
using StreetName;
using StreetName.Commands;
using StreetName.Events;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using global::AutoFixture;
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

        var command = Fixture.Create<RetireStreetName>();

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
                    new StreetNameWasRetired(streetNamePersistentLocalId))));
    }

    [Fact]
    public void WithAlreadyRetiredStreetName_ThenNone()
    {
        var command = Fixture.Create<RetireStreetName>();

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
    public void WithProposedAddresses_ThenAddressesWereRejected()
    {
        var command = Fixture.Create<RetireStreetName>();

        var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        var streetNameWasImported = new StreetNameWasImported(
            streetNamePersistentLocalId,
            Fixture.Create<MunicipalityId>(),
            StreetNameStatus.Current);
        ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

        var firstAddressPersistentLocalId = new AddressPersistentLocalId(123);
        var firstAddressWasProposedV2 = new AddressWasProposedV2(
            streetNamePersistentLocalId,
            firstAddressPersistentLocalId,
            parentPersistentLocalId: null,
            Fixture.Create<PostalCode>(),
            Fixture.Create<HouseNumber>(),
            boxNumber: null);
        ((ISetProvenance)firstAddressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

        var secondAddressPersistentLocalId = new AddressPersistentLocalId(456);
        var secondAddressWasProposedV2 = new AddressWasProposedV2(
            streetNamePersistentLocalId,
            secondAddressPersistentLocalId,
            parentPersistentLocalId: null,
            Fixture.Create<PostalCode>(),
            Fixture.Create<HouseNumber>(),
            boxNumber: null);
        ((ISetProvenance)secondAddressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

        Assert(new Scenario()
            .Given(_streamId,
                streetNameWasImported,
                firstAddressWasProposedV2,
                secondAddressWasProposedV2)
            .When(command)
            .Then(
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasRetired(streetNamePersistentLocalId)),
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new AddressWasRejectedBecauseStreetNameWasRetired(streetNamePersistentLocalId, firstAddressPersistentLocalId)),
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new AddressWasRejectedBecauseStreetNameWasRetired(streetNamePersistentLocalId, secondAddressPersistentLocalId))));
    }

    [Fact]
    public void WithCurrentAddresses_ThenAddressesWereRetired()
    {
        var command = Fixture.Create<RetireStreetName>();

        var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        var streetNameWasImported = new StreetNameWasImported(
            streetNamePersistentLocalId,
            Fixture.Create<MunicipalityId>(),
            StreetNameStatus.Current);
        ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

        var firstAddressPersistentLocalId = new AddressPersistentLocalId(123);
        var firstAddressWasProposedV2 = new AddressWasProposedV2(
            streetNamePersistentLocalId,
            firstAddressPersistentLocalId,
            parentPersistentLocalId: null,
            Fixture.Create<PostalCode>(),
            Fixture.Create<HouseNumber>(),
            boxNumber: null);
        ((ISetProvenance)firstAddressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
        var firstAddressWasApproved = new AddressWasApproved(
            streetNamePersistentLocalId,
            firstAddressPersistentLocalId);
        ((ISetProvenance)firstAddressWasApproved).SetProvenance(Fixture.Create<Provenance>());

        var secondAddressPersistentLocalId = new AddressPersistentLocalId(456);
        var secondAddressWasProposedV2 = new AddressWasProposedV2(
            streetNamePersistentLocalId,
            secondAddressPersistentLocalId,
            parentPersistentLocalId: null,
            Fixture.Create<PostalCode>(),
            Fixture.Create<HouseNumber>(),
            boxNumber: null);
        ((ISetProvenance)secondAddressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
        var secondAddressWasApproved = new AddressWasApproved(
            streetNamePersistentLocalId,
            secondAddressPersistentLocalId);
        ((ISetProvenance)secondAddressWasApproved).SetProvenance(Fixture.Create<Provenance>());

        Assert(new Scenario()
            .Given(_streamId,
                streetNameWasImported,
                firstAddressWasProposedV2,
                firstAddressWasApproved,
                secondAddressWasProposedV2,
                secondAddressWasApproved)
            .When(command)
            .Then(
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasRetired(streetNamePersistentLocalId)),
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new AddressWasRetiredBecauseStreetNameWasRetired(streetNamePersistentLocalId, firstAddressPersistentLocalId)),
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new AddressWasRetiredBecauseStreetNameWasRetired(streetNamePersistentLocalId, secondAddressPersistentLocalId))));
    }

    [Fact]
    public void WithRetiredAddress_ThenNoChangeOnAddress()
    {
        var command = Fixture.Create<RetireStreetName>();

        var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        var streetNameWasImported = new StreetNameWasImported(
            streetNamePersistentLocalId,
            Fixture.Create<MunicipalityId>(),
            StreetNameStatus.Current);
        ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

        var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
        var addressWasProposedV2 = new AddressWasProposedV2(
            streetNamePersistentLocalId,
            addressPersistentLocalId,
            parentPersistentLocalId: null,
            Fixture.Create<PostalCode>(),
            Fixture.Create<HouseNumber>(),
            boxNumber: null);
        ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
        var addressWasApproved = new AddressWasApproved(
            streetNamePersistentLocalId,
            addressPersistentLocalId);
        ((ISetProvenance)addressWasApproved).SetProvenance(Fixture.Create<Provenance>());
        var addressWasRetiredV2 = new AddressWasRetiredV2(
            streetNamePersistentLocalId,
            addressPersistentLocalId);
        ((ISetProvenance)addressWasRetiredV2).SetProvenance(Fixture.Create<Provenance>());

        Assert(new Scenario()
            .Given(_streamId,
                streetNameWasImported,
                addressWasProposedV2,
                addressWasApproved,
                addressWasRetiredV2)
            .When(command)
            .Then(
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasRetired(streetNamePersistentLocalId))));
    }

    [Fact]
    public void WithRejectedAddress_ThenNoChangeOnAddress()
    {
        var command = Fixture.Create<RetireStreetName>();

        var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        var streetNameWasImported = new StreetNameWasImported(
            streetNamePersistentLocalId,
            Fixture.Create<MunicipalityId>(),
            StreetNameStatus.Current);
        ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

        var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
        var addressWasProposedV2 = new AddressWasProposedV2(
            streetNamePersistentLocalId,
            addressPersistentLocalId,
            parentPersistentLocalId: null,
            Fixture.Create<PostalCode>(),
            Fixture.Create<HouseNumber>(),
            boxNumber: null);
        ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
        var addressWasRejected = new AddressWasRejected(
            streetNamePersistentLocalId,
            addressPersistentLocalId);
        ((ISetProvenance)addressWasRejected).SetProvenance(Fixture.Create<Provenance>());

        Assert(new Scenario()
            .Given(_streamId,
                streetNameWasImported,
                addressWasProposedV2,
                addressWasRejected)
            .When(command)
            .Then(
                new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasRetired(streetNamePersistentLocalId))));
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
        var proposedAddressPersistentLocalId = new AddressPersistentLocalId(123);
        var currentAddressPersistentLocalId = new AddressPersistentLocalId(456);

        var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
            Fixture.Create<StreetNameId>(),
            streetNamePersistentLocalId,
            Fixture.Create<MunicipalityId>(),
            Fixture.Create<NisCode>(),
            StreetNameStatus.Current);
        ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

        var proposedAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
            streetNamePersistentLocalId,
            Fixture.Create<AddressId>(),
            Fixture.Create<AddressStreetNameId>(),
            proposedAddressPersistentLocalId,
            AddressStatus.Proposed,
            Fixture.Create<HouseNumber>(),
            boxNumber: null,
            Fixture.Create<AddressGeometry>(),
            officiallyAssigned: true,
            postalCode: null,
            isCompleted: false,
            isRemoved: false,
            parentPersistentLocalId: null);
        ((ISetProvenance)proposedAddressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

        var currentAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
            streetNamePersistentLocalId,
            Fixture.Create<AddressId>(),
            Fixture.Create<AddressStreetNameId>(),
            currentAddressPersistentLocalId,
            AddressStatus.Current,
            Fixture.Create<HouseNumber>(),
            boxNumber: null,
            Fixture.Create<AddressGeometry>(),
            officiallyAssigned: true,
            postalCode: null,
            isCompleted: false,
            isRemoved: false,
            parentPersistentLocalId: null);
        ((ISetProvenance)currentAddressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

        var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
        sut.Initialize(new List<object> { migratedStreetNameWasImported, proposedAddressWasMigratedToStreetName, currentAddressWasMigratedToStreetName });

        // Act
        sut.RetireStreetName();

        // Assert
        sut.Status.Should().Be(StreetNameStatus.Retired);

        var proposedAddressWhichBecameRejected = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == proposedAddressPersistentLocalId);
        var currentAddressWhichBecameRetired = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == currentAddressPersistentLocalId);

        proposedAddressWhichBecameRejected.Status.Should().Be(AddressStatus.Rejected);
        currentAddressWhichBecameRetired.Status.Should().Be(AddressStatus.Retired);
    }
}
