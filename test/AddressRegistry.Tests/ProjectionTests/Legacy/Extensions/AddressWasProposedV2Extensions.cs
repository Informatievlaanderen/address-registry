namespace AddressRegistry.Tests.ProjectionTests.Legacy.Extensions
{
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class AddressWasProposedV2Extensions
    {
        public static AddressWasProposedV2 WithStreetNamePersistentLocalId(
            this AddressWasProposedV2 @event,
            StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var newEvent = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
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

        public static AddressWasProposedV2 WithAddressPersistentLocalId(
            this AddressWasProposedV2 @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
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

        public static AddressWasProposedV2 WithParentAddressPersistentLocalId(
            this AddressWasProposedV2 @event,
            AddressPersistentLocalId? parentAddressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
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

        public static AddressWasProposedV2 WithPostalCode(
            this AddressWasProposedV2 @event,
            PostalCode postalCode)
        {
            var newEvent = new AddressWasProposedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                postalCode,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedV2 WithHouseNumber(
            this AddressWasProposedV2 @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressWasProposedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
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

        public static AddressWasProposedV2 WithBoxNumber(
            this AddressWasProposedV2 @event,
            BoxNumber? boxNumber)
        {
            var newEvent = new AddressWasProposedV2(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
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
