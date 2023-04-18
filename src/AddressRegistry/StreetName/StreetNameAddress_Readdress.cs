namespace AddressRegistry.StreetName
{
    using System.Linq;
    using Events;

    public partial class StreetNameAddress
    {
        public void RejectBecauseOfReaddress()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Rejected)
            {
                return;
            }

            foreach (var child in _children)
            {
                child.RejectBecauseParentWasRejectedBecauseOfReaddress();
            }

            Apply(new AddressWasRejectedBecauseOfReaddress(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        private void RejectBecauseParentWasRejectedBecauseOfReaddress()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseOfReaddress(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void RetireBecauseOfReaddress()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Retired)
            {
                return;
            }

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Current))
            {
                child.RetireBecauseParentWasRetiredBecauseOfReaddress();
            }

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Proposed))
            {
                child.RejectBecauseParentWasRetiredBecauseOfReaddress();
            }

            Apply(new AddressWasRetiredBecauseOfReaddress(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        private void RetireBecauseParentWasRetiredBecauseOfReaddress()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Current)
            {
                Apply(new AddressWasRetiredBecauseOfReaddress(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        private void RejectBecauseParentWasRetiredBecauseOfReaddress()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseOfReaddress(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }
    }
}
