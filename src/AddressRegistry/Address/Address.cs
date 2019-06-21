namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;
    using Events;
    using Events.Crab;
    using NetTopologySuite.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NodaTime;

    public partial class Address : AggregateRootEntity
    {
        public static readonly Func<Address> Factory = () => new Address();

        public bool IsRetired => _status == AddressStatus.Retired;

        public static Address Register(
            AddressId id,
            StreetNameId streetNameId,
            HouseNumber houseNumber)
        {
            var address = Factory();
            address.ApplyChange(new AddressWasRegistered(id, streetNameId, houseNumber));
            return address;
        }

        public void ImportHouseNumberFromCrab(
            CrabHouseNumberId houseNumberId,
            CrabStreetNameId crabStreetNameId,
            HouseNumber houseNumber,
            GrbNotation grbNotation,
            CrabLifetime crabLifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            if (IsRemoved && !IsSubaddress)
                throw new AddressRemovedException($"Cannot change removed address for address id {_addressId}");

            if (!(_coupledHouseNumberId != null && _coupledHouseNumberId != houseNumberId) && !IsRemoved)
            {
                if (modification == CrabModification.Delete)
                {
                    ApplyChange(new AddressWasRemoved(_addressId));
                }
                else
                {
                    ApplyHouseNumberChange(crabStreetNameId, houseNumber, modification);
                    EvaluateRetirement(crabLifetime, modification);
                }
            }

            ApplyChange(new AddressHouseNumberWasImportedFromCrab(
                houseNumberId,
                crabStreetNameId,
                houseNumber,
                grbNotation,
                crabLifetime,
                timestamp,
                @operator,
                modification,
                organisation));
        }

        public void ImportHouseNumberStatusFromCrab(
            CrabHouseNumberStatusId houseNumberStatusId,
            CrabHouseNumberId houseNumberId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            var legacyEvent = new AddressHouseNumberStatusWasImportedFromCrab(
                houseNumberStatusId,
                houseNumberId,
                addressStatus,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            ApplyStatusChangesFor(_houseNumberStatusChronicle, legacyEvent);

            ApplyAddressCompletion();

            ApplyChange(legacyEvent);
        }

        public void ImportHouseNumberPositionFromCrab(
            CrabAddressPositionId addressPositionId,
            CrabHouseNumberId houseNumberId,
            WkbGeometry addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabAddressNature addressNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            var legacyEvent = new AddressHouseNumberPositionWasImportedFromCrab(
                addressPositionId,
                houseNumberId,
                addressPosition,
                addressPositionOrigin,
                addressNature,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            ApplyPositionChangeFor<AddressHouseNumberPositionWasImportedFromCrab, int>(_crabHouseNumberPositionEvents, legacyEvent);

            ApplyAddressCompletion();

            ApplyChange(legacyEvent);
        }

        public void ImportHouseNumberMailCantonFromCrab(
            CrabHouseNumberMailCantonId houseNumberMailCantonId,
            CrabHouseNumberId houseNumberId,
            CrabMailCantonId mailCantonId,
            CrabMailCantonCode mailCantonCode,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            if (!IsSubaddress)
                GuardRemoved(modification);

            var legacyEvent = new AddressHouseNumberMailCantonWasImportedFromCrab(
                houseNumberMailCantonId,
                houseNumberId,
                mailCantonId,
                mailCantonCode,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            if (!(IsSubaddress && IsRemoved) && !(_coupledHouseNumberId != null && _coupledHouseNumberId != houseNumberId))
                ApplyPostalCodeChangeFor(legacyEvent);

            ApplyChange(legacyEvent);
        }

        public void ImportSubaddressFromCrab(
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            if (modification == CrabModification.Delete)
            {
                ApplyChange(new AddressWasRemoved(_addressId));
            }
            else
            {
                if (_boxNumber != boxNumber)
                {
                    if (string.IsNullOrEmpty(boxNumber) && _boxNumber != null)
                        ApplyChange(new AddressBoxNumberWasRemoved(_addressId));
                    else if (modification != CrabModification.Correction)
                        ApplyChange(new AddressBoxNumberWasChanged(_addressId, boxNumber));
                    else
                        ApplyChange(new AddressBoxNumberWasCorrected(_addressId, boxNumber));
                }

                EvaluateRetirement(lifetime, modification);
            }

            if (_coupledHouseNumberId != null && _coupledHouseNumberId != houseNumberId)
            {
                var lastEvent = _lastHouseNumberEventsForSubaddress[houseNumberId];

                ApplyHouseNumberChange(new CrabStreetNameId(lastEvent.StreetNameId), new HouseNumber(lastEvent.HouseNumber), modification);

                ApplyPostalCodeChangeFor(
                    _lastHouseNumberMailCantonEventsForSubaddress.ContainsKey(houseNumberId)
                        ? _lastHouseNumberMailCantonEventsForSubaddress[houseNumberId]
                        : null,
                    modification);
            }

            ApplyChange(new AddressSubaddressWasImportedFromCrab(
                subaddressId,
                houseNumberId,
                boxNumber,
                boxNumberType,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation));
        }

        public void ImportSubaddressStatusFromCrab(
            CrabSubaddressStatusId subaddressStatusId,
            CrabSubaddressId subaddressId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            var legacyEvent = new AddressSubaddressStatusWasImportedFromCrab(
                subaddressStatusId,
                subaddressId,
                addressStatus,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            ApplyStatusChangesFor(_subAddressStatusChronicle, legacyEvent);

            ApplyAddressCompletion();

            ApplyChange(legacyEvent);
        }

        public void ImportSubaddressPositionFromCrab(
            CrabAddressPositionId addressPositionId,
            CrabSubaddressId subaddressId,
            WkbGeometry addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabAddressNature addressNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            var legacyEvent = new AddressSubaddressPositionWasImportedFromCrab(
                addressPositionId,
                subaddressId,
                addressPosition,
                addressPositionOrigin,
                addressNature,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            ApplyPositionChangeFor<AddressSubaddressPositionWasImportedFromCrab, int>(_crabSubaddressPositionEvents, legacyEvent);

            ApplyAddressCompletion();

            ApplyChange(legacyEvent);
        }

        public void AssignOsloId(
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            if (_osloId == null)
                ApplyChange(new AddressOsloIdWasAssigned(_addressId, osloId, assignmentDate));
        }

        public void RequestOsloId(IOsloIdGenerator osloIdGenerator)
        {
            if (_osloId == null)
                AssignOsloId(osloIdGenerator.GenerateNextOsloId(), new OsloAssignmentDate(Instant.FromDateTimeOffset(DateTimeOffset.Now)));
        }

        private void EvaluatePositionChange()
        {
            if (!IsSubaddress)
                ApplyPositionChangeFor<AddressHouseNumberPositionWasImportedFromCrab, int>(_crabHouseNumberPositionEvents, null);
            else
                ApplyPositionChangeFor<AddressSubaddressPositionWasImportedFromCrab, int>(_crabSubaddressPositionEvents, null);
        }

        private void EvaluateRetirement(
            CrabLifetime crabLifetime,
            CrabModification? modification)
        {
            if (crabLifetime.EndDateTime.HasValue && _status != AddressStatus.Retired)
                if (modification == CrabModification.Correction)
                    ApplyStatusCorrection(AddressStatus.Retired);
                else
                    ApplyStatusChange(AddressStatus.Retired);

            if (!crabLifetime.EndDateTime.HasValue && _status == AddressStatus.Retired)
                if (modification == CrabModification.Correction)
                    ApplyStatusCorrection(_previousStatus);
                else
                    ApplyStatusChange(_previousStatus);

            EvaluatePositionChange();

            ApplyAddressCompletion();
        }

        private void ApplyAddressCompletion()
        {
            if (_status.HasValue && _geometry != null && _officiallyAssigned.HasValue && !_isComplete)
                ApplyChange(new AddressBecameComplete(_addressId));
            else if ((!_status.HasValue || _geometry == null || !_officiallyAssigned.HasValue) && _isComplete)
                ApplyChange(new AddressBecameIncomplete(_addressId));
        }

        private void ApplyHouseNumberChange(
            CrabStreetNameId crabStreetNameId,
            HouseNumber houseNumber,
            CrabModification? modification)
        {
            if (HouseNumber != houseNumber)
                if (modification != CrabModification.Correction)
                    ApplyChange(new AddressHouseNumberWasChanged(_addressId, houseNumber));
                else
                    ApplyChange(new AddressHouseNumberWasCorrected(_addressId, houseNumber));

            var streetNameId = StreetNameId.CreateFor(crabStreetNameId);
            if (streetNameId == StreetNameId)
                return;

            if (modification != CrabModification.Correction)
                ApplyChange(new AddressStreetNameWasChanged(_addressId, streetNameId));
            else
                ApplyChange(new AddressStreetNameWasCorrected(_addressId, streetNameId));
        }

        private void ApplyPostalCodeChangeFor(
            AddressHouseNumberMailCantonWasImportedFromCrab legacyEvent,
            CrabModification? modification = null)
        {
            var crabMailCanton = _mailCantonChronicle.MostCurrent(legacyEvent);
            var newPostalCode = crabMailCanton == null ? null : new PostalCode(crabMailCanton.MailCantonCode);

            if (_postalCode != newPostalCode && crabMailCanton != null)
                if ((modification ?? crabMailCanton.Modification) != CrabModification.Correction)
                    ApplyChange(new AddressPostalCodeWasChanged(_addressId, newPostalCode));
                else
                    ApplyChange(new AddressPostalCodeWasCorrected(_addressId, newPostalCode));
            else if (crabMailCanton == null && _postalCode != null)
                ApplyChange(new AddressPostalCodeWasRemoved(_addressId));
        }

        private void ApplyPositionChangeFor<T, TKey>(
            List<T> events,
            T legacyEvent)
            where T : ICrabEvent, IHasCrabPosition, IHasCrabKey<TKey>
        {
            var mostQualitativeCrabPosition = GetLastMostQualitativeCrabPosition<T, TKey>(events, legacyEvent);

            if (mostQualitativeCrabPosition == null || mostQualitativeCrabPosition.Modification == CrabModification.Delete)
            {
                if (_geometry != null)
                    ApplyChange(new AddressPositionWasRemoved(_addressId));
            }
            else
            {
                var mostQualitativeGeometry = new AddressGeometry(
                    MapToGeometryMethod(mostQualitativeCrabPosition.AddressPositionOrigin),
                    MapToGeometrySpecification(mostQualitativeCrabPosition.AddressPositionOrigin),
                    CreateEWkb(mostQualitativeCrabPosition.AddressPosition.ToByteArray()));

                if (_geometry == mostQualitativeGeometry)
                    return;

                if (mostQualitativeCrabPosition.Modification != CrabModification.Correction)
                    ApplyChange(new AddressWasPositioned(_addressId, mostQualitativeGeometry));
                else
                    ApplyChange(new AddressPositionWasCorrected(_addressId, mostQualitativeGeometry));
            }
        }

        private static ExtendedWkbGeometry CreateEWkb(byte[] wkb)
        {
            if (wkb == null)
                return null;

            var geometry = WKBReaderFactory.Create()
                .Read(wkb);

            geometry.SRID = SpatialReferenceSystemId.Lambert72;

            var wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
            return new ExtendedWkbGeometry(wkbWriter.Write(geometry));
        }

        private void ApplyStatusChangesFor<T, TKey>(
            Chronicle<T, TKey> chronicle,
            T legacyEvent) where T : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<TKey>
        {
            var crabStatusEvent = chronicle.MostCurrent(legacyEvent);
            var newStatus = crabStatusEvent == null ? null : Map(crabStatusEvent.AddressStatus, crabStatusEvent.Modification);

            if (_status != newStatus)
            {
                if (crabStatusEvent != null && crabStatusEvent.Modification == CrabModification.Correction)
                {
                    if (!IsRetired)
                        ApplyStatusCorrection(newStatus);
                }
                else
                {
                    if (!IsRetired)
                        ApplyStatusChange(newStatus);
                }
            }

            if (crabStatusEvent?.Modification == CrabModification.Correction)
                ApplyOfficallyAssignedCorrection(crabStatusEvent.AddressStatus);
            else
                ApplyOfficallyAssignedChange(crabStatusEvent?.AddressStatus, crabStatusEvent?.Modification);
        }

        private void ApplyOfficallyAssignedChange(
            CrabAddressStatus? addressStatus,
            CrabModification? modification)
        {
            var canBecomeOfficiallyAssigned = !_officiallyAssigned.HasValue || _officiallyAssigned == false;
            var shouldBecomeOfficallyAssigned = addressStatus != CrabAddressStatus.Unofficial && modification.HasValue && modification != CrabModification.Delete;
            var canBecomeNotOfficallyAssigned = !_officiallyAssigned.HasValue || _officiallyAssigned == true;
            var shouldBecomeNotOfficallyAssigned = addressStatus == CrabAddressStatus.Unofficial && modification.HasValue && modification != CrabModification.Delete;

            if ((modification == CrabModification.Delete || !modification.HasValue) && _officiallyAssigned.HasValue)
                ApplyChange(new AddressOfficialAssignmentWasRemoved(_addressId));
            else if (canBecomeOfficiallyAssigned && shouldBecomeOfficallyAssigned)
                ApplyChange(new AddressWasOfficiallyAssigned(_addressId));
            else if (canBecomeNotOfficallyAssigned && shouldBecomeNotOfficallyAssigned)
                ApplyChange(new AddressBecameNotOfficiallyAssigned(_addressId));
        }

        private void ApplyOfficallyAssignedCorrection(CrabAddressStatus addressStatus)
        {
            var canBecomeOfficiallyAssigned = !_officiallyAssigned.HasValue || _officiallyAssigned == false;
            var shouldBecomeOfficallyAssigned = addressStatus != CrabAddressStatus.Unofficial;
            var canBecomeNotOfficallyAssigned = !_officiallyAssigned.HasValue || _officiallyAssigned == true;
            var shouldBecomeNotOfficallyAssigned = addressStatus == CrabAddressStatus.Unofficial;

            if (canBecomeOfficiallyAssigned && shouldBecomeOfficallyAssigned)
                ApplyChange(new AddressWasCorrectedToOfficiallyAssigned(_addressId));
            else if (canBecomeNotOfficallyAssigned && shouldBecomeNotOfficallyAssigned)
                ApplyChange(new AddressWasCorrectedToNotOfficiallyAssigned(_addressId));
        }

        private void ApplyStatusChange(AddressStatus? status)
        {
            switch (status)
            {
                case AddressStatus.Proposed:
                    ApplyChange(new AddressWasProposed(_addressId));
                    break;

                case AddressStatus.Retired:
                    ApplyChange(new AddressWasRetired(_addressId));
                    break;

                case AddressStatus.Current:
                    ApplyChange(new AddressBecameCurrent(_addressId));
                    break;

                case null:
                    ApplyChange(new AddressStatusWasRemoved(_addressId));
                    break;
            }
        }

        private void ApplyStatusCorrection(AddressStatus? status)
        {
            switch (status)
            {
                case AddressStatus.Proposed:
                    ApplyChange(new AddressWasCorrectedToProposed(_addressId));
                    break;

                case AddressStatus.Retired:
                    ApplyChange(new AddressWasCorrectedToRetired(_addressId));
                    break;

                case AddressStatus.Current:
                    ApplyChange(new AddressWasCorrectedToCurrent(_addressId));
                    break;

                case null:
                    ApplyChange(new AddressStatusWasCorrectedToRemoved(_addressId));
                    break;
            }
        }

        private GeometryMethod MapToGeometryMethod(CrabAddressPositionOrigin addressPositionOrigin)
        {
            switch (addressPositionOrigin)
            {
                case CrabAddressPositionOrigin.ManualIndicationFromLot:
                case CrabAddressPositionOrigin.ManualIndicationFromParcel:
                case CrabAddressPositionOrigin.ManualIndicationFromBuilding:
                case CrabAddressPositionOrigin.ManualIndicationFromMailbox:
                case CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection:
                case CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad:
                case CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding:
                case CrabAddressPositionOrigin.ManualIndicationFromStand:
                case CrabAddressPositionOrigin.ManualIndicationFromBerth:
                    return GeometryMethod.AppointedByAdministrator;

                case CrabAddressPositionOrigin.DerivedFromBuilding:
                case CrabAddressPositionOrigin.DerivedFromParcelGrb:
                case CrabAddressPositionOrigin.DerivedFromParcelCadastre:
                case CrabAddressPositionOrigin.DerivedFromStreet:
                case CrabAddressPositionOrigin.DerivedFromMunicipality:
                    return GeometryMethod.DerivedFromObject;

                case CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding:
                case CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb:
                case CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre:
                case CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection:
                    return GeometryMethod.Interpolated;

                default:
                    throw new NotImplementedException($"Cannot map {addressPositionOrigin} to GeometryMethod");
            }
        }

        private GeometrySpecification MapToGeometrySpecification(CrabAddressPositionOrigin addressPositionOrigin)
        {
            switch (addressPositionOrigin)
            {
                case CrabAddressPositionOrigin.ManualIndicationFromLot:
                    return GeometrySpecification.Lot;

                case CrabAddressPositionOrigin.ManualIndicationFromParcel:
                case CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad:
                case CrabAddressPositionOrigin.DerivedFromParcelGrb:
                case CrabAddressPositionOrigin.DerivedFromParcelCadastre:
                case CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding:
                case CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb:
                case CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre:
                    return GeometrySpecification.Parcel;

                case CrabAddressPositionOrigin.ManualIndicationFromBuilding:
                case CrabAddressPositionOrigin.ManualIndicationFromMailbox:
                case CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection:
                case CrabAddressPositionOrigin.DerivedFromBuilding:
                    return GeometrySpecification.BuildingUnit;

                case CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding:
                    return GeometrySpecification.Entry;

                case CrabAddressPositionOrigin.ManualIndicationFromBerth:
                    return GeometrySpecification.Berth;

                case CrabAddressPositionOrigin.ManualIndicationFromStand:
                    return GeometrySpecification.Stand;

                case CrabAddressPositionOrigin.DerivedFromStreet:
                case CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection:
                    return GeometrySpecification.RoadSegment;

                case CrabAddressPositionOrigin.DerivedFromMunicipality:
                    return GeometrySpecification.Municipality;

                default:
                    throw new NotImplementedException($"Cannot map {addressPositionOrigin} to GeometrySpecification");
            }
        }

        private AddressStatus? Map(CrabAddressStatus crabAddressStatus,
            CrabModification? modification)
        {
            if (modification == CrabModification.Delete)
                return null;

            switch (crabAddressStatus)
            {
                case CrabAddressStatus.InUse:
                case CrabAddressStatus.OutOfUse:
                case CrabAddressStatus.Unofficial:
                    return AddressStatus.Current;

                case CrabAddressStatus.Reserved:
                case CrabAddressStatus.Proposed:
                    return AddressStatus.Proposed;
                default:
                    throw new NotImplementedException();
            }
        }

        private void GuardRemoved(CrabModification? modification)
        {
            if (IsRemoved && modification != CrabModification.Delete)
                throw new AddressRemovedException($"Cannot change removed address for address id {_addressId}");
        }

        private T GetLastMostQualitativeCrabPosition<T, TKey>(
            List<T> events,
            T latestEvent)
            where T : ICrabEvent, IHasCrabPosition, IHasCrabKey<TKey>
        {
            var allPositionEvents = events.Concat(latestEvent != null ? new[] { latestEvent } : new T[0]);

            var lastEventsPerPositionId = allPositionEvents
                .GroupBy(e => e.Key)
                .Select(group =>
                {
                    var deleteEvent = group.FirstOrDefault(e => e.Modification == CrabModification.Delete);
                    if (deleteEvent != null)
                        return deleteEvent;

                    return group.Last();
                });

            var mostQualitativePosition = lastEventsPerPositionId
                .Where(e => e.Modification != CrabModification.Delete && (!e.EndDateTime.HasValue || IsRetired) && !string.IsNullOrEmpty(e.AddressPosition))
                .OrderBy(e => e.AddressPositionOrigin, new CrabAddressPositionComparer())
                .LastOrDefault();

            return mostQualitativePosition;
        }
    }
}
