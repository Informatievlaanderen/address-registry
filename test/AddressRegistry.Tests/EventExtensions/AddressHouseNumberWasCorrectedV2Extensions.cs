namespace AddressRegistry.Tests.EventExtensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressHouseNumberWasCorrectedV2Extensions
    {
        public static AddressHouseNumberWasCorrectedV2 WithAddressPersistentLocalId(
            this AddressHouseNumberWasCorrectedV2 @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressHouseNumberWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.BoxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                new HouseNumber(@event.HouseNumber));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressHouseNumberWasCorrectedV2 WithBoxNumberPersistentLocalIds(
            this AddressHouseNumberWasCorrectedV2 @event,
            IEnumerable<AddressPersistentLocalId> boxNumberAddressPersistentLocalId)
        {
            var newEvent = new AddressHouseNumberWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                boxNumberAddressPersistentLocalId,
                new HouseNumber(@event.HouseNumber));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
        public static AddressHouseNumberWasCorrectedV2 WithHouseNumber(
            this AddressHouseNumberWasCorrectedV2 @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressHouseNumberWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.BoxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                houseNumber);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
