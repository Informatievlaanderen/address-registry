namespace AddressRegistry.Tests.ProjectionTests
{
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using System.Threading.Tasks;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using GeoAPI.Geometries;
    using NetTopologySuite.IO;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressDetailProjectionsTests : ProjectionTest<LegacyContext, AddressDetailProjections>
    {
        private readonly WKBReader _wkbReader = WKBReaderFactory.Create();

        public AddressDetailProjectionsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRegisteredCreatesAddressDetailItem(
            AddressWasRegistered addressWasRegistered)
        {
            await Assert(
                Given(addressWasRegistered)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Complete = true
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBecameCurrentSetsStatusToCurrent(
            AddressWasRegistered addressWasRegistered,
            AddressBecameCurrent addressBecameCurrent)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBecameCurrent)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = AddressStatus.Current
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Complete = false
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressBecameNotOfficallyAssignedSetsOfficallyAssignedFalse(
            AddressWasRegistered addressWasRegistered,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressBecameNotOfficiallyAssigned addressBecameNotOfficiallyAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasOfficiallyAssigned,
                        addressBecameNotOfficiallyAssigned)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OfficiallyAssigned = false
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressHouseNumberWasChanged.HouseNumber,
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressHouseNumberWasCorrected.HouseNumber
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressOfficialAssignmentWasRemovedSetsOfficalAssignmentToNull(
            AddressWasRegistered addressWasRegistered,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressOfficialAssignmentWasRemoved addressOfficialAssignmentWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasOfficiallyAssigned,
                        addressOfficialAssignmentWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OfficiallyAssigned = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressOsloIdWasAssignedSetsOsloId(
            AddressWasRegistered addressWasRegistered,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressOsloIdWasAssigned)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OsloId = addressOsloIdWasAssigned.OsloId
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPositionWasCorrectedSetsPosition(
            AddressId addressId,
            Provenance provenance,
            WkbGeometry geometry,
            AddressWasRegistered addressWasRegistered)
        {
            var addressPositionWasCorrected =
                new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(geometry)));

            ((ISetProvenance)addressPositionWasCorrected).SetProvenance(provenance);

            await Assert(
                    Given(addressWasRegistered, addressPositionWasCorrected)
                    .Expect(new AddressComparer<AddressDetailItem>(),
                        ctx => ctx.AddressDetail,
                        new AddressDetailItem
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = (IPoint)_wkbReader.Read(addressPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()),
                            PositionMethod = addressPositionWasCorrected.GeometryMethod,
                            PositionSpecification = addressPositionWasCorrected.GeometrySpecification
                        }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPositionWasRemovedSetsPositionToNull(
            AddressId addressId,
            WkbGeometry geometry,
            Provenance provenance,
            AddressWasRegistered addressWasRegistered,
            AddressPositionWasRemoved addressPositionWasRemoved)
        {
            var addressPositionWasCorrected =
                new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(geometry)));

            ((ISetProvenance)addressPositionWasCorrected).SetProvenance(provenance);

            await Assert(
                Given(addressWasRegistered,
                        addressPositionWasCorrected, addressPositionWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Position = null,
                        PositionSpecification = null,
                        PositionMethod = null,
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        PostalCode = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressStatusWasCorrectedToRemovedSetsStatusToNull(
            AddressWasRegistered addressWasRegistered,
            AddressBecameCurrent addressBecameCurrent,
            AddressStatusWasCorrectedToRemoved addressStatusWasCorrectedToRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBecameCurrent,
                        addressStatusWasCorrectedToRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressStatusWasRemovedSetsStatusToNull(
            AddressWasRegistered addressWasRegistered,
            AddressBecameCurrent addressBecameCurrent,
            AddressStatusWasRemoved addressStatusWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressBecameCurrent,
                        addressStatusWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasCorrectedToCurrentSetsStatusToCurrent(
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToCurrent addressWasCorrectedToCurrent)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasCorrectedToCurrent)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = AddressStatus.Current
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasCorrectedToNotOfficiallyAssignedSetsOfficallyAssignedFalse(
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToNotOfficiallyAssigned addressWasCorrectedToNotOfficiallyAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasCorrectedToNotOfficiallyAssigned)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OfficiallyAssigned = false
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasCorrectedToOfficiallyAssignedSetsOfficallyAssignedTrue(
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToOfficiallyAssigned addressWasCorrectedToOfficiallyAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasCorrectedToOfficiallyAssigned)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OfficiallyAssigned = true
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasCorrectedToProposedSetsStatusToProposed(
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToProposed addressWasCorrectedToProposed)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasCorrectedToProposed)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = AddressStatus.Proposed
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasCorrectedToRetiredSetsStatusToRetired(
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToRetired addressWasCorrectedToRetired)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasCorrectedToRetired)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = AddressStatus.Retired
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasOfficiallyAssignedSetsOfficallyAssignedTrue(
            AddressWasRegistered addressWasRegistered,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasOfficiallyAssigned)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OfficiallyAssigned = true
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasPositionedSetsPosition(
            AddressId addressId,
            WkbGeometry geometry,
            Provenance provenance,
            AddressWasRegistered addressWasRegistered)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.Interpolated, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(geometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(provenance);

            await Assert(
                    Given(addressWasRegistered, addressWasPositioned)
                    .Expect(
                        new AddressComparer<AddressDetailItem>(),
                        ctx => ctx.AddressDetail,
                        new AddressDetailItem
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = (IPoint)_wkbReader.Read(addressWasPositioned.ExtendedWkbGeometry.ToByteArray()),
                            PositionMethod = addressWasPositioned.GeometryMethod,
                            PositionSpecification = addressWasPositioned.GeometrySpecification
                        }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasProposedSetsStatusToProposed(
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasProposed)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = AddressStatus.Proposed
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRemovedDeletesRecord(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Removed = true,
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRetiredSetsStatusToRetired(
            AddressWasRegistered addressWasRegistered,
            AddressWasRetired addressWasRetired)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRetired)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = AddressStatus.Retired
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        BoxNumber = null
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressPositionWasRemovedAfterRemoveIsSetToNull(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressPositionWasRemoved addressPositionWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressPositionWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Position = null,
                        PositionMethod = null,
                        PositionSpecification = null,
                        Removed = true,
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressOsloIdWasAssignedAfterRemoveIsSet(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressOsloIdWasAssigned)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OsloId = addressOsloIdWasAssigned.OsloId,
                        Removed = true,
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        PostalCode = null,
                        Removed = true,
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressStatusWasRemovedAfterRemoveIsSetToNull(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressStatusWasRemoved addressStatusWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressStatusWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Status = null,
                        Removed = true,
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressOfficialAssignmentWasRemovedAfterRemoveIsSetToNull(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressOfficialAssignmentWasRemoved addressOfficialAssignmentWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved,
                        addressOfficialAssignmentWasRemoved)
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        OfficiallyAssigned = null,
                        Removed = true,
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
                    .Expect(ctx => ctx.AddressDetail, new AddressDetailItem
                    {
                        AddressId = addressWasRegistered.AddressId,
                        StreetNameId = addressWasRegistered.StreetNameId,
                        HouseNumber = addressWasRegistered.HouseNumber,
                        Complete = false,
                        Removed = true,
                    }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasPositionedAfterRemoveIsSet(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            AddressWasPositioned addressWasPositioned,
            WkbGeometry geometry)
        {
            addressWasPositioned = addressWasPositioned.WithAddressGeometry(new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Building, GeometryHelpers.CreateEwkbFrom(geometry)));
            await Assert(
                    Given(addressWasRegistered, addressWasRemoved, addressWasPositioned)
                    .Expect(
                        new AddressComparer<AddressDetailItem>(),
                        ctx => ctx.AddressDetail, new AddressDetailItem
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = (IPoint)_wkbReader.Read(addressWasPositioned.ExtendedWkbGeometry.ToByteArray()),
                            PositionMethod = addressWasPositioned.GeometryMethod,
                            PositionSpecification = addressWasPositioned.GeometrySpecification,
                            Removed = true,
                        }));
        }

        protected override LegacyContext CreateContext(DbContextOptions<LegacyContext> options) => new LegacyContext(options);

        protected override AddressDetailProjections CreateProjection() => new AddressDetailProjections(_wkbReader);
    }
}
