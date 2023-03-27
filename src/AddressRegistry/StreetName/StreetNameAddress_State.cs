namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures;
    using Events;

    public partial class StreetNameAddress
    {
        private readonly StreetNameAddresses _children = new StreetNameAddresses();
        private IStreetNameEvent? _lastEvent;

        private string _lastSnapshottedEventHash = string.Empty;
        private ProvenanceData _lastSnapshottedProvenance;
        private StreetNamePersistentLocalId _streetNamePersistentLocalId;

        public AddressPersistentLocalId AddressPersistentLocalId { get; private set; }
        public AddressStatus Status { get; private set; }
        public bool IsActive => Status is AddressStatus.Proposed or AddressStatus.Current && !IsRemoved;
        public HouseNumber HouseNumber { get; private set; }
        public BoxNumber? BoxNumber { get; private set; }
        public bool IsHouseNumberAddress => !IsBoxNumberAddress;
        public bool IsBoxNumberAddress => BoxNumber is not null;
        public PostalCode? PostalCode { get; private set; }
        public AddressGeometry Geometry { get; private set; }
        public bool IsOfficiallyAssigned { get; set; }
        public bool IsRemoved { get; private set; }

        public StreetNameAddress? Parent { get; private set; }
        public IReadOnlyCollection<StreetNameAddress> Children => _children;

        public AddressId? LegacyAddressId { get; private set; }

        //TODO: Make EventHash Value object in Grar.Common
        public string LastEventHash => _lastEvent is null ? _lastSnapshottedEventHash : _lastEvent.GetHash();

        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshottedProvenance : _lastEvent.Provenance;

        public StreetNameAddress(Action<object> applier) : base(applier)
        {
            Register<AddressWasMigratedToStreetName>(When);
            Register<AddressWasProposedV2>(When);
            Register<AddressWasApproved>(When);
            Register<AddressWasRejected>(When);
            Register<AddressWasRejectedBecauseHouseNumberWasRejected>(When);
            Register<AddressWasRejectedBecauseHouseNumberWasRetired>(When);
            Register<AddressWasRejectedBecauseStreetNameWasRetired>(When);
            Register<AddressWasRetiredV2>(When);
            Register<AddressWasRetiredBecauseHouseNumberWasRetired>(When);
            Register<AddressWasRetiredBecauseStreetNameWasRetired>(When);
            Register<AddressWasRemovedV2>(When);
            Register<AddressWasRemovedBecauseHouseNumberWasRemoved>(When);
            Register<AddressWasDeregulated>(When);
            Register<AddressWasRegularized>(When);
            Register<AddressWasCorrectedFromApprovedToProposed>(When);
            Register<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(When);
            Register<AddressWasCorrectedFromRejectedToProposed>(When);
            Register<AddressWasCorrectedFromRetiredToCurrent>(When);
            Register<AddressPositionWasCorrectedV2>(When);
            Register<AddressPostalCodeWasCorrectedV2>(When);
            Register<AddressHouseNumberWasCorrectedV2>(When);
            Register<AddressBoxNumberWasCorrectedV2>(When);
            Register<AddressRegularizationWasCorrected>(When);
            Register<AddressDeregulationWasCorrected>(When);
            Register<AddressPositionWasChanged>(When);
            Register<AddressPostalCodeWasChangedV2>(When);
            Register<AddressWasRemovedBecauseStreetNameWasRemoved>(When);

            Register<StreetNameNamesWereCorrected>(When);
            Register<StreetNameHomonymAdditionsWereCorrected>(When);
            Register<StreetNameHomonymAdditionsWereRemoved>(When);
        }

        private void When(AddressWasMigratedToStreetName @event)
        {
            _streetNamePersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            AddressPersistentLocalId = new AddressPersistentLocalId(@event.AddressPersistentLocalId);
            Status = @event.Status;
            HouseNumber = new HouseNumber(@event.HouseNumber);
            BoxNumber = string.IsNullOrEmpty(@event.BoxNumber) ? null : new BoxNumber(@event.BoxNumber);
            PostalCode = string.IsNullOrEmpty(@event.PostalCode) ? null : new PostalCode(@event.PostalCode);
            Geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            IsOfficiallyAssigned = @event.OfficiallyAssigned;
            IsRemoved = @event.IsRemoved;

            LegacyAddressId = new AddressId(@event.AddressId);

            _lastEvent = @event;
        }

        private void When(AddressWasProposedV2 @event)
        {
            _streetNamePersistentLocalId = new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId);
            AddressPersistentLocalId = new AddressPersistentLocalId(@event.AddressPersistentLocalId);
            HouseNumber = new HouseNumber(@event.HouseNumber);
            Status = AddressStatus.Proposed;
            BoxNumber = string.IsNullOrEmpty(@event.BoxNumber) ? null : new BoxNumber(@event.BoxNumber);
            PostalCode = new PostalCode(@event.PostalCode);
            IsOfficiallyAssigned = true;
            Geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));

            _lastEvent = @event;
        }

        private void When(AddressWasApproved @event)
        {
            Status = AddressStatus.Current;

            _lastEvent = @event;
        }

        private void When(AddressWasRejected @event)
        {
            Status = AddressStatus.Rejected;

            _lastEvent = @event;
        }

        private void When(AddressWasRejectedBecauseHouseNumberWasRejected @event)
        {
            Status = AddressStatus.Rejected;

            _lastEvent = @event;
        }

        private void When(AddressWasRejectedBecauseHouseNumberWasRetired @event)
        {
            Status = AddressStatus.Rejected;

            _lastEvent = @event;
        }

        private void When(AddressWasRejectedBecauseStreetNameWasRetired @event)
        {
            Status = AddressStatus.Rejected;

            _lastEvent = @event;
        }

        private void When(AddressWasRetiredV2 @event)
        {
            Status = AddressStatus.Retired;

            _lastEvent = @event;
        }

        private void When(AddressWasRetiredBecauseHouseNumberWasRetired @event)
        {
            Status = AddressStatus.Retired;

            _lastEvent = @event;
        }

        private void When(AddressWasRetiredBecauseStreetNameWasRetired @event)
        {
            Status = AddressStatus.Retired;

            _lastEvent = @event;
        }

        private void When(AddressWasRemovedV2 @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        private void When(AddressWasRemovedBecauseHouseNumberWasRemoved @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        private void When(AddressWasRegularized @event)
        {
            IsOfficiallyAssigned = true;

            _lastEvent = @event;
        }

        private void When(AddressWasDeregulated @event)
        {
            IsOfficiallyAssigned = false;
            Status = AddressStatus.Current;

            _lastEvent = @event;
        }

        private void When(AddressPositionWasCorrectedV2 @event)
        {
            Geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));

            _lastEvent = @event;
        }

        private void When(AddressPostalCodeWasCorrectedV2 @event)
        {
            PostalCode = new PostalCode(@event.PostalCode);

            foreach (var boxNumberPersistentLocalId in @event.BoxNumberPersistentLocalIds)
            {
                var boxNumberAddress = _children.Single(x => x.AddressPersistentLocalId == boxNumberPersistentLocalId);
                boxNumberAddress.PostalCode = new PostalCode(@event.PostalCode);
            }

            _lastEvent = @event;
        }

        private void When(AddressHouseNumberWasCorrectedV2 @event)
        {
            HouseNumber = new HouseNumber(@event.HouseNumber);

            foreach (var boxNumberPersistentLocalId in @event.BoxNumberPersistentLocalIds)
            {
                var boxNumberAddress = _children.Single(x => x.AddressPersistentLocalId == boxNumberPersistentLocalId);
                boxNumberAddress.HouseNumber = new HouseNumber(@event.HouseNumber);
            }

            _lastEvent = @event;
        }

        private void When(AddressBoxNumberWasCorrectedV2 @event)
        {
            BoxNumber = new BoxNumber(@event.BoxNumber);
            _lastEvent = @event;
        }

        private void When(AddressRegularizationWasCorrected @event)
        {
            IsOfficiallyAssigned = false;
            Status = AddressStatus.Current;

            _lastEvent = @event;
        }

        private void When(AddressDeregulationWasCorrected @event)
        {
            IsOfficiallyAssigned = true;

            _lastEvent = @event;
        }

        internal void RestoreSnapshot(StreetNamePersistentLocalId streetNamePersistentLocalId, AddressData addressData)
        {
            _streetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = new AddressPersistentLocalId(addressData.AddressPersistentLocalId);
            Status = addressData.Status;
            HouseNumber = new HouseNumber(addressData.HouseNumber);
            BoxNumber = string.IsNullOrEmpty(addressData.BoxNumber) ? null : new BoxNumber(addressData.BoxNumber);
            PostalCode = new PostalCode(addressData.PostalCode);
            Geometry = new AddressGeometry(
                addressData.GeometryMethod.Value,
                addressData.GeometrySpecification.Value,
                new ExtendedWkbGeometry(addressData.ExtendedWkbGeometry));

            if (!string.IsNullOrEmpty(addressData.ExtendedWkbGeometry))
            {
                Geometry = new AddressGeometry(
                    addressData.GeometryMethod!.Value,
                    addressData.GeometrySpecification!.Value,
                    new ExtendedWkbGeometry(addressData.ExtendedWkbGeometry));
            }

            IsOfficiallyAssigned = addressData.IsOfficiallyAssigned;
            IsRemoved = addressData.IsRemoved;

            LegacyAddressId = addressData.LegacyAddressId.HasValue ? new AddressId(addressData.LegacyAddressId.Value) : null;
            _lastSnapshottedEventHash = addressData.LastEventHash;
            _lastSnapshottedProvenance = addressData.LastProvenanceData;
        }

        private void When(AddressWasCorrectedFromApprovedToProposed @event)
        {
            Status = AddressStatus.Proposed;

            _lastEvent = @event;
        }

        private void When(AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected @event)
        {
            Status = AddressStatus.Proposed;

            _lastEvent = @event;
        }

        private void When(AddressWasCorrectedFromRejectedToProposed @event)
        {
            Status = AddressStatus.Proposed;
            _lastEvent = @event;
        }

        private void When(AddressWasCorrectedFromRetiredToCurrent @event)
        {
            Status = AddressStatus.Current;

            _lastEvent = @event;
        }

        private void When(AddressPositionWasChanged @event)
        {
            Geometry = new AddressGeometry(
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));

            _lastEvent = @event;
        }

        private void When(AddressPostalCodeWasChangedV2 @event)
        {
            PostalCode = new PostalCode(@event.PostalCode);

            foreach (var boxNumberPersistentLocalId in @event.BoxNumberPersistentLocalIds)
            {
                var boxNumberAddress = _children.Single(x => x.AddressPersistentLocalId == boxNumberPersistentLocalId);
                boxNumberAddress.PostalCode = new PostalCode(@event.PostalCode);
            }

            _lastEvent = @event;
        }

        private void When(StreetNameNamesWereCorrected @event)
        {
            _lastEvent = @event;
        }

        private void When(StreetNameHomonymAdditionsWereCorrected @event)
        {
            _lastEvent = @event;
        }

        private void When(StreetNameHomonymAdditionsWereRemoved @event)
        {
            _lastEvent = @event;
        }

        private void When(AddressWasRemovedBecauseStreetNameWasRemoved @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        internal void Readdress(ReaddressAddressData readdressItem, StreetNameWasReaddressed @event)
        {
            HouseNumber = new HouseNumber(readdressItem.DestinationHouseNumber);
            Status = readdressItem.SourceStatus;
            PostalCode = new PostalCode(readdressItem.SourcePostalCode);
            IsOfficiallyAssigned = readdressItem.SourceIsOfficiallyAssigned;
            Geometry = new AddressGeometry(
                readdressItem.SourceGeometryMethod,
                readdressItem.SourceGeometrySpecification,
                new ExtendedWkbGeometry(readdressItem.SourceExtendedWkbGeometry));

            _lastEvent = @event;
        }
    }
}
