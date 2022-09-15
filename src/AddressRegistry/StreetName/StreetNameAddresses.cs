namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    public class StreetNameAddresses : List<StreetNameAddress>
    {
        public IEnumerable<StreetNameAddress> CurrentStreetNameAddresses =>
            this.Where(x => !x.IsRemoved && x.Status == AddressStatus.Current);

        public IEnumerable<StreetNameAddress> ProposedStreetNameAddresses =>
            this.Where(x => !x.IsRemoved && x.Status == AddressStatus.Proposed);

        public bool HasPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.Any(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public bool HasPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId,
            out StreetNameAddress? streetNameAddress)
        {
            streetNameAddress = this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            return streetNameAddress is not null;
        }

        public StreetNameAddress? FindByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public StreetNameAddress GetByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
        {
            var address = this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

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
            AddressPersistentLocalId addressPersistentLocalId)
        {
            return this.Any(x =>
                x.IsActive
                && x.AddressPersistentLocalId != addressPersistentLocalId
                && x.HouseNumber == houseNumber
                && x.BoxNumber is null);
        }
    }
}
