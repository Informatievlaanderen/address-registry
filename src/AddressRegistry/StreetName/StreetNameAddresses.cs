namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    public class StreetNameAddresses : IEnumerable<StreetNameAddress>
    {
        private readonly List<StreetNameAddress> _addresses;
        private readonly Dictionary<AddressPersistentLocalId, StreetNameAddress> _addressesByPersistentLocalId;

        public StreetNameAddresses()
        {
            _addresses = new List<StreetNameAddress>();
            _addressesByPersistentLocalId = new Dictionary<AddressPersistentLocalId, StreetNameAddress>();
        }

        public IEnumerable<StreetNameAddress> ProposedStreetNameAddresses =>
            this.Where(x => !x.IsRemoved && x.Status == AddressStatus.Proposed);

        public IEnumerable<StreetNameAddress> CurrentStreetNameAddresses =>
            this.Where(x => !x.IsRemoved && x.Status == AddressStatus.Current);

        public bool HasPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => _addressesByPersistentLocalId.ContainsKey(addressPersistentLocalId);

        public StreetNameAddress? FindByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
        {
            _addressesByPersistentLocalId.TryGetValue(addressPersistentLocalId, out var address);
            return address;
        }

        public StreetNameAddress GetByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
        {
            _addressesByPersistentLocalId.TryGetValue(addressPersistentLocalId, out var address);

            if (address is null)
            {
                throw new AddressIsNotFoundException(addressPersistentLocalId);
            }

            return address;
        }

        public StreetNameAddress GetNotRemovedByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
        {
            var address = GetByPersistentLocalId(addressPersistentLocalId);

            if (address.IsRemoved)
            {
                throw new AddressIsRemovedException(addressPersistentLocalId);
            }

            return address;
        }

        public StreetNameAddress? FindParentByLegacyAddressId(AddressId parentAddressId)
        {
            return this.SingleOrDefault(x =>
                EqualityComparer<Guid>.Default.Equals(parentAddressId, x.LegacyAddressId ?? AddressId.Default));
        }

        public StreetNameAddress? FindActiveParentByHouseNumber(HouseNumber houseNumber)
        {
            var result = this.SingleOrDefault(x =>
                x.IsActive
                && x.HouseNumber == houseNumber
                && x.BoxNumber is null);

            return result;
        }

        public bool HasActiveAddressForOtherThan(
            HouseNumber houseNumber,
            BoxNumber boxNumber,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            return this.Any(x =>
                x.IsActive
                && x.AddressPersistentLocalId != addressPersistentLocalId
                && x.HouseNumber == houseNumber
                && x.BoxNumber == boxNumber);
        }

        public void Add(StreetNameAddress address)
        {
            _addresses.Add(address);
            _addressesByPersistentLocalId.Add(address.AddressPersistentLocalId, address);
        }

        public IReadOnlyCollection<StreetNameAddress> AsReadOnly()
        {
            return _addresses.AsReadOnly();
        }

        public IEnumerator<StreetNameAddress> GetEnumerator()
        {
            return _addresses.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
