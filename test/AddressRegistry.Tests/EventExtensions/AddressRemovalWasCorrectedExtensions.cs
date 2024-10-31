namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressRemovalWasCorrectedExtensions
    {
        public static AddressRemovalWasCorrected WithAddressPersistentLocalId(
            this AddressRemovalWasCorrected @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressRemovalWasCorrected(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.Status,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressRemovalWasCorrected WithStatus(
            this AddressRemovalWasCorrected @event,
            AddressStatus status)
        {
            var newEvent = new AddressRemovalWasCorrected(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                status,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressRemovalWasCorrected WithHouseNumber(
            this AddressRemovalWasCorrected @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressRemovalWasCorrected(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                houseNumber,
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressRemovalWasCorrected WithBoxNumber(
            this AddressRemovalWasCorrected @event,
            BoxNumber? boxNumber)
        {
            var newEvent = new AddressRemovalWasCorrected(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                new HouseNumber(@event.HouseNumber),
                boxNumber,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressRemovalWasCorrected WithGeometry(
            this AddressRemovalWasCorrected @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var newEvent = new AddressRemovalWasCorrected(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                extendedWkbGeometry,
                @event.OfficiallyAssigned,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
