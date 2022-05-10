namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Events;
    using Exceptions;

    public class StreetNameAddress : Entity
    {
        private readonly StreetNameAddresses _children = new StreetNameAddresses();
        private IHaveHash _lastEvent;
        public AddressPersistentLocalId AddressPersistentLocalId { get; private set; }
        public AddressStatus Status { get; private set; }
        public HouseNumber HouseNumber { get; private set; }
        public BoxNumber? BoxNumber { get; private set; }
        public PostalCode PostalCode { get; private set; }
        public AddressGeometry Geometry { get; private set; }
        public bool IsOfficiallyAssigned { get; set; }
        public bool IsRemoved { get; private set; }

        public StreetNameAddress? Parent { get; private set; }
        public IReadOnlyCollection<StreetNameAddress> Children => _children;

        public AddressId? LegacyAddressId { get; private set; }

        public string LastEventHash => _lastEvent.GetHash();

        public bool IsActive => Status == AddressStatus.Proposed || Status == AddressStatus.Current;

        public StreetNameAddress(Action<object> applier) : base(applier)
        {
            Register<AddressWasMigratedToStreetName>(When);
            Register<AddressWasProposedV2>(When);
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

            _lastEvent = @event;
        }
    }
}
