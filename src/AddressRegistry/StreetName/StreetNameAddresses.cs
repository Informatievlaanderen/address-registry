namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
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

        public StreetNameAddress GetParentByLegacyAddressId(AddressId parentAddressId)
        {
            var result = this.SingleOrDefault(x => EqualityComparer<Guid>.Default.Equals(parentAddressId, x.LegacyAddressId ?? AddressId.Default));
            if (result is null)
            {
                throw new ParentAddressNotFoundException();
            }

            return result;
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
