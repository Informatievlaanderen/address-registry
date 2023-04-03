namespace AddressRegistry.StreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using Address;
    using Commands;
    using DataStructures;
    using Exceptions;

    public sealed class StreetNameReaddresser
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly IEnumerable<ReaddressAddressItem> _addressesToReaddress;
        private readonly StreetName _streetName;

        public List<ReaddressedAddressData> ReaddressedAddresses { get; }
        public List<(ReaddressAction action, AddressPersistentLocalId addressPersistentLocalId)> Actions { get; }

        public StreetNameReaddresser(
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> addressesToReaddress,
            StreetName streetName)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _addressesToReaddress = addressesToReaddress;
            _streetName = streetName;

            ReaddressedAddresses = new List<ReaddressedAddressData>();
            Actions = new List<(ReaddressAction, AddressPersistentLocalId)>();

            Readdress();
        }

        public IEnumerable<AddressPersistentLocalId> GetAddressesByActions(params ReaddressAction[] actions)
            => Actions.Where(x => actions.Contains(x.action)).Select(x => x.addressPersistentLocalId);

        private void Readdress()
        {
            foreach (var addressToReaddress in _addressesToReaddress)
            {
                var sourceAddress = GetSourceAddress(addressToReaddress);

                var destinationAddress = _streetName.StreetNameAddresses.FindActiveParentByHouseNumber(addressToReaddress.DestinationHouseNumber);
                var destinationAddressPersistentLocalId = destinationAddress?.AddressPersistentLocalId;

                if (addressToReaddress.SourceAddressPersistentLocalId == destinationAddressPersistentLocalId)
                {
                    throw new SourceAndDestinationAddressAreTheSameException(
                        addressToReaddress.SourceAddressPersistentLocalId,
                        addressToReaddress.DestinationHouseNumber);
                }

                if (destinationAddress is null)
                {
                    destinationAddressPersistentLocalId = new AddressPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                    Actions.Add((ReaddressAction.ProposeHouseNumber, destinationAddressPersistentLocalId));
                }
                else if (IsOnlyUsedAsDestinationAddress(destinationAddress))
                {
                    // 11 -> 13
                    // 11 is the source address and 13 is ONLY a destination address. Therefore 13's box numbers should be rejected or retired.
                    foreach (var boxNumberAddress in destinationAddress.Children.Where(x => x.IsActive))
                    {
                        RejectOrRetireAddress(boxNumberAddress);
                    }
                }

                ReaddressedAddresses.Add(new ReaddressedAddressData(
                    addressToReaddress.SourceAddressPersistentLocalId,
                    destinationAddressPersistentLocalId!,
                    sourceAddress.Status,
                    addressToReaddress.DestinationHouseNumber,
                    boxNumber: null,
                    sourceAddress.PostalCode!,
                    sourceAddress.Geometry,
                    sourceAddress.IsOfficiallyAssigned,
                    parentAddressPersistentLocalId: null
                ));

                ReaddressBoxNumbers(addressToReaddress, sourceAddress, destinationAddress, destinationAddressPersistentLocalId!);
            }
        }

        private StreetNameAddress GetSourceAddress(ReaddressAddressItem item)
        {
            var sourceAddress =
                _streetName.StreetNameAddresses.GetNotRemovedByPersistentLocalId(item.SourceAddressPersistentLocalId);

            if (!sourceAddress.IsHouseNumberAddress)
            {
                throw new AddressHasBoxNumberException(sourceAddress.AddressPersistentLocalId);
            }

            if (!sourceAddress.IsActive)
            {
                throw new AddressHasInvalidStatusException(sourceAddress.AddressPersistentLocalId);
            }

            if (sourceAddress.PostalCode is null)
            {
                throw new AddressHasNoPostalCodeException(sourceAddress.AddressPersistentLocalId);
            }

            return sourceAddress;
        }

        private void ReaddressBoxNumbers(
            ReaddressAddressItem addressToReaddress,
            StreetNameAddress sourceAddress,
            StreetNameAddress? destinationAddress,
            AddressPersistentLocalId destinationAddressPersistentLocalId)
        {
            foreach (var boxNumberAddress in sourceAddress.Children.Where(x => x.IsActive))
            {
                var destinationBoxNumberAddress = destinationAddress?.FindBoxNumberAddress(boxNumberAddress.BoxNumber!);
                var boxNumberPersistentLocalId = destinationBoxNumberAddress?.AddressPersistentLocalId;

                if (destinationBoxNumberAddress is null)
                {
                    boxNumberPersistentLocalId = new AddressPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                    Actions.Add((ReaddressAction.ProposeBoxNumber, boxNumberPersistentLocalId));
                }

                ReaddressedAddresses.Add(new ReaddressedAddressData(
                    boxNumberAddress.AddressPersistentLocalId,
                    boxNumberPersistentLocalId!,
                    boxNumberAddress.Status,
                    addressToReaddress.DestinationHouseNumber,
                    boxNumberAddress.BoxNumber,
                    sourceAddress.PostalCode!,
                    boxNumberAddress.Geometry,
                    boxNumberAddress.IsOfficiallyAssigned,
                    destinationAddressPersistentLocalId
                ));

                // 11   ->  13   ->   15
                // 11A      13A1
                // 11B      13B
                // When house number 13 is the current sourceAddress, then _readdressedAddresses will contain boxNumberAddress 13B and we should keep it.
                // In the case of house number 11, the sourceAddress' house number is never used as a destination house number, so we should keep its box numbers.
                if (IsUsedAsDestinationAddress(sourceAddress) && !HasBeenReaddressed(boxNumberAddress))
                {
                    RejectOrRetireAddress(boxNumberAddress);
                }
            }
        }

        private void RejectOrRetireAddress(StreetNameAddress boxNumberAddress)
        {
            if (boxNumberAddress.Status == AddressStatus.Proposed)
            {
                Actions.Add((ReaddressAction.Reject, boxNumberAddress.AddressPersistentLocalId));
            }
            else if (boxNumberAddress.Status == AddressStatus.Current)
            {
                Actions.Add((ReaddressAction.Retire, boxNumberAddress.AddressPersistentLocalId));
            }
        }

        private bool IsOnlyUsedAsDestinationAddress(StreetNameAddress address)
        {
            return !_addressesToReaddress.Select(x => x.SourceAddressPersistentLocalId).Contains(address.AddressPersistentLocalId);
        }

        private bool IsUsedAsDestinationAddress(StreetNameAddress address)
        {
            return _addressesToReaddress.Any(x => x.DestinationHouseNumber == address.HouseNumber);
        }

        private bool HasBeenReaddressed(StreetNameAddress boxNumberAddress)
        {
            return ReaddressedAddresses.Any(
                x => x.DestinationAddressPersistentLocalId == boxNumberAddress.AddressPersistentLocalId);
        }
    }

    public enum ReaddressAction
    {
        ProposeHouseNumber,
        ProposeBoxNumber,
        Reject,
        Retire,
    }
}
