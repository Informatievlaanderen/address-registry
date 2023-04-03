namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Address;
    using Commands;
    using DataStructures;
    using Exceptions;

    public sealed class StreetNameReaddresser
    {
        private readonly IStreetNames _streetNames;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly IEnumerable<ReaddressAddressItem> _addressesToReaddress;
        private readonly StreetName _streetName;

        public List<ReaddressedAddressData> ReaddressedAddresses { get; }
        public List<(ReaddressAction action, AddressPersistentLocalId addressPersistentLocalId)> Actions { get; }

        public StreetNameReaddresser(
            IStreetNames streetNames,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> addressesToReaddress,
            StreetName streetName)
        {
            _streetNames = streetNames;
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

                if (destinationAddress is null || IsUsedAsSourceAddress(destinationAddress))
                {
                    continue;
                }

                // 11 -> 13
                // 11 is the source address and 13 is ONLY a destination address. Therefore 13's box numbers should be rejected or retired if they were not reused.
                foreach (var boxNumberAddress in destinationAddress.Children.Where(x => x.IsActive))
                {
                    if (ReaddressedAddresses.Any(x =>
                            x.DestinationAddressPersistentLocalId == boxNumberAddress.AddressPersistentLocalId))
                    {
                        continue;
                    }

                    RejectOrRetireAddress(boxNumberAddress);
                }
            }
        }

        private StreetNameAddress GetSourceAddress(ReaddressAddressItem addressToReaddress)
        {
            StreetNameAddress sourceAddress;
            if (addressToReaddress.SourceStreetNamePersistentLocalId != _streetName.PersistentLocalId)
            {
                var otherStreetName = _streetNames
                    .GetAsync(new StreetNameStreamId(addressToReaddress.SourceStreetNamePersistentLocalId)).GetAwaiter().GetResult();
                sourceAddress = otherStreetName.StreetNameAddresses.GetNotRemovedByPersistentLocalId(addressToReaddress.SourceAddressPersistentLocalId);
            }
            else
            {
                sourceAddress = _streetName.StreetNameAddresses.GetNotRemovedByPersistentLocalId(addressToReaddress.SourceAddressPersistentLocalId);
            }

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
            switch (boxNumberAddress.Status)
            {
                case AddressStatus.Proposed:
                    Actions.Add((ReaddressAction.Reject, boxNumberAddress.AddressPersistentLocalId));
                    break;
                case AddressStatus.Current:
                    Actions.Add((ReaddressAction.Retire, boxNumberAddress.AddressPersistentLocalId));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsUsedAsSourceAddress(StreetNameAddress address)
        {
            return _addressesToReaddress.Select(x => x.SourceAddressPersistentLocalId).Contains(address.AddressPersistentLocalId);
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
