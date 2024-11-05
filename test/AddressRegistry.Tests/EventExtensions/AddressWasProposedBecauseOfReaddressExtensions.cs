namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressWasProposedBecauseOfReaddressExtensions
    {
        public static AddressWasProposedBecauseOfReaddress WithAddressPersistentLocalId(
            this AddressWasProposedBecauseOfReaddress @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedBecauseOfReaddress(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                new AddressPersistentLocalId(@event.SourceAddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
        public static AddressWasProposedBecauseOfReaddress WithParentAddressPersistentLocalId(
            this AddressWasProposedBecauseOfReaddress @event,
            AddressPersistentLocalId? parentAddressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedBecauseOfReaddress(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                new AddressPersistentLocalId(@event.SourceAddressPersistentLocalId),
                parentAddressPersistentLocalId,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfReaddress WithHouseNumber(
            this AddressWasProposedBecauseOfReaddress @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressWasProposedBecauseOfReaddress(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                new AddressPersistentLocalId(@event.SourceAddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                houseNumber,
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfReaddress WithExtendedWkbGeometry(
            this AddressWasProposedBecauseOfReaddress @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var newEvent = new AddressWasProposedBecauseOfReaddress(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                new AddressPersistentLocalId(@event.SourceAddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                extendedWkbGeometry);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfReaddress WithBoxNumber(
            this AddressWasProposedBecauseOfReaddress @event,
            BoxNumber? boxNumber)
        {
            var newEvent = new AddressWasProposedBecauseOfReaddress(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                new AddressPersistentLocalId(@event.SourceAddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                boxNumber,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
