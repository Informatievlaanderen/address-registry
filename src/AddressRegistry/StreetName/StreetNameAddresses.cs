namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using Guid = System.Guid;

    public class StreetNameAddresses : List<StreetNameAddress>
    {
        public bool HasPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.Any(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public bool HasPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId, out StreetNameAddress? streetNameAddress)
        {
            streetNameAddress = this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            return streetNameAddress is not null;
        }

        public StreetNameAddress? FindByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public StreetNameAddress GetByPersistentLocalId(AddressPersistentLocalId addressPersistentLocalId)
            => this.Single(x => x.AddressPersistentLocalId == addressPersistentLocalId);

        public StreetNameAddress GetByLegacyAddressId(AddressId parentAddressId)
            => this.Single(x => EqualityComparer<Guid>.Default.Equals(parentAddressId, x.LegacyAddressId ?? AddressId.Default));
    }
}
