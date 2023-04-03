namespace AddressRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Address;
    using Commands;
    using Events;

    public partial class StreetName
    {
        public void Readdress(
            IStreetNames streetNames,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IEnumerable<ReaddressAddressItem> readdressItems,
            ReaddressExecutionContext executionContext)
        {
            GuardActiveStreetName();

            var streetNameReaddresser = new StreetNameReaddresser(streetNames, persistentLocalIdGenerator, readdressItems, this);

            foreach (var item in streetNameReaddresser.ReaddressedAddresses.Where(x => x.IsHouseNumberAddress))
            {
                executionContext.AddressesUpdated.Add((PersistentLocalId, new AddressPersistentLocalId(item.DestinationAddressPersistentLocalId)));
            }

            foreach (var (action, addressPersistentLocalId) in streetNameReaddresser.Actions)
            {
                switch (action)
                {
                    case ReaddressAction.ProposeHouseNumber:
                        var addressData = streetNameReaddresser.ReaddressedAddresses.Single(x =>
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
                        addressData = streetNameReaddresser.ReaddressedAddresses.Single(x =>
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

            ApplyChange(new StreetNameWasReaddressed(
                PersistentLocalId,
                streetNameReaddresser.GetAddressesByActions(ReaddressAction.ProposeHouseNumber, ReaddressAction.ProposeBoxNumber).ToList(),
                streetNameReaddresser.GetAddressesByActions(ReaddressAction.Reject).ToList(),
                streetNameReaddresser.GetAddressesByActions(ReaddressAction.Retire).ToList(),
                streetNameReaddresser.ReaddressedAddresses));
        }
    }
}
