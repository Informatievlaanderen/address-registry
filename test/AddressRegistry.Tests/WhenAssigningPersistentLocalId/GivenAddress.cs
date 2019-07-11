namespace AddressRegistry.Tests.WhenAssigningPersistentLocalId
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void WhenForHouseNumberIdThenPersistentLocalIdWasAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AssignPersistentLocalIdForCrabHouseNumberId assignPersistentLocalIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(assignPersistentLocalIdForCrabHouseNumberId)
                .Then(addressId,
                    new AddressPersistentLocalIdWasAssigned(addressId, assignPersistentLocalIdForCrabHouseNumberId.PersistentLocalId, assignPersistentLocalIdForCrabHouseNumberId.AssignmentDate)));
        }


        [Theory, DefaultData]
        public void WhenForHouseNumberIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressPersistentLocalIdWasAssigned addressPersistentLocalIdWasAssigned,
            AssignPersistentLocalIdForCrabHouseNumberId assignPersistentLocalIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressPersistentLocalIdWasAssigned)
                .When(assignPersistentLocalIdForCrabHouseNumberId)
                .ThenNone());
        }

        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdThenPersistentLocalIdWasAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AssignPersistentLocalIdForCrabSubaddressId assignPersistentLocalIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(assignPersistentLocalIdForCrabSubaddressId)
                .Then(addressId,
                    new AddressPersistentLocalIdWasAssigned(addressId, assignPersistentLocalIdForCrabSubaddressId.PersistentLocalId, assignPersistentLocalIdForCrabSubaddressId.AssignmentDate)));
        }


        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressPersistentLocalIdWasAssigned addressPersistentLocalIdWasAssigned,
            AssignPersistentLocalIdForCrabSubaddressId assignPersistentLocalIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressPersistentLocalIdWasAssigned)
                .When(assignPersistentLocalIdForCrabSubaddressId)
                .ThenNone());
        }
    }
}
