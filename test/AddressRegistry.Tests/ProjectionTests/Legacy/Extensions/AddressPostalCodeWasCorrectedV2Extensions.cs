namespace AddressRegistry.Tests.ProjectionTests.Legacy.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class AddressPostalCodeWasCorrectedV2Extensions
    {
        public static AddressPostalCodeWasCorrectedV2 WithAddressPersistentLocalId(
            this AddressPostalCodeWasCorrectedV2 @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressPostalCodeWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.BoxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                new PostalCode(@event.PostalCode));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressPostalCodeWasCorrectedV2 WithBoxNumberPersistentLocalIds(
            this AddressPostalCodeWasCorrectedV2 @event,
            IEnumerable<AddressPersistentLocalId> boxNumberAddressPersistentLocalId)
        {
            var newEvent = new AddressPostalCodeWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                boxNumberAddressPersistentLocalId,
                new PostalCode(@event.PostalCode));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
        public static AddressPostalCodeWasCorrectedV2 WithPostalCode(
            this AddressPostalCodeWasCorrectedV2 @event,
            PostalCode postalCode)
        {
            var newEvent = new AddressPostalCodeWasCorrectedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.BoxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                postalCode);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
