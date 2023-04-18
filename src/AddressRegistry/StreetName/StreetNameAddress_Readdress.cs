namespace AddressRegistry.StreetName
{
    using System.Linq;
    using Events;

    public partial class StreetNameAddress
    {
        public void RejectBecauseOfReaddressing()
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
                child.RejectBecauseParentWasRejectedBecauseOfReaddressing();
            }

            Apply(new AddressWasRejectedBecauseOfReaddressing(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        private void RejectBecauseParentWasRejectedBecauseOfReaddressing()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseOfReaddressing(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        public void RetireBecauseOfReaddressing()
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
                child.RetireBecauseParentWasRetiredBecauseOfReaddressing();
            }

            foreach (var child in _children.Where(address => address.Status == AddressStatus.Proposed))
            {
                child.RejectBecauseParentWasRetiredBecauseOfReaddressing();
            }

            Apply(new AddressWasRetiredBecauseOfReaddressing(_streetNamePersistentLocalId, AddressPersistentLocalId));
        }

        private void RetireBecauseParentWasRetiredBecauseOfReaddressing()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Current)
            {
                Apply(new AddressWasRetiredBecauseOfReaddressing(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }

        private void RejectBecauseParentWasRetiredBecauseOfReaddressing()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == AddressStatus.Proposed)
            {
                Apply(new AddressWasRejectedBecauseOfReaddressing(_streetNamePersistentLocalId, AddressPersistentLocalId));
            }
        }
    }
}
