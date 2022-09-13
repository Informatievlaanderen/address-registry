namespace AddressRegistry.Producer.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using AddressAggregate = Address.Events;
    using StreetNameAggregate = StreetName.Events;

    public static class MessageExtensions
    {
        public static Contracts.AddressBecameComplete ToContract(this AddressAggregate.AddressBecameComplete message) =>
            new Contracts.AddressBecameComplete(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressBecameCurrent ToContract(this AddressAggregate.AddressBecameCurrent message) =>
            new Contracts.AddressBecameCurrent(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressBecameIncomplete ToContract(this AddressAggregate.AddressBecameIncomplete message) =>
            new Contracts.AddressBecameIncomplete(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressBecameNotOfficiallyAssigned ToContract(this AddressAggregate.AddressBecameNotOfficiallyAssigned message) =>
            new Contracts.AddressBecameNotOfficiallyAssigned(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressBoxNumberWasChanged ToContract(this AddressAggregate.AddressBoxNumberWasChanged message) =>
            new Contracts.AddressBoxNumberWasChanged(message.AddressId.ToString("D"), message.BoxNumber, message.Provenance.ToContract());

        public static Contracts.AddressBoxNumberWasCorrected ToContract(this AddressAggregate.AddressBoxNumberWasCorrected message) =>
            new Contracts.AddressBoxNumberWasCorrected(message.AddressId.ToString("D"), message.BoxNumber, message.Provenance.ToContract());

        public static Contracts.AddressBoxNumberWasRemoved ToContract(this AddressAggregate.AddressBoxNumberWasRemoved message) =>
            new Contracts.AddressBoxNumberWasRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressHouseNumberWasChanged ToContract(this AddressAggregate.AddressHouseNumberWasChanged message) =>
            new Contracts.AddressHouseNumberWasChanged(message.AddressId.ToString("D"), message.HouseNumber, message.Provenance.ToContract());

        public static Contracts.AddressHouseNumberWasCorrected ToContract(this AddressAggregate.AddressHouseNumberWasCorrected message) =>
            new Contracts.AddressHouseNumberWasCorrected(message.AddressId.ToString("D"), message.HouseNumber, message.Provenance.ToContract());

        public static Contracts.AddressOfficialAssignmentWasRemoved ToContract(this AddressAggregate.AddressOfficialAssignmentWasRemoved message) =>
            new Contracts.AddressOfficialAssignmentWasRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressPersistentLocalIdWasAssigned ToContract(this AddressAggregate.AddressPersistentLocalIdWasAssigned message) =>
            new Contracts.AddressPersistentLocalIdWasAssigned(message.AddressId.ToString("D"), message.PersistentLocalId, message.AssignmentDate.ToString(), message.Provenance.ToContract());

        public static Contracts.AddressPositionWasCorrected ToContract(this AddressAggregate.AddressPositionWasCorrected message) =>
            new Contracts.AddressPositionWasCorrected(message.AddressId.ToString("D"), message.GeometryMethod.ToString(), message.GeometrySpecification.ToString(), message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.AddressPositionWasRemoved ToContract(this AddressAggregate.AddressPositionWasRemoved message) =>
            new Contracts.AddressPositionWasRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressPostalCodeWasChanged ToContract(this AddressAggregate.AddressPostalCodeWasChanged message) =>
            new Contracts.AddressPostalCodeWasChanged(message.AddressId.ToString("D"), message.PostalCode ,message.Provenance.ToContract());

        public static Contracts.AddressPostalCodeWasCorrected ToContract(this AddressAggregate.AddressPostalCodeWasCorrected message) =>
            new Contracts.AddressPostalCodeWasCorrected(message.AddressId.ToString("D"), message.PostalCode ,message.Provenance.ToContract());

        public static Contracts.AddressPostalCodeWasRemoved ToContract(this AddressAggregate.AddressPostalCodeWasRemoved message) =>
            new Contracts.AddressPostalCodeWasRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressStatusWasCorrectedToRemoved ToContract(this AddressAggregate.AddressStatusWasCorrectedToRemoved message) =>
            new Contracts.AddressStatusWasCorrectedToRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressStatusWasRemoved ToContract(this AddressAggregate.AddressStatusWasRemoved message) =>
            new Contracts.AddressStatusWasRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressStreetNameWasChanged ToContract(this AddressAggregate.AddressStreetNameWasChanged message) =>
            new Contracts.AddressStreetNameWasChanged(message.AddressId.ToString("D"), message.StreetNameId.ToString("D") ,message.Provenance.ToContract());

        public static Contracts.AddressStreetNameWasCorrected ToContract(this AddressAggregate.AddressStreetNameWasCorrected message) =>
            new Contracts.AddressStreetNameWasCorrected(message.AddressId.ToString("D"), message.StreetNameId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasCorrectedToCurrent ToContract(this AddressAggregate.AddressWasCorrectedToCurrent message) =>
            new Contracts.AddressWasCorrectedToCurrent(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasCorrectedToNotOfficiallyAssigned ToContract(this AddressAggregate.AddressWasCorrectedToNotOfficiallyAssigned message) =>
            new Contracts.AddressWasCorrectedToNotOfficiallyAssigned(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasCorrectedToOfficiallyAssigned ToContract(this AddressAggregate.AddressWasCorrectedToOfficiallyAssigned message) =>
            new Contracts.AddressWasCorrectedToOfficiallyAssigned(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasCorrectedToProposed ToContract(this AddressAggregate.AddressWasCorrectedToProposed message) =>
            new Contracts.AddressWasCorrectedToProposed(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasCorrectedToRetired ToContract(this AddressAggregate.AddressWasCorrectedToRetired message) =>
            new Contracts.AddressWasCorrectedToRetired(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasMigrated ToContract(this AddressAggregate.AddressWasMigrated message) =>
            new Contracts.AddressWasMigrated(message.AddressId.ToString("D"), message.StreetNamePersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasOfficiallyAssigned ToContract(this AddressAggregate.AddressWasOfficiallyAssigned message) =>
            new Contracts.AddressWasOfficiallyAssigned(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasPositioned ToContract(this AddressAggregate.AddressWasPositioned message) =>
            new Contracts.AddressWasPositioned(message.AddressId.ToString("D"), message.GeometryMethod.ToString(), message.GeometrySpecification.ToString(), message.ExtendedWkbGeometry, message.Provenance.ToContract());

        public static Contracts.AddressWasProposed ToContract(this AddressAggregate.AddressWasProposed message) =>
            new Contracts.AddressWasProposed(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasRegistered ToContract(this AddressAggregate.AddressWasRegistered message) =>
            new Contracts.AddressWasRegistered(message.AddressId.ToString("D"), message.StreetNameId.ToString("D"), message.HouseNumber, message.Provenance.ToContract());

        public static Contracts.AddressWasRemoved ToContract(this AddressAggregate.AddressWasRemoved message) =>
            new Contracts.AddressWasRemoved(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasRetired ToContract(this AddressAggregate.AddressWasRetired message) =>
            new Contracts.AddressWasRetired(message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.AddressWasApproved ToContract(this StreetNameAggregate.AddressWasApproved message) =>
            new Contracts.AddressWasApproved(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRemovedV2 ToContract(this StreetNameAggregate.AddressWasRemovedV2 message) =>
            new Contracts.AddressWasRemovedV2(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRemovedBecauseHouseNumberWasRemoved ToContract(this StreetNameAggregate.AddressWasRemovedBecauseHouseNumberWasRemoved message) =>
            new Contracts.AddressWasRemovedBecauseHouseNumberWasRemoved(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRejected ToContract(this StreetNameAggregate.AddressWasRejected message) =>
            new Contracts.AddressWasRejected(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRejectedBecauseHouseNumberWasRejected ToContract(this StreetNameAggregate.AddressWasRejectedBecauseHouseNumberWasRejected message) =>
            new Contracts.AddressWasRejectedBecauseHouseNumberWasRejected(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRejectedBecauseHouseNumberWasRetired ToContract(this StreetNameAggregate.AddressWasRejectedBecauseHouseNumberWasRetired message) =>
            new Contracts.AddressWasRejectedBecauseHouseNumberWasRetired(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRejectedBecauseStreetNameWasRetired ToContract(this StreetNameAggregate.AddressWasRejectedBecauseStreetNameWasRetired message) =>
            new Contracts.AddressWasRejectedBecauseStreetNameWasRetired(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasDeregulated ToContract(this StreetNameAggregate.AddressWasDeregulated message) =>
            new Contracts.AddressWasDeregulated(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRegularized ToContract(this StreetNameAggregate.AddressWasRegularized message) =>
            new Contracts.AddressWasRegularized(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRetiredV2 ToContract(this StreetNameAggregate.AddressWasRetiredV2 message) =>
            new Contracts.AddressWasRetiredV2(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRetiredBecauseHouseNumberWasRetired ToContract(this StreetNameAggregate.AddressWasRetiredBecauseHouseNumberWasRetired message) =>
            new Contracts.AddressWasRetiredBecauseHouseNumberWasRetired(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressWasRetiredBecauseStreetNameWasRetired ToContract(this StreetNameAggregate.AddressWasRetiredBecauseStreetNameWasRetired message) =>
            new Contracts.AddressWasRetiredBecauseStreetNameWasRetired(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.AddressPostalCodeWasChangedV2 ToContract(this StreetNameAggregate.AddressPostalCodeWasChangedV2 message) =>
            new Contracts.AddressPostalCodeWasChangedV2(message.StreetNamePersistentLocalId, message.AddressPersistentLocalId, message.PostalCode, message.Provenance.ToContract());

        public static Contracts.AddressPositionWasChanged ToContract(this StreetNameAggregate.AddressPositionWasChanged message) =>
            new Contracts.AddressPositionWasChanged(
                message.StreetNamePersistentLocalId,
                message.AddressPersistentLocalId,
                message.GeometryMethod.ToString(),
                message.GeometrySpecification.ToString(),
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.AddressPositionWasCorrectedV2 ToContract(this StreetNameAggregate.AddressPositionWasCorrectedV2 message) =>
            new Contracts.AddressPositionWasCorrectedV2(
                message.StreetNamePersistentLocalId,
                message.AddressPersistentLocalId,
                message.GeometryMethod.ToString(),
                message.GeometrySpecification.ToString(),
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.AddressWasMigratedToStreetName ToContract(
            this StreetNameAggregate.AddressWasMigratedToStreetName message) =>
            new Contracts.AddressWasMigratedToStreetName(
                message.StreetNamePersistentLocalId,
                message.AddressId.ToString("D"),
                message.StreetNameId.ToString("D"),
                message.AddressPersistentLocalId,
                message.Status.ToString(),
                message.HouseNumber,
                message.BoxNumber,
                message.GeometryMethod.ToString(),
                message.GeometrySpecification.ToString(),
                message.ExtendedWkbGeometry,
                message.OfficiallyAssigned,
                message.PostalCode,
                message.IsCompleted,
                message.IsRemoved,
                message.ParentPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.AddressWasProposedV2 ToContract(this StreetNameAggregate.AddressWasProposedV2 message) =>
            new Contracts.AddressWasProposedV2(
                message.StreetNamePersistentLocalId,
                message.AddressPersistentLocalId,
                message.ParentPersistentLocalId,
                message.PostalCode,
                message.HouseNumber,
                message.BoxNumber,
                message.GeometryMethod.ToString(),
                message.GeometrySpecification.ToString(),
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.MigratedStreetNameWasImported ToContract(this StreetNameAggregate.MigratedStreetNameWasImported message) =>
            new Contracts.MigratedStreetNameWasImported(
                message.StreetNameId.ToString("D"),
                message.StreetNamePersistentLocalId,
                message.MunicipalityId.ToString("D"),
                message.NisCode,
                message.StreetNameStatus.ToString(),
                message.Provenance.ToContract());

        public static Contracts.StreetNameWasApproved ToContract(this StreetNameAggregate.StreetNameWasApproved message) =>
            new Contracts.StreetNameWasApproved(message.StreetNamePersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasRetired ToContract(this StreetNameAggregate.StreetNameWasRetired message) =>
            new Contracts.StreetNameWasRetired(message.StreetNamePersistentLocalId, message.Provenance.ToContract());

        public static Contracts.StreetNameWasImported ToContract(this StreetNameAggregate.StreetNameWasImported message) =>
            new Contracts.StreetNameWasImported(message.StreetNamePersistentLocalId, message.MunicipalityId.ToString("D"), message.StreetNameStatus.ToString(), message.Provenance.ToContract());

        public static Contracts.StreetNameWasRemoved ToContract(this StreetNameAggregate.StreetNameWasRemoved message) =>
            new Contracts.StreetNameWasRemoved(message.StreetNamePersistentLocalId, message.Provenance.ToContract());

        private static Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance ToContract(this ProvenanceData provenance)
            => new Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance(
                provenance.Timestamp.ToString(),
                provenance.Application.ToString(),
                provenance.Modification.ToString(),
                provenance.Organisation.ToString(),
                provenance.Reason);
    }
}
