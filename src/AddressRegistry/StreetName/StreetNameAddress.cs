namespace AddressRegistry.StreetName
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
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

        public bool BoxNumberIsUnique(BoxNumber boxNumber)
        {
            return _children.FirstOrDefault(x =>
                    x.IsActive
                 && x.BoxNumber is not null
                 && x.BoxNumber == boxNumber) is null;
        }

        public StreetNameAddress? FindBoxNumberAddress(BoxNumber boxNumber)
        {
            return _children.SingleOrDefault(x => x.BoxNumber! == boxNumber);
        }

        public void Approve()
        {
            GuardNotRemovedAddress();

            if (Parent is not null && Parent.Status != AddressStatus.Current)
            {
                throw new ParentAddressHasInvalidStatusException();
            }

            if (Status == AddressStatus.Current)
            {
                return;
            }

            GuardAddressStatus(AddressStatus.Proposed);

            Apply(new AddressWasApproved(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void Reject()
        {
            GuardNotRemovedAddress();

            if (Status == AddressStatus.Rejected)
            {
                return;
            }

            GuardAddressStatus(AddressStatus.Proposed);

            foreach (var child in _children)
            {
                child.RejectBecauseParentWasRejected();
            }

            Apply(new AddressWasRejected(_streetNamePersistentLocalId, AddressPersistentLocalId));
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

        public void RejectBecauseStreetNameWasRejected()
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
                Apply(new AddressWasRejectedBecauseStreetNameWasRejected(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void RetireBecauseStreetNameWasRejected()
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
                Apply(new AddressWasRetiredBecauseStreetNameWasRejected(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void Retire()
        {
            GuardNotRemovedAddress();

            if (Status == AddressStatus.Retired)
            {
                return;
            }

            GuardAddressStatus(AddressStatus.Current);

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Current))
            {
                child.RetireBecauseParentWasRetired();
            }

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Proposed))
            {
                child.RejectBecauseParentWasRetired();
            }

            Apply(new AddressWasRetiredV2(_streetNamePersistentLocalId, AddressPersistentLocalId));
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

        public void Remove()
        {
            if (IsRemoved)
            {
                return;
            }

            foreach (var child in _children)
            {
                child.RemoveBecauseParentWasRemoved();
            }

            Apply(new AddressWasRemovedV2(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        private void RemoveBecauseParentWasRemoved()
        {
            if (IsRemoved)
            {
                return;
            }

            Apply(new AddressWasRemovedBecauseHouseNumberWasRemoved(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void RemoveBecauseStreetNameWasRemoved()
        {
            if (IsRemoved)
            {
                return;
            }

            Apply(new AddressWasRemovedBecauseStreetNameWasRemoved(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void Regularize()
        {
            GuardNotRemovedAddress();
            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);

            if (IsOfficiallyAssigned)
            {
                return;
            }

            Apply(new AddressWasRegularized(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void Deregulate()
        {
            GuardNotRemovedAddress();
            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);

            if (Parent is not null && Parent.Status == AddressStatus.Proposed)
            {
                throw new ParentAddressHasInvalidStatusException();
            }

            if (!IsOfficiallyAssigned)
            {
                return;
            }

            Apply(new AddressWasDeregulated(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void CorrectPosition(
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry position)
        {
            GuardNotRemovedAddress();
            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);

            GuardGeometry(geometryMethod, geometrySpecification);

            if (Geometry != new AddressGeometry(geometryMethod, geometrySpecification, position))
            {
                Apply(new AddressPositionWasCorrectedV2(
                    _streetNamePersistentLocalId,
                    AddressPersistentLocalId,
                    geometryMethod,
                    geometrySpecification,
                    position));
            }
        }

        public void CorrectPostalCode(PostalCode postalCode, Action guardPostalCodeMunicipalityMatchesStreetNameMunicipality)
        {
            GuardNotRemovedAddress();

            if (IsBoxNumberAddress)
            {
                throw new AddressHasBoxNumberException();
            }

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };
            GuardAddressStatus(validStatuses);

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
            GuardAddressStatus(validStatuses);

            guardHouseNumberAddressIsUnique();

            if (IsBoxNumberAddress)
            {
                throw new AddressHasBoxNumberException();
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

            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);

            guardBoxNumberAddressIsUnique();

            Apply(new AddressBoxNumberWasCorrectedV2(
                _streetNamePersistentLocalId,
                AddressPersistentLocalId,
                boxNumber));
        }

        public void CorrectApproval()
        {
            GuardNotRemovedAddress();

            if (!IsOfficiallyAssigned)
            {
                throw new AddressIsNotOfficiallyAssignedException();
            }

            if (Status == AddressStatus.Proposed)
            {
                return;
            }

            GuardAddressStatus(AddressStatus.Current);

            foreach (var child in _children)
            {
                child.CorrectApprovalBecauseParentWasCorrected();
            }

            Apply(new AddressWasCorrectedFromApprovedToProposed(_streetNamePersistentLocalId, AddressPersistentLocalId));
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

        public void CorrectRejection(Action guardAddressIsUnique)
        {
            GuardNotRemovedAddress();

            if (Parent is not null && Parent.HouseNumber != HouseNumber)
            {
                throw new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException();
            }

            if (Parent is not null && Parent.PostalCode != PostalCode)
            {
                throw new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException();
            }

            if (Status == AddressStatus.Proposed)
            {
                return;
            }

            GuardAddressStatus(AddressStatus.Rejected);

            guardAddressIsUnique();

            Apply(new AddressWasCorrectedFromRejectedToProposed(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void CorrectRetirement(Action guardAddressIsUnique)
        {
            GuardNotRemovedAddress();

            if (Parent is not null && Parent.HouseNumber != HouseNumber)
            {
                throw new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException();
            }

            if (Parent is not null && Parent.PostalCode != PostalCode)
            {
                throw new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException();
            }

            if (Status == AddressStatus.Current)
            {
                return;
            }

            GuardAddressStatus(AddressStatus.Retired);

            guardAddressIsUnique();

            Apply(new AddressWasCorrectedFromRetiredToCurrent(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void CorrectRegularization()
        {
            GuardNotRemovedAddress();
            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);

            if (Parent is not null && Parent.Status == AddressStatus.Proposed)
            {
                throw new ParentAddressHasInvalidStatusException();
            }

            if (!IsOfficiallyAssigned)
            {
                return;
            }

            Apply(new AddressRegularizationWasCorrected(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void CorrectDeregulation()
        {
            GuardNotRemovedAddress();
            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);

            if (IsOfficiallyAssigned)
            {
                return;
            }

            Apply(new AddressDeregulationWasCorrected(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void CorrectRemoval(Action guardAddressIsUnique)
        {
            if (Parent is not null && Parent.HouseNumber != HouseNumber)
            {
                throw new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException();
            }

            if (Parent is not null && Parent.PostalCode != PostalCode)
            {
                throw new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException();
            }

            if (!IsRemoved)
            {
                return;
            }

            guardAddressIsUnique();

            Apply(new AddressRemovalWasCorrected(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        public void ChangePosition(
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry position)
        {
            GuardNotRemovedAddress();
            GuardAddressStatus(AddressStatus.Proposed, AddressStatus.Current);
            GuardGeometry(geometryMethod, geometrySpecification);

            if (Geometry != new AddressGeometry(geometryMethod, geometrySpecification, position))
            {
                Apply(new AddressPositionWasChanged(
                    _streetNamePersistentLocalId,
                    AddressPersistentLocalId,
                    geometryMethod,
                    geometrySpecification,
                    position));
            }
        }

        public void ChangePostalCode(PostalCode postalCode)
        {
            GuardNotRemovedAddress();

            if (IsBoxNumberAddress)
            {
                throw new AddressHasBoxNumberException();
            }

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };
            GuardAddressStatus(validStatuses);

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

        private void GuardAddressStatus(params AddressStatus[] validStatuses)
        {
            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }
        }
    }
}
