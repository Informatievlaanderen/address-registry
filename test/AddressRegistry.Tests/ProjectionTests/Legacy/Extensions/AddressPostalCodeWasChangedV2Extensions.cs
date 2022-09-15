namespace AddressRegistry.Tests.ProjectionTests.Legacy.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class AddressPostalCodeWasChangedVExtensions
    {
        public static AddressPostalCodeWasChangedV2 WithAddressPersistentLocalId(
            this AddressPostalCodeWasChangedV2 @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressPostalCodeWasChangedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.BoxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                new PostalCode(@event.PostalCode));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressPostalCodeWasChangedV2 WithBoxNumberPersistentLocalIds(
            this AddressPostalCodeWasChangedV2 @event,
            IEnumerable<AddressPersistentLocalId> boxNumberAddressPersistentLocalId)
        {
            var newEvent = new AddressPostalCodeWasChangedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                boxNumberAddressPersistentLocalId,
                new PostalCode(@event.PostalCode));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
        public static AddressPostalCodeWasChangedV2 WithPostalCode(
            this AddressPostalCodeWasChangedV2 @event,
            PostalCode postalCode)
        {
            var newEvent = new AddressPostalCodeWasChangedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.BoxNumberPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                postalCode);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
