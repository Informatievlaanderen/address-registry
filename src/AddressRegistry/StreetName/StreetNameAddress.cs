namespace AddressRegistry.StreetName
{
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
                    Apply(new AddressWasRetiredV2(_streetNamePersistentLocalId, AddressPersistentLocalId));
                    break;
            }

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Current))
            {
                child.RetireBecauseParentWasRetired();
            }

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Proposed))
            {
                child.RejectBecauseParentWasRetired();
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
            IMunicipalities municipalities)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            GuardGeometry(geometryMethod, geometrySpecification, position);

            var newGeometry = GetFinalGeometry(geometryMethod, geometrySpecification, position, municipalities);

            if (Geometry is null || Geometry != newGeometry)
            {
                Apply(new AddressPositionWasChanged(
                    _streetNamePersistentLocalId,
                    AddressPersistentLocalId,
                    newGeometry.GeometryMethod,
                    newGeometry.GeometrySpecification,
                    newGeometry.Geometry));
            }
        }

        public void CorrectPosition(
            GeometryMethod geometryMethod,
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position,
            IMunicipalities municipalities)
        {
            GuardNotRemovedAddress();

            var validStatuses = new[] { AddressStatus.Proposed, AddressStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            GuardGeometry(geometryMethod, geometrySpecification, position);

            var newGeometry = GetFinalGeometry(geometryMethod, geometrySpecification, position, municipalities);

            if (Geometry is null || Geometry != newGeometry)
            {
                Apply(new AddressPositionWasCorrectedV2(
                    _streetNamePersistentLocalId,
                    AddressPersistentLocalId,
                    newGeometry.GeometryMethod,
                    newGeometry.GeometrySpecification,
                    newGeometry.Geometry));
            }
        }

        private AddressGeometry GetFinalGeometry(
            GeometryMethod geometryMethod,
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position,
            IMunicipalities municipalities)
        {
            var finalSpecification = geometryMethod == GeometryMethod.DerivedFromObject
                ? GeometrySpecification.Municipality
                : geometrySpecification!.Value;
            var finalPosition = geometryMethod == GeometryMethod.DerivedFromObject
                ? municipalities.Get(_streetName.MunicipalityId).Centroid()
                : position!;

            return new AddressGeometry(geometryMethod, finalSpecification, finalPosition);
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
