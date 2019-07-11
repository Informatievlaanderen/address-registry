namespace AddressRegistry.Tests.ProjectionTests
{
    using System.Threading.Tasks;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressList;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressListProjectionsTests : ProjectionTest<LegacyContext, AddressListProjections>
    {
        public AddressListProjectionsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRegisteredCreatesAddressListItem(
            AddressWasRegistered addressWasRegistered)
        {
            await Assert(
                Given(addressWasRegistered)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBecameCompleteSetsCompletedTrue(
            AddressWasRegistered addressWasRegistered,
            AddressBecameComplete addressBecameComplete)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBecameComplete)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Complete = true
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBecameIncompleteSetsCompletedFalse(
            AddressWasRegistered addressWasRegistered,
            AddressBecameComplete addressBecameComplete,
            AddressBecameIncomplete addressBecameIncomplete)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBecameComplete,
                        addressBecameIncomplete)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Complete = false
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressHouseNumberWasChangedSetsHouseNumber(
            AddressId addressId,
            Provenance provenance,
            AddressWasRegistered addressWasRegistered)
        {
            var addressHouseNumberWasChanged = new AddressHouseNumberWasChanged(addressId, new HouseNumber("17"));
            ((ISetProvenance)addressHouseNumberWasChanged).SetProvenance(provenance);
            await Assert(
                Given(addressWasRegistered,
                        addressHouseNumberWasChanged)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressHouseNumberWasChanged.HouseNumber
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressHouseNumberWasCorrectedSetsHouseNumber(
            AddressId addressId,
            Provenance provenance,
            AddressWasRegistered addressWasRegistered)
        {
            var addressHouseNumberWasCorrected = new AddressHouseNumberWasCorrected(addressId, new HouseNumber("17"));
            ((ISetProvenance)addressHouseNumberWasCorrected).SetProvenance(provenance);

            await Assert(
                Given(addressWasRegistered,
                        addressHouseNumberWasCorrected)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressHouseNumberWasCorrected.HouseNumber
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPersistentLocalIdWasAssignedSetsPersistentLocalId(
            AddressWasRegistered addressWasRegistered,
            AddressPersistentLocalIdWasAssigned addressPersistentLocalIdWasAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressPersistentLocalIdWasAssigned)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        PersistentLocalId = addressPersistentLocalIdWasAssigned.PersistentLocalId
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPostalCodeWasChangedSetsPostalCode(
            AddressWasRegistered addressWasRegistered,
            AddressPostalCodeWasChanged addressPostalCodeWasChanged)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressPostalCodeWasChanged)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        PostalCode = addressPostalCodeWasChanged.PostalCode
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPostalCodeWasCorrectedSetsPostalCode(
            AddressWasRegistered addressWasRegistered,
            AddressPostalCodeWasCorrected addressPostalCodeWasCorrected)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressPostalCodeWasCorrected)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        PostalCode = addressPostalCodeWasCorrected.PostalCode
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPostalCodeWasRemovedSetsPostalCodeToNull(
            AddressWasRegistered addressWasRegistered,
            AddressPostalCodeWasChanged addressPostalCodeWasChanged,
            AddressPostalCodeWasRemoved addressPostalCodeWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressPostalCodeWasChanged,
                        addressPostalCodeWasRemoved)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        PostalCode = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRemovedSetsRemoved(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Removed = true
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBoxNumberWasChangedChangesBoxNumber(
            AddressWasRegistered addressWasRegistered,
            AddressBoxNumberWasChanged addressBoxNumberWasChanged)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBoxNumberWasChanged)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        BoxNumber = addressBoxNumberWasChanged.BoxNumber
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBoxNumberWasCorrectedChangesBoxNumber(
            AddressWasRegistered addressWasRegistered,
            AddressBoxNumberWasCorrected addressBoxNumberWasCorrected)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBoxNumberWasCorrected)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        BoxNumber = addressBoxNumberWasCorrected.BoxNumber
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBoxNumberWasRemovedClearsBoxNumber(
            AddressWasRegistered addressWasRegistered,
            AddressBoxNumberWasChanged addressBoxNumberWasChanged,
            AddressBoxNumberWasRemoved addressBoxNumberWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBoxNumberWasChanged,
                        addressBoxNumberWasRemoved)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        BoxNumber = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPersistentLocalIdWasAssignedAfterRemoveIsSet(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressPersistentLocalIdWasAssigned addressPersistentLocalIdWasAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressPersistentLocalIdWasAssigned)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Removed = true,
                        PersistentLocalId = addressPersistentLocalIdWasAssigned.PersistentLocalId
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPostalCodeWasRemovedAfterRemoveIsSetToNull(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressPostalCodeWasRemoved addressPostalCodeWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressPostalCodeWasRemoved)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Removed = true,
                        PostalCode = null,
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBecameIncompleAfterRemoveIsSet(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressBecameIncomplete addressBecameIncomplete)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressBecameIncomplete)
                    .Expect(ctx => ctx.AddressList, new AddressListItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Complete = false,
                        Removed = true,
                    }));
        }

        protected override LegacyContext CreateContext(DbContextOptions<LegacyContext> options) => new LegacyContext(options);

        protected override AddressListProjections CreateProjection() => new AddressListProjections();
    }
}
