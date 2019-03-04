namespace AddressRegistry.Tests.WhenRequestingOsloId
{
    using System;
    using Address.Commands.Crab;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using AutoFixture;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void WhenForHouseNumberIdThenOsloIdWasRequested(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            RequestOsloIdForCrabHouseNumberId requestOsloIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(requestOsloIdForCrabHouseNumberId)
                .Then(addressId,
                    new AddressOsloIdWasAssigned(addressId, new OsloId(1), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now)))));
        }


        [Theory, DefaultData]
        public void WhenForHouseNumberIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned,
            RequestOsloIdForCrabHouseNumberId requestOsloIdForCrabHouseNumberId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressOsloIdWasAssigned)
                .When(requestOsloIdForCrabHouseNumberId)
                .ThenNone());
        }

        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdThenOsloIdWasRequested(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            RequestOsloIdForCrabSubaddressId requestOsloIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(requestOsloIdForCrabSubaddressId)
                .Then(addressId,
                    new AddressOsloIdWasAssigned(addressId, new OsloId(1), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now)))));
        }


        [Theory, DefaultDataForSubaddress]
        public void WhenForSubaddressIdAndAlreadyAssignedThenNothingHappens(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned,
            RequestOsloIdForCrabSubaddressId requestOsloIdForCrabSubaddressId)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressOsloIdWasAssigned)
                .When(requestOsloIdForCrabSubaddressId)
                .ThenNone());
        }
    }
}
