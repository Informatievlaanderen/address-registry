namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures;
    using Events;
    using Exceptions;

    public class StreetNameAddress : Entity
    {
        private readonly StreetNameAddresses _children = new StreetNameAddresses();
        private IStreetNameEvent? _lastEvent;

        private string _lastSnapshottedEventHash = string.Empty;
        private ProvenanceData _lastSnapshottedProvenance;

        public AddressPersistentLocalId AddressPersistentLocalId { get; private set; }
        public AddressStatus Status { get; private set; }
        public bool IsActive => Status is AddressStatus.Proposed or AddressStatus.Current;
        public HouseNumber HouseNumber { get; private set; }
        public BoxNumber? BoxNumber { get; private set; }
        public PostalCode PostalCode { get; private set; }
        public AddressGeometry? Geometry { get; private set; }
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
        }
        
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

        private void When(AddressWasMigratedToStreetName @event)
        {
            AddressPersistentLocalId = new AddressPersistentLocalId(@event.AddressPersistentLocalId);
            Status = @event.Status;
            HouseNumber = new HouseNumber(@event.HouseNumber);
            BoxNumber = string.IsNullOrEmpty(@event.BoxNumber) ? null : new BoxNumber(@event.BoxNumber);
            PostalCode = new PostalCode(@event.PostalCode);
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
            AddressPersistentLocalId = new AddressPersistentLocalId(@event.AddressPersistentLocalId);
            HouseNumber = new HouseNumber(@event.HouseNumber);
            Status = AddressStatus.Proposed;
            BoxNumber = string.IsNullOrEmpty(@event.BoxNumber) ? null : new BoxNumber(@event.BoxNumber);
            PostalCode = new PostalCode(@event.PostalCode);
            IsOfficiallyAssigned = true;

            _lastEvent = @event;
        }

        private void When(AddressWasApproved @event)
        {
            Status = AddressStatus.Current;

            _lastEvent = @event;
        }

        internal void RestoreSnapshot(AddressData addressData)
        {
            AddressPersistentLocalId = new AddressPersistentLocalId(addressData.AddressPersistentLocalId);
            Status = addressData.Status;
            HouseNumber = new HouseNumber(addressData.HouseNumber);
            BoxNumber = string.IsNullOrEmpty(addressData.BoxNumber) ? null : new BoxNumber(addressData.BoxNumber);
            PostalCode = new PostalCode(addressData.PostalCode);

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
    }
}
