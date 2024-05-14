namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

            var streetNameReaddresser = new StreetNameReaddresser(this, streetNames, persistentLocalIdGenerator, addressesToReaddress);
            var readdressedHouseNumbers = streetNameReaddresser.ReaddressedHouseNumbers.ToList();
            var readdressedBoxNumbers = streetNameReaddresser.ReaddressedBoxNumbers.ToList();

            // Propose Actions
            var proposeActions = streetNameReaddresser.Actions.Where(x =>
                x.action is ReaddressAction.ProposeHouseNumber or ReaddressAction.ProposeBoxNumber);

            foreach (var (action, addressPersistentLocalId) in proposeActions)
            {
                HandleAction(
                    action,
                    addressPersistentLocalId,
                    readdressedHouseNumbers,
                    readdressedBoxNumbers,
                    executionContext);
            }

            var readdressedEvents = new List<AddressHouseNumberWasReaddressed>();
            foreach (var (addressPersistentLocalId, readdressedData) in streetNameReaddresser.ReaddressedAddresses)
            {
                var addressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
                    PersistentLocalId,
                    addressPersistentLocalId,
                    readdressedData.readdressedHouseNumber,
                    readdressedData.readdressedBoxNumbers);

                readdressedEvents.Add(addressHouseNumberWasReaddressed);
                ApplyChange(addressHouseNumberWasReaddressed);
            }

            ApplyChange(new StreetNameWasReaddressed(PersistentLocalId, readdressedEvents));

            // Reject or Retire BoxNumber Actions
            var rejectRetireBoxNumberActions = streetNameReaddresser.Actions.Where(x =>
                x.action is ReaddressAction.RejectBoxNumber or ReaddressAction.RetireBoxNumber);

            foreach (var (action, addressPersistentLocalId) in rejectRetireBoxNumberActions)
            {
                HandleAction(
                    action,
                    addressPersistentLocalId,
                    readdressedHouseNumbers,
                    readdressedBoxNumbers,
                    executionContext);
            }

            // Reject or retire the specified addresses which are within the current street name.
            var addresssToRejectOrRetireWithinStreetName = addressesToRejectOrRetire
                .Where(x => x.StreetNamePersistentLocalId == PersistentLocalId)
                .ToList();
            RejectOrRetireAddresses(addresssToRejectOrRetireWithinStreetName);

            executionContext.AddressesUpdated.AddRange(
                readdressedHouseNumbers.Select(x => (PersistentLocalId, new AddressPersistentLocalId(x.DestinationAddressPersistentLocalId))));
            executionContext.AddressesUpdated.AddRange(
                addresssToRejectOrRetireWithinStreetName.Select(x => (PersistentLocalId, x.AddressPersistentLocalId)));
        }

        private void HandleAction(
            ReaddressAction action,
            AddressPersistentLocalId addressPersistentLocalId,
            IEnumerable<ReaddressedAddressData> readdressedHouseNumbers,
            IEnumerable<ReaddressedAddressData> readdressedBoxNumbers,
            ReaddressExecutionContext executionContext)
        {
            switch (action)
            {
                case ReaddressAction.ProposeHouseNumber:
                    var addressData = readdressedHouseNumbers.Single(x =>
                        x.DestinationAddressPersistentLocalId == addressPersistentLocalId);

                    ProposeAddressBecauseOfReaddress(
                        addressPersistentLocalId,
                        new AddressPersistentLocalId(addressData.SourceAddressPersistentLocalId),
                        new PostalCode(addressData.SourcePostalCode),
                        new HouseNumber(addressData.DestinationHouseNumber),
                        null,
                        addressData.SourceGeometryMethod,
                        addressData.SourceGeometrySpecification,
                        new ExtendedWkbGeometry(addressData.SourceExtendedWkbGeometry)
                    );

                    executionContext.AddressesAdded.Add((PersistentLocalId, addressPersistentLocalId));
                    break;

                case ReaddressAction.ProposeBoxNumber:
                    addressData = readdressedBoxNumbers.Single(x =>
                        x.DestinationAddressPersistentLocalId == addressPersistentLocalId);

                    ProposeAddressBecauseOfReaddress(
                        addressPersistentLocalId,
                        new AddressPersistentLocalId(addressData.SourceAddressPersistentLocalId),
                        new PostalCode(addressData.SourcePostalCode),
                        new HouseNumber(addressData.DestinationHouseNumber),
                        new BoxNumber(addressData.SourceBoxNumber!),
                        addressData.SourceGeometryMethod,
                        addressData.SourceGeometrySpecification,
                        new ExtendedWkbGeometry(addressData.SourceExtendedWkbGeometry)
                    );

                    executionContext.AddressesAdded.Add((PersistentLocalId, addressPersistentLocalId));
                    break;

                case ReaddressAction.RejectBoxNumber:
                    RejectAddressBecauseOfReaddress(addressPersistentLocalId);
                    break;

                case ReaddressAction.RetireBoxNumber:
                    RetireAddressBecauseOfReaddress(addressPersistentLocalId);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ProposeAddressBecauseOfReaddress(
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry geometryPosition)
        {
            var parent = StreetNameAddresses.FindActiveParentByHouseNumber(houseNumber);

            ApplyChange(new AddressWasProposedBecauseOfReaddress(
                PersistentLocalId,
                destinationAddressPersistentLocalId,
                sourceAddressPersistentLocalId,
                parent?.AddressPersistentLocalId,
                postalCode,
                houseNumber,
                boxNumber,
                geometryMethod,
                geometrySpecification,
                geometryPosition));
        }

        private void RejectOrRetireAddresses(IEnumerable<RetireAddressItem> addresssToRejectOrRetireWithinStreetName)
        {
            foreach (var (_, addressPersistentLocalId) in addresssToRejectOrRetireWithinStreetName)
            {
                var sourceAddress = StreetNameAddresses.GetByPersistentLocalId(addressPersistentLocalId);

                if (sourceAddress.Status == AddressStatus.Proposed)
                {
                    RejectAddressBecauseOfReaddress(addressPersistentLocalId);
                }

                if (sourceAddress.Status == AddressStatus.Current)
                {
                    RetireAddressBecauseOfReaddress(addressPersistentLocalId);
                }
            }
        }

        public void RejectOrRetireAddressForReaddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            var address = StreetNameAddresses.GetByPersistentLocalId(addressPersistentLocalId);

            if (address.IsRemoved)
            {
                return;
            }

            if (address.Status != AddressStatus.Proposed && address.Status != AddressStatus.Current)
            {
                return;
            }

            if (address.Status == AddressStatus.Proposed)
            {
                RejectAddressBecauseOfReaddress(addressPersistentLocalId);
            }

            if (address.Status == AddressStatus.Current)
            {
                RetireAddressBecauseOfReaddress(addressPersistentLocalId);
            }
        }

        private void RejectAddressBecauseOfReaddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .RejectBecauseOfReaddress();
        }

        private void RetireAddressBecauseOfReaddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            StreetNameAddresses
                .GetNotRemovedByPersistentLocalId(addressPersistentLocalId)
                .RetireBecauseOfReaddress();
        }
    }
}
