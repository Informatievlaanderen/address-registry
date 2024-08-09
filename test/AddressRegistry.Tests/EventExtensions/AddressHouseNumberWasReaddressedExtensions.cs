namespace AddressRegistry.Tests.EventExtensions
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;

    public static class AddressHouseNumberWasReaddressedExtensions
    {
        public static AddressHouseNumberWasReaddressed WithExtendedWkbGeometry(
            this AddressHouseNumberWasReaddressed @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var houseNumberAddress = new ReaddressedAddressData(
                    new AddressPersistentLocalId(@event.ReaddressedHouseNumber.SourceAddressPersistentLocalId),
                    new AddressPersistentLocalId(@event.ReaddressedHouseNumber.DestinationAddressPersistentLocalId),
                    @event.ReaddressedHouseNumber.IsDestinationNewlyProposed,
                    @event.ReaddressedHouseNumber.SourceStatus,
                    new HouseNumber(@event.ReaddressedHouseNumber.DestinationHouseNumber),
                    null,
                    new PostalCode(@event.ReaddressedHouseNumber.SourcePostalCode),
                    new AddressGeometry(
                        @event.ReaddressedHouseNumber.SourceGeometryMethod,
                        @event.ReaddressedHouseNumber.SourceGeometrySpecification,
                        extendedWkbGeometry),
                    @event.ReaddressedHouseNumber.SourceIsOfficiallyAssigned);

            var boxNumberAddresses = @event.ReaddressedBoxNumbers.Select(x => new ReaddressedAddressData(
                new AddressPersistentLocalId(x.SourceAddressPersistentLocalId),
                new AddressPersistentLocalId(x.DestinationAddressPersistentLocalId),
                x.IsDestinationNewlyProposed,
                x.SourceStatus,
                new HouseNumber(x.DestinationHouseNumber),
                new BoxNumber(x.SourceBoxNumber!),
                new PostalCode(x.SourcePostalCode),
                new AddressGeometry(
                    x.SourceGeometryMethod,
                    x.SourceGeometrySpecification,
                    extendedWkbGeometry),
                x.SourceIsOfficiallyAssigned
            ));

            var newEvent = new AddressHouseNumberWasReaddressed(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                houseNumberAddress,
                []);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
