namespace AddressRegistry.Tests.ProjectionTests
{
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressVersion;
    using System.Threading.Tasks;
    using Address;
    using GeoAPI.Geometries;
    using NetTopologySuite.IO;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressVersionsProjectionsTests : ProjectionTest<LegacyContext, AddressVersionProjections>
    {
        private readonly WKBReader _wkbReader = WKBReaderFactory.Create();

        public AddressVersionsProjectionsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRegisteredCreatesAddressVersionsItem(
            AddressWasRegistered addressWasRegistered)
        {
            await Assert(
                Given(addressWasRegistered)
                    .Expect(ctx => ctx.AddressVersions, new AddressVersion
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Complete = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressBecameComplete.Provenance.Timestamp,
                            Application = addressBecameComplete.Provenance.Application,
                            Organisation = addressBecameComplete.Provenance.Organisation,
                            Operator = addressBecameComplete.Provenance.Operator,
                            Modification = addressBecameComplete.Provenance.Modification,
                            Reason = addressBecameComplete.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Current,
                            StreamPosition = 1,
                            VersionTimestamp = addressBecameCurrent.Provenance.Timestamp,
                            Application = addressBecameCurrent.Provenance.Application,
                            Organisation = addressBecameCurrent.Provenance.Organisation,
                            Operator = addressBecameCurrent.Provenance.Operator,
                            Modification = addressBecameCurrent.Provenance.Modification,
                            Reason = addressBecameCurrent.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Complete = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressBecameComplete.Provenance.Timestamp,
                            Application = addressBecameComplete.Provenance.Application,
                            Organisation = addressBecameComplete.Provenance.Organisation,
                            Operator = addressBecameComplete.Provenance.Operator,
                            Modification = addressBecameComplete.Provenance.Modification,
                            Reason = addressBecameComplete.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Complete = false,
                            StreamPosition = 2,
                            VersionTimestamp = addressBecameIncomplete.Provenance.Timestamp,
                            Application = addressBecameIncomplete.Provenance.Application,
                            Organisation = addressBecameIncomplete.Provenance.Organisation,
                            Operator = addressBecameIncomplete.Provenance.Operator,
                            Modification = addressBecameIncomplete.Provenance.Modification,
                            Reason = addressBecameIncomplete.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasOfficiallyAssigned.Provenance.Timestamp,
                            Application = addressWasOfficiallyAssigned.Provenance.Application,
                            Organisation = addressWasOfficiallyAssigned.Provenance.Organisation,
                            Operator = addressWasOfficiallyAssigned.Provenance.Operator,
                            Modification = addressWasOfficiallyAssigned.Provenance.Modification,
                            Reason = addressWasOfficiallyAssigned.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = false,
                            StreamPosition = 2,
                            VersionTimestamp = addressBecameNotOfficiallyAssigned.Provenance.Timestamp,
                            Application = addressBecameNotOfficiallyAssigned.Provenance.Application,
                            Organisation = addressBecameNotOfficiallyAssigned.Provenance.Organisation,
                            Operator = addressBecameNotOfficiallyAssigned.Provenance.Operator,
                            Modification = addressBecameNotOfficiallyAssigned.Provenance.Modification,
                            Reason = addressBecameNotOfficiallyAssigned.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressHouseNumberWasChanged.HouseNumber,
                            StreamPosition = 1,
                            VersionTimestamp = addressHouseNumberWasChanged.Provenance.Timestamp,
                            Application = addressHouseNumberWasChanged.Provenance.Application,
                            Organisation = addressHouseNumberWasChanged.Provenance.Organisation,
                            Operator = addressHouseNumberWasChanged.Provenance.Operator,
                            Modification = addressHouseNumberWasChanged.Provenance.Modification,
                            Reason = addressHouseNumberWasChanged.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressHouseNumberWasCorrected.HouseNumber,
                            StreamPosition = 1,
                            VersionTimestamp = addressHouseNumberWasCorrected.Provenance.Timestamp,
                            Application = addressHouseNumberWasCorrected.Provenance.Application,
                            Organisation = addressHouseNumberWasCorrected.Provenance.Organisation,
                            Operator = addressHouseNumberWasCorrected.Provenance.Operator,
                            Modification = addressHouseNumberWasCorrected.Provenance.Modification,
                            Reason = addressHouseNumberWasCorrected.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasOfficiallyAssigned.Provenance.Timestamp,
                            Application = addressWasOfficiallyAssigned.Provenance.Application,
                            Organisation = addressWasOfficiallyAssigned.Provenance.Organisation,
                            Operator = addressWasOfficiallyAssigned.Provenance.Operator,
                            Modification = addressWasOfficiallyAssigned.Provenance.Modification,
                            Reason = addressWasOfficiallyAssigned.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = null,
                            StreamPosition = 2,
                            VersionTimestamp = addressOfficialAssignmentWasRemoved.Provenance.Timestamp,
                            Application = addressOfficialAssignmentWasRemoved.Provenance.Application,
                            Organisation = addressOfficialAssignmentWasRemoved.Provenance.Organisation,
                            Operator = addressOfficialAssignmentWasRemoved.Provenance.Operator,
                            Modification = addressOfficialAssignmentWasRemoved.Provenance.Modification,
                            Reason = addressOfficialAssignmentWasRemoved.Provenance.Reason,
                        }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressOsloIdWasAssignedSetsOsloIdForAllVersionsRecords(
            AddressWasRegistered addressWasRegistered,
            AddressOsloIdWasAssigned addressOsloIdWasAssigned)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressOsloIdWasAssigned)
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
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
            WkbGeometry geometry,
            Provenance provenance,
            AddressWasRegistered addressWasRegistered)
        {
            var addressPositionWasCorrected =
                new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(geometry)));
            ((ISetProvenance)addressPositionWasCorrected).SetProvenance(provenance);

            await Assert(
                Given(addressWasRegistered,
                        addressPositionWasCorrected)
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            StreamPosition = 1,
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = (IPoint) _wkbReader.Read(addressPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()),
                            PositionMethod = addressPositionWasCorrected.GeometryMethod,
                            PositionSpecification = addressPositionWasCorrected.GeometrySpecification,
                            VersionTimestamp = addressPositionWasCorrected.Provenance.Timestamp,
                            Application = addressPositionWasCorrected.Provenance.Application,
                            Organisation = addressPositionWasCorrected.Provenance.Organisation,
                            Operator = addressPositionWasCorrected.Provenance.Operator,
                            Modification = addressPositionWasCorrected.Provenance.Modification,
                            Reason = addressPositionWasCorrected.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = (IPoint)_wkbReader.Read(addressPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()),
                            PositionMethod = addressPositionWasCorrected.GeometryMethod,
                            PositionSpecification = addressPositionWasCorrected.GeometrySpecification,
                            StreamPosition = 1,
                            VersionTimestamp = addressPositionWasCorrected.Provenance.Timestamp,
                            Application = addressPositionWasCorrected.Provenance.Application,
                            Organisation = addressPositionWasCorrected.Provenance.Organisation,
                            Operator = addressPositionWasCorrected.Provenance.Operator,
                            Modification = addressPositionWasCorrected.Provenance.Modification,
                            Reason = addressPositionWasCorrected.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = null,
                            StreamPosition = 2,
                            VersionTimestamp = addressPositionWasRemoved.Provenance.Timestamp,
                            Application = addressPositionWasRemoved.Provenance.Application,
                            Organisation = addressPositionWasRemoved.Provenance.Organisation,
                            Operator = addressPositionWasRemoved.Provenance.Operator,
                            Modification = addressPositionWasRemoved.Provenance.Modification,
                            Reason = addressPositionWasRemoved.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            PostalCode = addressPostalCodeWasChanged.PostalCode,
                            StreamPosition = 1,
                            VersionTimestamp = addressPostalCodeWasChanged.Provenance.Timestamp,
                            Application = addressPostalCodeWasChanged.Provenance.Application,
                            Organisation = addressPostalCodeWasChanged.Provenance.Organisation,
                            Operator = addressPostalCodeWasChanged.Provenance.Operator,
                            Modification = addressPostalCodeWasChanged.Provenance.Modification,
                            Reason = addressPostalCodeWasChanged.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            PostalCode = addressPostalCodeWasCorrected.PostalCode,
                            StreamPosition = 1,
                            VersionTimestamp = addressPostalCodeWasCorrected.Provenance.Timestamp,
                            Application = addressPostalCodeWasCorrected.Provenance.Application,
                            Organisation = addressPostalCodeWasCorrected.Provenance.Organisation,
                            Operator = addressPostalCodeWasCorrected.Provenance.Operator,
                            Modification = addressPostalCodeWasCorrected.Provenance.Modification,
                            Reason = addressPostalCodeWasCorrected.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            PostalCode = addressPostalCodeWasChanged.PostalCode,
                            StreamPosition = 1,
                            VersionTimestamp = addressPostalCodeWasChanged.Provenance.Timestamp,
                            Application = addressPostalCodeWasChanged.Provenance.Application,
                            Organisation = addressPostalCodeWasChanged.Provenance.Organisation,
                            Operator = addressPostalCodeWasChanged.Provenance.Operator,
                            Modification = addressPostalCodeWasChanged.Provenance.Modification,
                            Reason = addressPostalCodeWasChanged.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            PostalCode = null,
                            StreamPosition = 2,
                            VersionTimestamp = addressPostalCodeWasRemoved.Provenance.Timestamp,
                            Application = addressPostalCodeWasRemoved.Provenance.Application,
                            Organisation = addressPostalCodeWasRemoved.Provenance.Organisation,
                            Operator = addressPostalCodeWasRemoved.Provenance.Operator,
                            Modification = addressPostalCodeWasRemoved.Provenance.Modification,
                            Reason = addressPostalCodeWasRemoved.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Current,
                            StreamPosition = 1,
                            VersionTimestamp = addressBecameCurrent.Provenance.Timestamp,
                            Application = addressBecameCurrent.Provenance.Application,
                            Organisation = addressBecameCurrent.Provenance.Organisation,
                            Operator = addressBecameCurrent.Provenance.Operator,
                            Modification = addressBecameCurrent.Provenance.Modification,
                            Reason = addressBecameCurrent.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = null,
                            StreamPosition = 2,
                            VersionTimestamp = addressStatusWasCorrectedToRemoved.Provenance.Timestamp,
                            Application = addressStatusWasCorrectedToRemoved.Provenance.Application,
                            Organisation = addressStatusWasCorrectedToRemoved.Provenance.Organisation,
                            Operator = addressStatusWasCorrectedToRemoved.Provenance.Operator,
                            Modification = addressStatusWasCorrectedToRemoved.Provenance.Modification,
                            Reason = addressStatusWasCorrectedToRemoved.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Current,
                            StreamPosition = 1,
                            VersionTimestamp = addressBecameCurrent.Provenance.Timestamp,
                            Application = addressBecameCurrent.Provenance.Application,
                            Organisation = addressBecameCurrent.Provenance.Organisation,
                            Operator = addressBecameCurrent.Provenance.Operator,
                            Modification = addressBecameCurrent.Provenance.Modification,
                            Reason = addressBecameCurrent.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = null,
                            StreamPosition = 2,
                            VersionTimestamp = addressStatusWasRemoved.Provenance.Timestamp,
                            Application = addressStatusWasRemoved.Provenance.Application,
                            Organisation = addressStatusWasRemoved.Provenance.Organisation,
                            Operator = addressStatusWasRemoved.Provenance.Operator,
                            Modification = addressStatusWasRemoved.Provenance.Modification,
                            Reason = addressStatusWasRemoved.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Current,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasCorrectedToCurrent.Provenance.Timestamp,
                            Application = addressWasCorrectedToCurrent.Provenance.Application,
                            Organisation = addressWasCorrectedToCurrent.Provenance.Organisation,
                            Operator = addressWasCorrectedToCurrent.Provenance.Operator,
                            Modification = addressWasCorrectedToCurrent.Provenance.Modification,
                            Reason = addressWasCorrectedToCurrent.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = false,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasCorrectedToNotOfficiallyAssigned.Provenance.Timestamp,
                            Application = addressWasCorrectedToNotOfficiallyAssigned.Provenance.Application,
                            Organisation = addressWasCorrectedToNotOfficiallyAssigned.Provenance.Organisation,
                            Operator = addressWasCorrectedToNotOfficiallyAssigned.Provenance.Operator,
                            Modification = addressWasCorrectedToNotOfficiallyAssigned.Provenance.Modification,
                            Reason = addressWasCorrectedToNotOfficiallyAssigned.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasCorrectedToOfficiallyAssigned.Provenance.Timestamp,
                            Application = addressWasCorrectedToOfficiallyAssigned.Provenance.Application,
                            Organisation = addressWasCorrectedToOfficiallyAssigned.Provenance.Organisation,
                            Operator = addressWasCorrectedToOfficiallyAssigned.Provenance.Operator,
                            Modification = addressWasCorrectedToOfficiallyAssigned.Provenance.Modification,
                            Reason = addressWasCorrectedToOfficiallyAssigned.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Proposed,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasCorrectedToProposed.Provenance.Timestamp,
                            Application = addressWasCorrectedToProposed.Provenance.Application,
                            Organisation = addressWasCorrectedToProposed.Provenance.Organisation,
                            Operator = addressWasCorrectedToProposed.Provenance.Operator,
                            Modification = addressWasCorrectedToProposed.Provenance.Modification,
                            Reason = addressWasCorrectedToProposed.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Retired,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasCorrectedToRetired.Provenance.Timestamp,
                            Application = addressWasCorrectedToRetired.Provenance.Application,
                            Organisation = addressWasCorrectedToRetired.Provenance.Organisation,
                            Operator = addressWasCorrectedToRetired.Provenance.Operator,
                            Modification = addressWasCorrectedToRetired.Provenance.Modification,
                            Reason = addressWasCorrectedToRetired.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            OfficiallyAssigned = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasOfficiallyAssigned.Provenance.Timestamp,
                            Application = addressWasOfficiallyAssigned.Provenance.Application,
                            Organisation = addressWasOfficiallyAssigned.Provenance.Organisation,
                            Operator = addressWasOfficiallyAssigned.Provenance.Operator,
                            Modification = addressWasOfficiallyAssigned.Provenance.Modification,
                            Reason = addressWasOfficiallyAssigned.Provenance.Reason,
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
                Given(addressWasRegistered,
                        addressWasPositioned)
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Position = (IPoint)_wkbReader.Read(addressWasPositioned.ExtendedWkbGeometry.ToByteArray()),
                            PositionMethod = addressWasPositioned.GeometryMethod,
                            PositionSpecification = addressWasPositioned.GeometrySpecification,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasPositioned.Provenance.Timestamp,
                            Application = addressWasPositioned.Provenance.Application,
                            Organisation = addressWasPositioned.Provenance.Organisation,
                            Operator = addressWasPositioned.Provenance.Operator,
                            Modification = addressWasPositioned.Provenance.Modification,
                            Reason = addressWasPositioned.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Proposed,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasProposed.Provenance.Timestamp,
                            Application = addressWasProposed.Provenance.Application,
                            Organisation = addressWasProposed.Provenance.Organisation,
                            Operator = addressWasProposed.Provenance.Operator,
                            Modification = addressWasProposed.Provenance.Modification,
                            Reason = addressWasProposed.Provenance.Reason,
                        }));
        }

        [Theory]
        [DefaultData]
        public async Task AddressWasRemovedSetRemovedToTrue(
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved)
        {
            await Assert(
                Given(addressWasRegistered,
                        addressWasRemoved)
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Removed = true,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasRemoved.Provenance.Timestamp,
                            Application = addressWasRemoved.Provenance.Application,
                            Organisation = addressWasRemoved.Provenance.Organisation,
                            Operator = addressWasRemoved.Provenance.Operator,
                            Modification = addressWasRemoved.Provenance.Modification,
                            Reason = addressWasRemoved.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            Status = AddressStatus.Retired,
                            StreamPosition = 1,
                            VersionTimestamp = addressWasRetired.Provenance.Timestamp,
                            Application = addressWasRetired.Provenance.Application,
                            Organisation = addressWasRetired.Provenance.Organisation,
                            Operator = addressWasRetired.Provenance.Operator,
                            Modification = addressWasRetired.Provenance.Modification,
                            Reason = addressWasRetired.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            BoxNumber = addressBoxNumberWasChanged.BoxNumber,
                            StreamPosition = 1,
                            VersionTimestamp = addressBoxNumberWasChanged.Provenance.Timestamp,
                            Application = addressBoxNumberWasChanged.Provenance.Application,
                            Organisation = addressBoxNumberWasChanged.Provenance.Organisation,
                            Operator = addressBoxNumberWasChanged.Provenance.Operator,
                            Modification = addressBoxNumberWasChanged.Provenance.Modification,
                            Reason = addressBoxNumberWasChanged.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            BoxNumber = addressBoxNumberWasCorrected.BoxNumber,
                            StreamPosition = 1,
                            VersionTimestamp = addressBoxNumberWasCorrected.Provenance.Timestamp,
                            Application = addressBoxNumberWasCorrected.Provenance.Application,
                            Organisation = addressBoxNumberWasCorrected.Provenance.Organisation,
                            Operator = addressBoxNumberWasCorrected.Provenance.Operator,
                            Modification = addressBoxNumberWasCorrected.Provenance.Modification,
                            Reason = addressBoxNumberWasCorrected.Provenance.Reason,
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
                    .Expect(ctx => ctx.AddressVersions,
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            BoxNumber = addressBoxNumberWasChanged.BoxNumber,
                            StreamPosition = 1,
                            VersionTimestamp = addressBoxNumberWasChanged.Provenance.Timestamp,
                            Application = addressBoxNumberWasChanged.Provenance.Application,
                            Organisation = addressBoxNumberWasChanged.Provenance.Organisation,
                            Operator = addressBoxNumberWasChanged.Provenance.Operator,
                            Modification = addressBoxNumberWasChanged.Provenance.Modification,
                            Reason = addressBoxNumberWasChanged.Provenance.Reason,
                        },
                        new AddressVersion
                        {
                            AddressId = addressWasRegistered.AddressId,
                            StreetNameId = addressWasRegistered.StreetNameId,
                            HouseNumber = addressWasRegistered.HouseNumber,
                            BoxNumber = null,
                            StreamPosition = 2,
                            VersionTimestamp = addressBoxNumberWasRemoved.Provenance.Timestamp,
                            Application = addressBoxNumberWasRemoved.Provenance.Application,
                            Organisation = addressBoxNumberWasRemoved.Provenance.Organisation,
                            Operator = addressBoxNumberWasRemoved.Provenance.Operator,
                            Modification = addressBoxNumberWasRemoved.Provenance.Modification,
                            Reason = addressBoxNumberWasRemoved.Provenance.Reason,
                        }));
        }

        protected override LegacyContext CreateContext(DbContextOptions<LegacyContext> options) => new LegacyContext(options);

        protected override AddressVersionProjections CreateProjection() => new AddressVersionProjections(_wkbReader);
    }
}
