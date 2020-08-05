namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;
    using System.Collections.Generic;
    using System.Linq;

    public partial class Address
    {
        private readonly List<AddressHouseNumberPositionWasImportedFromCrab> _crabHouseNumberPositionEvents =
            new List<AddressHouseNumberPositionWasImportedFromCrab>();

        private readonly List<AddressSubaddressPositionWasImportedFromCrab> _crabSubaddressPositionEvents =
            new List<AddressSubaddressPositionWasImportedFromCrab>();

        private readonly Chronicle<AddressHouseNumberStatusWasImportedFromCrab, int> _houseNumberStatusChronicle =
            new Chronicle<AddressHouseNumberStatusWasImportedFromCrab, int>();

        private readonly Chronicle<AddressHouseNumberMailCantonWasImportedFromCrab, int> _mailCantonChronicle =
            new Chronicle<AddressHouseNumberMailCantonWasImportedFromCrab, int>();

        private readonly Chronicle<AddressSubaddressStatusWasImportedFromCrab, int> _subAddressStatusChronicle =
            new Chronicle<AddressSubaddressStatusWasImportedFromCrab, int>();

        private readonly Dictionary<CrabHouseNumberId, AddressHouseNumberWasImportedFromCrab> _lastHouseNumberEventsForSubaddress =
            new Dictionary<CrabHouseNumberId, AddressHouseNumberWasImportedFromCrab>();

        private readonly Dictionary<CrabHouseNumberId, AddressHouseNumberMailCantonWasImportedFromCrab> _lastHouseNumberMailCantonEventsForSubaddress =
            new Dictionary<CrabHouseNumberId, AddressHouseNumberMailCantonWasImportedFromCrab>();

        private AddressId _addressId;
        private BoxNumber _boxNumber;
        private AddressGeometry _geometry;
        private bool _isComplete;
        private bool? _officiallyAssigned;
        private PostalCode _postalCode;
        private AddressStatus? _previousStatus;
        private AddressStatus? _status;
        private CrabHouseNumberId _coupledHouseNumberId;

        public StreetNameId StreetNameId { get; private set; }
        public HouseNumber HouseNumber { get; private set; }
        public PersistentLocalId PersistentLocalId { get; private set; }

        public bool IsRemoved { get; private set; }

        public bool IsSubaddress { get; private set; }

        public Modification LastModificationBasedOnCrab { get; private set; }

        private Address()
        {
            Register<AddressWasRegistered>(When);
            Register<AddressWasRemoved>(When);

            Register<AddressBecameComplete>(When);
            Register<AddressBecameIncomplete>(When);

            Register<AddressWasPositioned>(When);
            Register<AddressPositionWasRemoved>(When);
            Register<AddressPositionWasCorrected>(When);

            Register<AddressWasOfficiallyAssigned>(When);
            Register<AddressBecameNotOfficiallyAssigned>(When);
            Register<AddressOfficialAssignmentWasRemoved>(When);
            Register<AddressWasCorrectedToNotOfficiallyAssigned>(When);
            Register<AddressWasCorrectedToOfficiallyAssigned>(When);

            Register<AddressWasProposed>(When);
            Register<AddressWasRetired>(When);
            Register<AddressBecameCurrent>(When);
            Register<AddressStatusWasRemoved>(When);
            Register<AddressWasCorrectedToCurrent>(When);
            Register<AddressWasCorrectedToProposed>(When);
            Register<AddressWasCorrectedToRetired>(When);
            Register<AddressStatusWasCorrectedToRemoved>(When);

            Register<AddressHouseNumberWasChanged>(When);
            Register<AddressHouseNumberWasCorrected>(When);

            Register<AddressPostalCodeWasChanged>(When);
            Register<AddressPostalCodeWasCorrected>(When);
            Register<AddressPostalCodeWasRemoved>(When);

            Register<AddressBoxNumberWasChanged>(When);
            Register<AddressBoxNumberWasCorrected>(When);
            Register<AddressBoxNumberWasRemoved>(When);

            Register<AddressStreetNameWasChanged>(When);
            Register<AddressStreetNameWasCorrected>(When);

            Register<AddressPersistentLocalIdWasAssigned>(When);

            Register<AddressSubaddressWasImportedFromCrab>(When);
            Register<AddressHouseNumberWasImportedFromCrab>(When);
            Register<AddressHouseNumberMailCantonWasImportedFromCrab>(When);
            Register<AddressHouseNumberStatusWasImportedFromCrab>(When);
            Register<AddressSubaddressStatusWasImportedFromCrab>(When);
            Register<AddressHouseNumberPositionWasImportedFromCrab>(When);
            Register<AddressSubaddressPositionWasImportedFromCrab>(When);
        }

        #region HouseNumber/Boxnumber
        private void When(AddressHouseNumberWasChanged @event)
        {
            HouseNumber = new HouseNumber(@event.HouseNumber);
        }

        private void When(AddressHouseNumberWasCorrected @event)
        {
            HouseNumber = new HouseNumber(@event.HouseNumber);
        }

        private void When(AddressBoxNumberWasRemoved obj)
        {
            _boxNumber = null;
        }

        private void When(AddressBoxNumberWasCorrected @event)
        {
            _boxNumber = new BoxNumber(@event.BoxNumber);
        }

        private void When(AddressBoxNumberWasChanged @event)
        {
            _boxNumber = new BoxNumber(@event.BoxNumber);
        }
        #endregion

        #region StreetName
        private void When(AddressStreetNameWasCorrected @event)
        {
            StreetNameId = new StreetNameId(@event.StreetNameId);
        }

        private void When(AddressStreetNameWasChanged @event)
        {
            StreetNameId = new StreetNameId(@event.StreetNameId);
        }
        #endregion

        #region PostalCode
        private void When(AddressPostalCodeWasChanged @event)
        {
            _postalCode = new PostalCode(@event.PostalCode);
        }

        private void When(AddressPostalCodeWasCorrected @event)
        {
            _postalCode = new PostalCode(@event.PostalCode);
        }

        private void When(AddressPostalCodeWasRemoved @event)
        {
            _postalCode = null;
        }
        #endregion

        #region Position
        private void When(AddressPositionWasRemoved @event)
        {
            _geometry = null;
        }

        private void When(AddressWasPositioned @event)
        {
            _geometry = new AddressGeometry(@event.GeometryMethod, @event.GeometrySpecification, new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
        }

        private void When(AddressPositionWasCorrected @event)
        {
            _geometry = new AddressGeometry(@event.GeometryMethod, @event.GeometrySpecification, new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
        }
        #endregion Position

        #region Officialy assigned
        private void When(AddressOfficialAssignmentWasRemoved @event)
        {
            _officiallyAssigned = null;
        }

        private void When(AddressBecameNotOfficiallyAssigned @event)
        {
            _officiallyAssigned = false;
        }

        private void When(AddressWasOfficiallyAssigned @event)
        {
            _officiallyAssigned = true;
        }

        private void When(AddressWasCorrectedToOfficiallyAssigned @event)
        {
            _officiallyAssigned = true;
        }

        private void When(AddressWasCorrectedToNotOfficiallyAssigned @event)
        {
            _officiallyAssigned = false;
        }
        #endregion

        #region Status
        private void When(AddressBecameCurrent @event)
        {
            SetStatus(AddressStatus.Current);
        }

        private void When(AddressWasRetired @event)
        {
            SetStatus(AddressStatus.Retired);
        }

        private void When(AddressWasProposed @event)
        {
            SetStatus(AddressStatus.Proposed);
        }

        private void When(AddressStatusWasCorrectedToRemoved @event)
        {
            SetStatus(null);
        }

        private void When(AddressWasCorrectedToRetired @event)
        {
            SetStatus(AddressStatus.Retired);
        }

        private void When(AddressWasCorrectedToProposed @event)
        {
            SetStatus(AddressStatus.Proposed);
        }

        private void When(AddressWasCorrectedToCurrent @event)
        {
            SetStatus(AddressStatus.Current);
        }

        private void When(AddressStatusWasRemoved @event)
        {
            SetStatus(null);
        }

        private void SetStatus(AddressStatus? status)
        {
            _previousStatus = _status;
            _status = status;
        }
        #endregion Status

        private void When(AddressBecameIncomplete @event)
        {
            _isComplete = false;
        }

        private void When(AddressBecameComplete @event)
        {
            _isComplete = true;
        }

        private void When(AddressWasRegistered @event)
        {
            _addressId = new AddressId(@event.AddressId);
            HouseNumber = new HouseNumber(@event.HouseNumber);
            StreetNameId = new StreetNameId(@event.StreetNameId);
        }

        private void When(AddressWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(AddressPersistentLocalIdWasAssigned @event)
        {
            PersistentLocalId = new PersistentLocalId(@event.PersistentLocalId);
        }

        #region CRAB
        private void When(AddressHouseNumberWasImportedFromCrab @event)
        {
            var houseNumberId = new CrabHouseNumberId(@event.HouseNumberId);
            if (!_lastHouseNumberEventsForSubaddress.Any())
                _coupledHouseNumberId = houseNumberId;

            _lastHouseNumberEventsForSubaddress[houseNumberId] = @event;
            WhenCrabEventApplied(@event.Modification == CrabModification.Delete);
        }

        private void When(AddressSubaddressWasImportedFromCrab @event)
        {
            IsSubaddress = true;
            _coupledHouseNumberId = new CrabHouseNumberId(@event.HouseNumberId);
            WhenCrabEventApplied(@event.Modification == CrabModification.Delete);
        }

        private void When(AddressHouseNumberStatusWasImportedFromCrab @event)
        {
            _houseNumberStatusChronicle.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressHouseNumberPositionWasImportedFromCrab @event)
        {
            _crabHouseNumberPositionEvents.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressHouseNumberMailCantonWasImportedFromCrab @event)
        {
            _mailCantonChronicle.Add(@event);
            _lastHouseNumberMailCantonEventsForSubaddress[new CrabHouseNumberId(@event.HouseNumberId)] = @event;
            WhenCrabEventApplied();
        }

        private void When(AddressSubaddressPositionWasImportedFromCrab @event)
        {
            _crabSubaddressPositionEvents.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(AddressSubaddressStatusWasImportedFromCrab @event)
        {
            _subAddressStatusChronicle.Add(@event);
            WhenCrabEventApplied();
        }

        private void WhenCrabEventApplied(bool isDeleted = false)
        {
            if (isDeleted)
                LastModificationBasedOnCrab = Modification.Delete;
            else if (LastModificationBasedOnCrab == Modification.Unknown)
                LastModificationBasedOnCrab = Modification.Insert;
            else if (LastModificationBasedOnCrab == Modification.Insert)
                LastModificationBasedOnCrab = Modification.Update;
        }
        #endregion CRAB
    }
}
