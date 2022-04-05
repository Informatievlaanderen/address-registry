namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenRequestingPersistentLocalId
{
    using System;
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void WhenForHouseNumberIdThenPersistentLocalIdWasRequested(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            RequestPersistentLocalIdForCrabHouseNumberId requestPersistentLocalIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(requestPersistentLocalIdForCrabHouseNumberId)
                .Then(addressId,
                    new AddressPersistentLocalIdWasAssigned(addressId, new PersistentLocalId(1), new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now)))));
        }


        [Theory, DefaultData]
        public void WhenForHouseNumberIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressPersistentLocalIdWasAssigned addressPersistentLocalIdWasAssigned,
            RequestPersistentLocalIdForCrabHouseNumberId requestPersistentLocalIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressPersistentLocalIdWasAssigned)
                .When(requestPersistentLocalIdForCrabHouseNumberId)
                .ThenNone());
        }

        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdThenPersistentLocalIdWasRequested(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            RequestPersistentLocalIdForCrabSubaddressId requestPersistentLocalIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(requestPersistentLocalIdForCrabSubaddressId)
                .Then(addressId,
                    new AddressPersistentLocalIdWasAssigned(addressId, new PersistentLocalId(1), new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now)))));
        }


        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressPersistentLocalIdWasAssigned addressPersistentLocalIdWasAssigned,
            RequestPersistentLocalIdForCrabSubaddressId requestPersistentLocalIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressPersistentLocalIdWasAssigned)
                .When(requestPersistentLocalIdForCrabSubaddressId)
                .ThenNone());
        }
    }
}
