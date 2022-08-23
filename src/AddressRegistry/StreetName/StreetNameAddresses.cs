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

        public bool HasPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId, out StreetNameAddress? streetNameAddress)
        {
            streetNameAddress = this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            return streetNameAddress is not null;
        }

        public StreetNameAddress? FindByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public StreetNameAddress GetRequiredByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
        {
            var address = this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (address is null)
            {
                throw new AddressIsNotFoundException(addressPersistentLocalId);
            }

            return address;
        }

        public StreetNameAddress GetByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.Single(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public StreetNameAddress? FindParentByLegacyAddressId(AddressId parentAddressId)
        {
            return this.SingleOrDefault(x => EqualityComparer<Guid>.Default.Equals(parentAddressId, x.LegacyAddressId ?? AddressId.Default));
        }

        public StreetNameAddress? FindActiveParentByHouseNumber(HouseNumber houseNumber)
        {
            var result = this.SingleOrDefault(x =>
                x.IsActive
               && x.HouseNumber == houseNumber
               && x.BoxNumber is null);

            return result;
        }
    }
}
