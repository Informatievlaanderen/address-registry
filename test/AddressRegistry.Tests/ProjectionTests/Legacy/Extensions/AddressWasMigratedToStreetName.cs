namespace AddressRegistry.Tests.ProjectionTests.Legacy.Extensions
{
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class AddressWasMigratedToStreetNameExtensions
    {
        public static AddressWasMigratedToStreetName WithStreetNamePersistentLocalId(
            this AddressWasMigratedToStreetName @event,
            StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                new AddressGeometry(
                    @event.GeometryMethod,
                    @event.GeometrySpecification,
                    new ExtendedWkbGeometry(@event.ExtendedWkbGeometry)),
                @event.OfficiallyAssigned,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                @event.IsCompleted,
                @event.IsRemoved,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasMigratedToStreetName WithAddressPersistentLocalId(
            this AddressWasMigratedToStreetName @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                addressPersistentLocalId,
                @event.Status,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                new AddressGeometry(
                    @event.GeometryMethod,
                    @event.GeometrySpecification,
                    new ExtendedWkbGeometry(@event.ExtendedWkbGeometry)),
                @event.OfficiallyAssigned,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                @event.IsCompleted,
                @event.IsRemoved,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasMigratedToStreetName WithHouseNumber(
            this AddressWasMigratedToStreetName @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                houseNumber,
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                new AddressGeometry(
                    @event.GeometryMethod,
                    @event.GeometrySpecification,
                    new ExtendedWkbGeometry(@event.ExtendedWkbGeometry)),
                @event.OfficiallyAssigned,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                @event.IsCompleted,
                @event.IsRemoved,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasMigratedToStreetName WithPosition(
            this AddressWasMigratedToStreetName @event,
            ExtendedWkbGeometry position)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                new AddressGeometry(
                    @event.GeometryMethod,
                    @event.GeometrySpecification,
                    position),
                @event.OfficiallyAssigned,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                @event.IsCompleted,
                @event.IsRemoved,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasMigratedToStreetName WithStatus(
            this AddressWasMigratedToStreetName @event,
            AddressStatus status)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                status,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                new AddressGeometry(
                    @event.GeometryMethod,
                    @event.GeometrySpecification,
                    new ExtendedWkbGeometry(@event.ExtendedWkbGeometry)),
                @event.OfficiallyAssigned,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                @event.IsCompleted,
                @event.IsRemoved,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasMigratedToStreetName WithNotRemoved(
            this AddressWasMigratedToStreetName @event)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                new AddressGeometry(
                    @event.GeometryMethod,
                    @event.GeometrySpecification,
                    new ExtendedWkbGeometry(@event.ExtendedWkbGeometry)),
                @event.OfficiallyAssigned,
                @event.PostalCode is not null ? new PostalCode(@event.PostalCode) : null,
                @event.IsCompleted,
                false,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
