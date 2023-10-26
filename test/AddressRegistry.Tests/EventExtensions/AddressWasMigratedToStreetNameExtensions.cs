namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressWasMigratedToStreetNameExtensions
    {
        public static AddressWasMigratedToStreetName AsHouseNumberAddress(
            this AddressWasMigratedToStreetName @event,
            HouseNumber? houseNumber = null,
            AddressStatus? addressStatus = null)
        {
            var houseNumberAddressWasMigratedToStreetName = @event
                .WithParentAddressPersistentLocalId(null)
                .WithBoxNumber(null)
                .WithNotRemoved();

            if (addressStatus.HasValue)
            {
                houseNumberAddressWasMigratedToStreetName = houseNumberAddressWasMigratedToStreetName.WithStatus(addressStatus.Value);
            }

            return houseNumber is not null
                ? houseNumberAddressWasMigratedToStreetName.WithHouseNumber(houseNumber)
                : houseNumberAddressWasMigratedToStreetName;
        }

        public static AddressWasMigratedToStreetName AsBoxNumberAddress(
            this AddressWasMigratedToStreetName @event,
            AddressPersistentLocalId houseNumberAddressPersistentLocalId,
            BoxNumber? boxNumber = null)
        {
            var boxNumberToUse = boxNumber ?? (!string.IsNullOrWhiteSpace(@event.BoxNumber) ? new BoxNumber(@event.BoxNumber!) : null);

            return @event
                .WithParentAddressPersistentLocalId(houseNumberAddressPersistentLocalId)
                .WithBoxNumber(boxNumberToUse)
                .WithNotRemoved();
        }

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

        public static AddressWasMigratedToStreetName WithParentAddressPersistentLocalId(
            this AddressWasMigratedToStreetName @event,
            AddressPersistentLocalId? parentAddressPersistentLocalId)
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
                @event.IsRemoved,
                parentAddressPersistentLocalId);
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

        public static AddressWasMigratedToStreetName WithBoxNumber(
            this AddressWasMigratedToStreetName @event,
            BoxNumber? boxNumber)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                new HouseNumber(@event.HouseNumber),
                boxNumber,
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

        public static AddressWasMigratedToStreetName WithPostalCode(
            this AddressWasMigratedToStreetName @event,
            PostalCode postalCode)
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
                postalCode,
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

        public static AddressWasMigratedToStreetName WithAddressGeometry(
            this AddressWasMigratedToStreetName @event,
            AddressGeometry addressGeometry)
        {
            var newEvent = new AddressWasMigratedToStreetName(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressId(@event.AddressId),
                new AddressStreetNameId(@event.StreetNameId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.Status,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                addressGeometry,
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

        public static AddressWasMigratedToStreetName WithOfficiallyAssigned(
            this AddressWasMigratedToStreetName @event,
            bool officiallyAssigned)
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
                officiallyAssigned,
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

        public static AddressWasMigratedToStreetName WithRemoved(
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
                true,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
