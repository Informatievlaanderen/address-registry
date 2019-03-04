namespace AddressRegistry.Tests.WhenAssigningOsloId
{
    using Address.Commands.Crab;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void WhenForHouseNumberIdThenOsloIdWasAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AssignOsloIdForCrabHouseNumberId assignOsloIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(assignOsloIdForCrabHouseNumberId)
                .Then(addressId,
                    new AddressOsloIdWasAssigned(addressId, assignOsloIdForCrabHouseNumberId.OsloId, assignOsloIdForCrabHouseNumberId.AssignmentDate)));
        }


        [Theory, DefaultData]
        public void WhenForHouseNumberIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned,
            AssignOsloIdForCrabHouseNumberId assignOsloIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressOsloIdWasAssigned)
                .When(assignOsloIdForCrabHouseNumberId)
                .ThenNone());
        }

        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdThenOsloIdWasAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AssignOsloIdForCrabSubaddressId assignOsloIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(assignOsloIdForCrabSubaddressId)
                .Then(addressId,
                    new AddressOsloIdWasAssigned(addressId, assignOsloIdForCrabSubaddressId.OsloId, assignOsloIdForCrabSubaddressId.AssignmentDate)));
        }


        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned,
            AssignOsloIdForCrabSubaddressId assignOsloIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressOsloIdWasAssigned)
                .When(assignOsloIdForCrabSubaddressId)
                .ThenNone());
        }
    }
}
