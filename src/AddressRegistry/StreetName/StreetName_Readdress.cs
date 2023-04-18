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

            var streetNameReaddresser = new StreetNameReaddresser(this, streetNames, persistentLocalIdGenerator, addressesToReaddress);
            var readdressedHouseNumbers = streetNameReaddresser.ReaddressedHouseNumbers.ToList();
            var readdressedBoxNumbers = streetNameReaddresser.ReaddressedBoxNumbers.ToList();

            // Perform all actions gathered by the StreetNameReaddresser.
            foreach (var (action, addressPersistentLocalId) in streetNameReaddresser.Actions)
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
            RejectOrRetireAddresses(addresssToRejectOrRetireWithinStreetName, streetNameReaddresser);

            executionContext.AddressesUpdated.AddRange(
                readdressedHouseNumbers.Select(x => (PersistentLocalId, new AddressPersistentLocalId(x.DestinationAddressPersistentLocalId))));
            executionContext.AddressesUpdated.AddRange(
                addresssToRejectOrRetireWithinStreetName.Select(x => (PersistentLocalId, x.AddressPersistentLocalId)));

            foreach (var (addressPersistentLocalId, readressedData) in streetNameReaddresser.ReaddressedAddresses)
            {
                ApplyChange(new AddressHouseNumberWasReaddressed(
                    PersistentLocalId,
                    addressPersistentLocalId,
                    readressedData.readdressedHouseNumber,
                    readressedData.readdressedBoxNumbers,
                    readressedData.rejectedBoxNumberPersistentLocalIds,
                    readressedData.retiredBoxNumberPersistentLocalIds));
            }
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

                    ProposeAddressBecauseOfReaddressing(
                        addressPersistentLocalId,
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

                    ProposeAddressBecauseOfReaddressing(
                        addressPersistentLocalId,
                        new PostalCode(addressData.SourcePostalCode),
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

        private void ProposeAddressBecauseOfReaddressing(
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry geometryPosition)
        {
            var parent = StreetNameAddresses.FindActiveParentByHouseNumber(houseNumber);

            ApplyChange(new AddressWasProposedBecauseOfReaddressing(
                PersistentLocalId,
                addressPersistentLocalId,
                parent?.AddressPersistentLocalId,
                postalCode,
                houseNumber,
                boxNumber,
                geometryMethod,
                geometrySpecification,
                geometryPosition));
        }

        private void RejectOrRetireAddresses(
            IEnumerable<RetireAddressItem> addresssToRejectOrRetireWithinStreetName,
            StreetNameReaddresser streetNameReaddresser)
        {
            foreach (var (_, addressPersistentLocalId) in addresssToRejectOrRetireWithinStreetName)
            {
                var sourceAddress = StreetNameAddresses.GetByPersistentLocalId(addressPersistentLocalId);
                var destinationAddress = streetNameReaddresser.ReaddressedAddresses
                    .Single(x => x.Value.readdressedHouseNumber.SourceAddressPersistentLocalId == addressPersistentLocalId)
                    .Value;

                var boxNumberAddressPersistentLocalIds = new List<AddressBoxNumberReplacedBecauseOfReaddressData>();
                foreach (var sourceAddressBoxNumber in sourceAddress.Children.Where(x => x.IsActive))
                {
                    var destinationAddressBoxNumberPersistentLocalId = destinationAddress.readdressedBoxNumbers
                        .Single(x => x.SourceAddressPersistentLocalId == sourceAddressBoxNumber.AddressPersistentLocalId)
                        .DestinationAddressPersistentLocalId;

                    boxNumberAddressPersistentLocalIds.Add(new AddressBoxNumberReplacedBecauseOfReaddressData(
                        sourceAddressBoxNumber.AddressPersistentLocalId,
                        new AddressPersistentLocalId(destinationAddressBoxNumberPersistentLocalId)));
                }

                ApplyChange(new AddressHouseNumberWasReplacedBecauseOfReaddress(
                    PersistentLocalId,
                    PersistentLocalId,
                    addressPersistentLocalId,
                    new AddressPersistentLocalId(destinationAddress.readdressedHouseNumber.DestinationAddressPersistentLocalId),
                    boxNumberAddressPersistentLocalIds));

                if (sourceAddress.Status == AddressStatus.Proposed)
                {
                    RejectAddress(addressPersistentLocalId);
                }

                if (sourceAddress.Status == AddressStatus.Current)
                {
                    RetireAddress(addressPersistentLocalId);
                }
            }
        }

        public void RejectOrRetireAddressForReaddressing(
            StreetNamePersistentLocalId destinationStreetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId,
            IEnumerable<BoxNumberAddressPersistentLocalId> destinationBoxNumbers)
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

            var boxNumbers = new List<AddressBoxNumberReplacedBecauseOfReaddressData>();
            foreach (var destinationBoxNumber in destinationBoxNumbers)
            {
                var sourceAddressBoxNumber = address.Children.Single(x => x.BoxNumber == destinationBoxNumber.BoxNumber);
                boxNumbers.Add(new AddressBoxNumberReplacedBecauseOfReaddressData(
                    sourceAddressBoxNumber.AddressPersistentLocalId,
                    destinationBoxNumber.AddressPersistentLocalId));
            }

            ApplyChange(new AddressHouseNumberWasReplacedBecauseOfReaddress(
                PersistentLocalId,
                destinationStreetNamePersistentLocalId,
                addressPersistentLocalId,
                destinationAddressPersistentLocalId,
                boxNumbers));

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
