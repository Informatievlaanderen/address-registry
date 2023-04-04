namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Address;
    using Commands;
    using DataStructures;
    using Events;

    public partial class StreetName
    {
        public void Readdress(
            IStreetNames streetNames,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> addressesToReaddress,
            IEnumerable<RetireAddressItem> addressesToRejectOrRetire,
            ReaddressExecutionContext executionContext)
        {
            GuardActiveStreetName();

            var addresssToRejectOrRetireWithinStreetName = addressesToRejectOrRetire
                .Where(x => x.StreetNamePersistentLocalId == PersistentLocalId)
                .ToList();

            var streetNameReaddresser = new StreetNameReaddresser(
                streetNames,
                persistentLocalIdGenerator,
                addressesToReaddress,
                addresssToRejectOrRetireWithinStreetName,
                this);

            // Perform all actions gathered by the StreetNameReaddresser.
            foreach (var (action, addressPersistentLocalId) in streetNameReaddresser.Actions)
            {
                HandleAction(action, addressPersistentLocalId, streetNameReaddresser.ReaddressedAddresses, executionContext);
            }

            // Collect all addresses specified from the addressesToRejectOrRetire list which are outside of this StreetName.
            // Theses addresses cannot be rejected or retired in this transaction scope
            // because the ConcurrentUnitOfWork can only handle changes from only one aggregate within a given transaction scope.
            // These addresses will be rejected or retired through one or more separate commands.
            var addressesWhichWillBeRejectedOrRetired = addressesToRejectOrRetire
                .Where(x => x.StreetNamePersistentLocalId != PersistentLocalId)
                .Select(x => x.AddressPersistentLocalId)
                .ToList();

            executionContext.AddressesUpdated.AddRange(
                streetNameReaddresser.ReaddressedAddresses
                    .Where(x => x.IsHouseNumberAddress)
                    .Select(x => (PersistentLocalId, new AddressPersistentLocalId(x.DestinationAddressPersistentLocalId))));
            executionContext.AddressesUpdated.AddRange(
                addresssToRejectOrRetireWithinStreetName.Select(x => (PersistentLocalId, x.AddressPersistentLocalId)));

            ApplyChange(new StreetNameWasReaddressed(
                PersistentLocalId,
                streetNameReaddresser.GetAddressesByActions(ReaddressAction.ProposeHouseNumber, ReaddressAction.ProposeBoxNumber).ToList(),
                streetNameReaddresser.GetAddressesByActions(ReaddressAction.Reject).ToList(),
                streetNameReaddresser.GetAddressesByActions(ReaddressAction.Retire).ToList(),
                addressesWhichWillBeRejectedOrRetired,
                streetNameReaddresser.ReaddressedAddresses));
        }

        private void HandleAction(
            ReaddressAction action,
            AddressPersistentLocalId addressPersistentLocalId,
            IEnumerable<ReaddressedAddressData> readdressedAddresses,
            ReaddressExecutionContext executionContext)
        {
            switch (action)
            {
                case ReaddressAction.ProposeHouseNumber:
                    var addressData = readdressedAddresses.Single(x =>
                        x.DestinationAddressPersistentLocalId == addressPersistentLocalId);

                    ProposeAddress(
                        addressPersistentLocalId,
                        new PostalCode(addressData.SourcePostalCode),
                        MunicipalityId,
                        new HouseNumber(addressData.DestinationHouseNumber),
                        null,
                        addressData.SourceGeometryMethod,
                        addressData.SourceGeometrySpecification,
                        new ExtendedWkbGeometry(addressData.SourceExtendedWkbGeometry)
                    );

                    executionContext.AddressesAdded.Add((PersistentLocalId, addressPersistentLocalId));
                    break;

                case ReaddressAction.ProposeBoxNumber:
                    addressData = readdressedAddresses.Single(x =>
                        x.DestinationAddressPersistentLocalId == addressPersistentLocalId);

                    ProposeAddress(
                        addressPersistentLocalId,
                        new PostalCode(addressData.SourcePostalCode),
                        MunicipalityId,
                        new HouseNumber(addressData.DestinationHouseNumber),
                        new BoxNumber(addressData.SourceBoxNumber!),
                        addressData.SourceGeometryMethod,
                        addressData.SourceGeometrySpecification,
                        new ExtendedWkbGeometry(addressData.SourceExtendedWkbGeometry)
                    );

                    executionContext.AddressesAdded.Add((PersistentLocalId, addressPersistentLocalId));
                    break;

                case ReaddressAction.Reject:
                    RejectAddress(addressPersistentLocalId);
                    break;

                case ReaddressAction.Retire:
                    RetireAddress(addressPersistentLocalId);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void RejectOrRetireAddressesForReaddressing(IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            foreach (var addressPersistentLocalId in addressPersistentLocalIds)
            {
                var address = StreetNameAddresses.GetByPersistentLocalId(addressPersistentLocalId);

                if (address.IsRemoved)
                {
                    continue;
                }

                if (address.Status == AddressStatus.Proposed)
                {
                    RejectAddress(addressPersistentLocalId);
                }

                if (address.Status == AddressStatus.Current)
                {
                    RetireAddress(addressPersistentLocalId);
                }
            }
        }
    }
}
