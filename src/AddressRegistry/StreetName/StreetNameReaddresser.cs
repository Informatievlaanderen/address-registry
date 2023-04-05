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

        public IEnumerable<ReaddressedAddressData> ReaddressedHouseNumbers
            => ReaddressedAddresses.Select(x => x.Value.readdressedHouseNumber);

        public IEnumerable<ReaddressedAddressData> ReaddressedBoxNumbers
            => ReaddressedAddresses.SelectMany(x => x.Value.readdressedBoxNumbers);

        public IDictionary<
            AddressPersistentLocalId,
            (
                ReaddressedAddressData readdressedHouseNumber,
                List<ReaddressedAddressData> readdressedBoxNumbers,
                List<AddressPersistentLocalId> rejectedBoxNumberPersistentLocalIds,
                List<AddressPersistentLocalId> retiredBoxNumberPersistentLocalIds
            )> ReaddressedAddresses { get; }

        public List<(ReaddressAction action, AddressPersistentLocalId addressPersistentLocalId)> Actions { get; }

        public StreetNameReaddresser(
            StreetName streetName,
            IStreetNames streetNames,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> addressesToReaddress)
        {
            _streetNames = streetNames;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _addressesToReaddress = addressesToReaddress;
            _streetName = streetName;

            ReaddressedAddresses =
                new Dictionary<AddressPersistentLocalId, (ReaddressedAddressData readdressedHouseNumber,
                    List<ReaddressedAddressData> readdressedBoxNumbers, List<AddressPersistentLocalId>
                    rejectedBoxNumberPersistentLocalIds, List<AddressPersistentLocalId>
                    retiredBoxNumberPersistentLocalIds)>();
            Actions = new List<(ReaddressAction, AddressPersistentLocalId)>();

            Readdress();
        }

        private void Readdress()
        {
            foreach (var addressToReaddress in _addressesToReaddress)
            {
                var (sourceAddress, destinationAddress) = GetSourceAndDestinationAddresses(addressToReaddress);
                var destinationAddressPersistentLocalId = destinationAddress?.AddressPersistentLocalId;

                if (destinationAddress is null)
                {
                    destinationAddressPersistentLocalId = new AddressPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());
                    Actions.Add((ReaddressAction.ProposeHouseNumber, destinationAddressPersistentLocalId));
                }

                ReaddressedAddresses.Add(destinationAddressPersistentLocalId!,
                    (
                        new ReaddressedAddressData(
                            addressToReaddress.SourceAddressPersistentLocalId,
                            destinationAddressPersistentLocalId!,
                            isDestinationNewlyProposed: destinationAddress is null,
                            sourceAddress.Status,
                            addressToReaddress.DestinationHouseNumber,
                            boxNumber: null,
                            sourceAddress.PostalCode!,
                            sourceAddress.Geometry,
                            sourceAddress.IsOfficiallyAssigned
                        ),
                        new List<ReaddressedAddressData>(),
                        new List<AddressPersistentLocalId>(),
                        new List<AddressPersistentLocalId>()
                    ));

                ReaddressBoxNumbers(
                    addressToReaddress,
                    sourceAddress,
                    destinationAddress,
                    destinationAddressPersistentLocalId!);

                if (destinationAddress is not null)
                {
                    RejectOrRetireDestinationAddressBoxNumbers(destinationAddress);
                }
            }
        }

        private (StreetNameAddress sourceAddress, StreetNameAddress? destinationAddress) GetSourceAndDestinationAddresses(ReaddressAddressItem addressToReaddress)
        {
            var streetName = addressToReaddress.SourceStreetNamePersistentLocalId == _streetName.PersistentLocalId
                ? _streetName
                : _streetNames
                    .GetAsync(new StreetNameStreamId(addressToReaddress.SourceStreetNamePersistentLocalId))
                    .GetAwaiter().GetResult();

            var sourceAddress = streetName.StreetNameAddresses.GetNotRemovedByPersistentLocalId(addressToReaddress.SourceAddressPersistentLocalId);

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

            var destinationAddress = _streetName.StreetNameAddresses.FindActiveParentByHouseNumber(addressToReaddress.DestinationHouseNumber);

            if (destinationAddress is not null
                && sourceAddress.AddressPersistentLocalId == destinationAddress.AddressPersistentLocalId)
            {
                throw new SourceAndDestinationAddressAreTheSameException(
                    addressToReaddress.SourceAddressPersistentLocalId,
                    addressToReaddress.DestinationHouseNumber);
            }

            return (sourceAddress, destinationAddress);
        }

        private void ReaddressBoxNumbers(
            ReaddressAddressItem addressToReaddress,
            StreetNameAddress sourceAddress,
            StreetNameAddress? destinationAddress,
            AddressPersistentLocalId destinationAddressPersistentLocalId)
        {
            foreach (var sourceBoxNumberAddress in sourceAddress.Children.Where(x => x.IsActive))
            {
                var destinationBoxNumberAddress = destinationAddress?.FindBoxNumberAddress(sourceBoxNumberAddress.BoxNumber!);
                var destinationBoxNumberPersistentLocalId = destinationBoxNumberAddress?.AddressPersistentLocalId;

                if (destinationBoxNumberAddress is null)
                {
                    destinationBoxNumberPersistentLocalId = new AddressPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());
                    Actions.Add((ReaddressAction.ProposeBoxNumber, destinationBoxNumberPersistentLocalId));
                }

                ReaddressedAddresses[destinationAddressPersistentLocalId]
                    .readdressedBoxNumbers.Add(new ReaddressedAddressData(
                        sourceBoxNumberAddress.AddressPersistentLocalId,
                        destinationBoxNumberPersistentLocalId!,
                        isDestinationNewlyProposed: destinationBoxNumberAddress is null,
                        sourceBoxNumberAddress.Status,
                        addressToReaddress.DestinationHouseNumber,
                        sourceBoxNumberAddress.BoxNumber,
                        sourceAddress.PostalCode!,
                        sourceBoxNumberAddress.Geometry,
                        sourceBoxNumberAddress.IsOfficiallyAssigned
                    ));

                // 11   ->  13   ->   15
                // 11A      13A1
                // 11B      13B
                // Given the above,
                // When house number 11 is the current sourceAddress, it is never used as a destination address, so we should keep all of its box numbers.
                // When house number 13 is the current sourceAddress, then ReaddressedAddresses will contain boxNumberAddress 13B and we should keep it.
                if (IsUsedAsDestinationAddress(sourceAddress) && !HasBeenReaddressed(sourceBoxNumberAddress))
                {
                    RejectOrRetireBoxNumberAddress(sourceBoxNumberAddress);
                }
            }
        }

        private void RejectOrRetireDestinationAddressBoxNumbers(StreetNameAddress destinationAddress)
        {
            // 11 -> 13
            // Given house number 11 is the source address and house number 13 is the destination address.
            // When house number 13 is not used as a source address,
            //   then house number 13's box numbers should be rejected or retired ONLY when they previously were NOT the destination address.
            if (IsUsedAsSourceAddress(destinationAddress))
            {
                return;
            }

            foreach (var boxNumberAddress in destinationAddress.Children.Where(x => x.IsActive))
            {
                if (ReaddressedBoxNumbers.Any(x =>
                        x.DestinationAddressPersistentLocalId == boxNumberAddress.AddressPersistentLocalId))
                {
                    continue;
                }

                RejectOrRetireBoxNumberAddress(boxNumberAddress);
            }
        }

        private void RejectOrRetireBoxNumberAddress(StreetNameAddress boxNumberAddress)
        {
            switch (boxNumberAddress.Status)
            {
                case AddressStatus.Proposed:
                    Actions.Add((ReaddressAction.Reject, boxNumberAddress.AddressPersistentLocalId));
                    ReaddressedAddresses[boxNumberAddress.Parent!.AddressPersistentLocalId]
                        .rejectedBoxNumberPersistentLocalIds.Add(boxNumberAddress.AddressPersistentLocalId);
                    break;

                case AddressStatus.Current:
                    Actions.Add((ReaddressAction.Retire, boxNumberAddress.AddressPersistentLocalId));
                    ReaddressedAddresses[boxNumberAddress.Parent!.AddressPersistentLocalId]
                        .retiredBoxNumberPersistentLocalIds.Add(boxNumberAddress.AddressPersistentLocalId);
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
            return ReaddressedBoxNumbers.Any(x => x.DestinationAddressPersistentLocalId == boxNumberAddress.AddressPersistentLocalId);
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
