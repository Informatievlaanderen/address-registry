namespace AddressRegistry.StreetName
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Commands;
    using DataStructures;
    using Events;
    using Exceptions;

    public partial class StreetNameAddress : Entity
    {
        public StreetNameAddress AddChild(StreetNameAddress streetNameAddress)
        {
            if (_children.HasPersistentLocalId(streetNameAddress.AddressPersistentLocalId))
            {
                throw new StreetNameAddressChildAlreadyExistsException();
            }

            _children.Add(streetNameAddress);
            streetNameAddress.SetParent(this);

            return streetNameAddress;
        }

        public StreetNameAddress RemoveChild(StreetNameAddress streetNameAddress)
        {
            _children.Remove(streetNameAddress);
            streetNameAddress.SetParent(null);

            return streetNameAddress;
        }

        public bool BoxNumberIsUnique(BoxNumber boxNumber)
        {
            return _children.FirstOrDefault(x =>
                    x.IsActive
                 && x.BoxNumber is not null
                 && x.BoxNumber == boxNumber) is null;
        }

        public void Approve()
        {
            GuardNotRemovedAddress();

            if (Parent is not null && Parent.Status != AddressStatus.Current)
            {
                throw new ParentAddressHasInvalidStatusException();
            }

            switch (Status)
            {
                case AddressStatus.Current:
                    return;
                case AddressStatus.Retired or AddressStatus.Rejected:
                    throw new AddressHasInvalidStatusException();
                case AddressStatus.Proposed:
                    Apply(new AddressWasApproved(_streetNamePersistentLocalId, AddressPersistentLocalId));
                    break;
            }
        }

        public void CorrectApproval()
        {
            GuardNotRemovedAddress();

            if (!IsOfficiallyAssigned)
            {
                throw new AddressIsNotOfficiallyAssignedException();
            }

            switch (Status)
            {
                case AddressStatus.Proposed:
                    return;
                case AddressStatus.Retired or AddressStatus.Rejected:
                    throw new AddressHasInvalidStatusException();
                case AddressStatus.Current:
                    foreach (var child in _children)
                    {
                        child.CorrectApprovalBecauseParentWasCorrected();
                    }

                    Apply(new AddressWasCorrectedFromApprovedToProposed(_streetNamePersistentLocalId, AddressPersistentLocalId));
                    break;
            }
        }

        private void CorrectApprovalBecauseParentWasCorrected()
        {
            if (IsRemoved)
            {
                return;
            }

            if (!IsOfficiallyAssigned)
            {
                return;
            }

            if (Status == AddressStatus.Current)
            {
                Apply(new AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void CorrectRetirement()
        {
            GuardNotRemovedAddress();

            switch (Status)
            {
                case AddressStatus.Current:
                    return;
                case AddressStatus.Proposed or AddressStatus.Rejected:
                    throw new AddressHasInvalidStatusException();
                case AddressStatus.Retired:
                    Apply(new AddressWasCorrectedFromRetiredToCurrent(_streetNamePersistentLocalId, AddressPersistentLocalId));
                    break;
            }
        }

        public void Reject()
        {
            GuardNotRemovedAddress();

            switch (Status)
            {
                case AddressStatus.Rejected:
                    return;
                case AddressStatus.Current or AddressStatus.Retired:
                    throw new AddressHasInvalidStatusException();
                case AddressStatus.Proposed:
                    foreach (var child in _children)
                    {
                        child.RejectBecauseParentWasRejected();
                    }

                    Apply(new AddressWasRejected(_streetNamePersistentLocalId, AddressPersistentLocalId));
                    break;
            }
        }

        private void RejectBecauseParentWasRejected()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseHouseNumberWasRejected(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void RejectBecauseStreetNameWasRetired()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Rejected)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseStreetNameWasRetired(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void Deregulate()
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            if (!IsOfficiallyAssigned)
            {
                return;
            }

            Apply(new AddressWasDeregulated(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void Regularize()
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            if (IsOfficiallyAssigned)
            {
                return;
            }

            Apply(new AddressWasRegularized(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void Retire()
        {
            GuardNotRemovedAddress();

            switch (Status)
            {
                case AddressStatus.Retired:
                    return;
                case AddressStatus.Proposed or AddressStatus.Rejected:
                    throw new AddressHasInvalidStatusException();
                case AddressStatus.Current:
                    foreach (var child in _children.Where(address => address.Status == AddressStatus.Current))
                    {
                        child.RetireBecauseParentWasRetired();
                    }

                    foreach (var child in _children.Where(address => address.Status == AddressStatus.Proposed))
                    {
                        child.RejectBecauseParentWasRetired();
                    }

                    Apply(new AddressWasRetiredV2(_streetNamePersistentLocalId, AddressPersistentLocalId));
                    break;
            }
        }

        private void RetireBecauseParentWasRetired()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Current)
            {
                Apply(new AddressWasRetiredBecauseHouseNumberWasRetired(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        private void RejectBecauseParentWasRetired()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseHouseNumberWasRetired(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void RetireBecauseStreetNameWasRetired()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Retired)
            {
                return;
            }

            if (Status == AddressStatus.Current)
            {
                Apply(new AddressWasRetiredBecauseStreetNameWasRetired(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void ChangePosition(
            GeometryMethod geometryMethod,
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position,
            Func<MunicipalityData> getMunicipalityData)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            GuardGeometry(geometryMethod, geometrySpecification, position);

            var newGeometry = GetFinalGeometry(
                geometryMethod,
                geometrySpecification,
                position,
                getMunicipalityData);

            if (Geometry != newGeometry)
            {
                Apply(new AddressPositionWasChanged(
                    _streetNamePersistentLocalId,
                    AddressPersistentLocalId,
                    newGeometry.GeometryMethod,
                    newGeometry.GeometrySpecification,
                    newGeometry.Geometry));
            }
        }

        public void ChangePostalCode(PostalCode postalCode)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            if (PostalCode == postalCode)
            {
                return;
            }

            var boxNumbers = _children
                .Where(x => !x.IsRemoved && validStatuses.Contains(x.Status))
                .Select(x => x.AddressPersistentLocalId);

            Apply(new AddressPostalCodeWasChangedV2(
                _streetNamePersistentLocalId,
                AddressPersistentLocalId,
                boxNumbers,
                postalCode));
        }

        public void CorrectPosition(
            GeometryMethod geometryMethod,
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position,
            Func<MunicipalityData> getMunicipalityData)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            GuardGeometry(geometryMethod, geometrySpecification, position);

            var newGeometry = GetFinalGeometry(
                geometryMethod,
                geometrySpecification,
                position,
                getMunicipalityData);

            if (Geometry != newGeometry)
            {
                Apply(new AddressPositionWasCorrectedV2(
                    _streetNamePersistentLocalId,
                    AddressPersistentLocalId,
                    newGeometry.GeometryMethod,
                    newGeometry.GeometrySpecification,
                    newGeometry.Geometry));
            }
        }

        public void CorrectPostalCode(PostalCode postalCode, Action guardPostalCodeMunicipalityMatchesStreetNameMunicipality)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            guardPostalCodeMunicipalityMatchesStreetNameMunicipality();

            if (PostalCode == postalCode)
            {
                return;
            }

            var boxNumbers = _children
                .Where(x => !x.IsRemoved && validStatuses.Contains(x.Status))
                .Select(x => x.AddressPersistentLocalId);

            Apply(new AddressPostalCodeWasCorrectedV2(
                _streetNamePersistentLocalId,
                AddressPersistentLocalId,
                boxNumbers,
                postalCode));
        }

        public void CorrectHouseNumber(HouseNumber houseNumber, Action guardHouseNumberAddressIsUnique)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            guardHouseNumberAddressIsUnique();

            if (BoxNumber is not null)
            {
                throw new HouseNumberToCorrectHasBoxNumberException();
            }

            if (HouseNumber == houseNumber)
            {
                return;
            }

            var boxNumbers = _children
                .Where(x => !x.IsRemoved && validStatuses.Contains(x.Status))
                .Select(x => x.AddressPersistentLocalId);

            Apply(new AddressHouseNumberWasCorrectedV2(
                _streetNamePersistentLocalId,
                AddressPersistentLocalId,
                boxNumbers,
                houseNumber));
        }

        public void CorrectBoxNumber(BoxNumber boxNumber, Action guardBoxNumberAddressIsUnique)
        {
            GuardNotRemovedAddress();

            if (BoxNumber is null)
            {
                throw new AddressHasNoBoxNumberException();
            }

            if (BoxNumber == boxNumber)
            {
                return;
            }

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            guardBoxNumberAddressIsUnique();

            Apply(new AddressBoxNumberWasCorrectedV2(
                _streetNamePersistentLocalId,
                AddressPersistentLocalId,
                boxNumber));
        }

        public static AddressGeometry GetFinalGeometry(
            GeometryMethod geometryMethod,
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position,
            Func<MunicipalityData> getMunicipalityData)
        {
            var finalSpecification = geometryMethod == GeometryMethod.DerivedFromObject
                ? GeometrySpecification.Municipality
                : geometrySpecification!.Value;
            var finalPosition = geometryMethod == GeometryMethod.DerivedFromObject
                ? getMunicipalityData.Invoke().Centroid()
                : position!;

            return new AddressGeometry(geometryMethod, finalSpecification, finalPosition);
        }

        public void Remove()
        {
            if (IsRemoved)
            {
                return;
            }

            foreach (var child in _children)
            {
                child.RemovedBecauseParentWasRemoved();
            }

            Apply(new AddressWasRemovedV2(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void RemovedBecauseParentWasRemoved()
        {
            if (IsRemoved)
            {
                return;
            }

            Apply(new AddressWasRemovedBecauseHouseNumberWasRemoved(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void CorrectAddressRejection()
        {
            GuardNotRemovedAddress();

            if (Status == AddressStatus.Proposed)
            {
                return;
            }

            if (Status != AddressStatus.Rejected)
            {
                throw new AddressHasInvalidStatusException();
            }

            Apply(new AddressWasCorrectedFromRejectedToProposed(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        /// <summary>
        /// Set the parent of the instance.
        /// </summary>
        /// <param name="parentStreetNameAddress">The parent instance.</param>
        /// <returns>The instance of which you have set the parent.</returns>
        public StreetNameAddress SetParent(StreetNameAddress? parentStreetNameAddress)
        {
            Parent = parentStreetNameAddress;
            return this;
        }
    }
}
