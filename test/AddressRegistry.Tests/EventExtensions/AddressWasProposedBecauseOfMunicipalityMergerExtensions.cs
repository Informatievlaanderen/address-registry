namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressWasProposedBecauseOfMunicipalityMergerExtensions
    {
        public static AddressWasProposedBecauseOfMunicipalityMerger AsHouseNumberAddress(
            this AddressWasProposedBecauseOfMunicipalityMerger @event, HouseNumber? houseNumber = null)
        {
            var houseNumberAddressWasProposedBecauseOfMunicipalityMerger = @event
                .WithParentAddressPersistentLocalId(null)
                .WithBoxNumber(null);

            if (houseNumber is not null)
            {
                houseNumberAddressWasProposedBecauseOfMunicipalityMerger = houseNumberAddressWasProposedBecauseOfMunicipalityMerger.WithHouseNumber(houseNumber);
            }

            return houseNumberAddressWasProposedBecauseOfMunicipalityMerger;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger AsBoxNumberAddress(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            AddressPersistentLocalId houseNumberAddressPersistentLocalId,
            BoxNumber? boxNumber = null)
        {
            var boxNumberToUse = boxNumber ?? (!string.IsNullOrWhiteSpace(@event.BoxNumber) ? new BoxNumber(@event.BoxNumber!) : null);

            return @event
                .WithParentAddressPersistentLocalId(houseNumberAddressPersistentLocalId)
                .WithBoxNumber(boxNumberToUse);
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithStreetNamePersistentLocalId(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithAddressPersistentLocalId(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                addressPersistentLocalId,
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithAddressPersistentLocalId(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            int addressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithParentAddressPersistentLocalId(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            AddressPersistentLocalId? parentAddressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                parentAddressPersistentLocalId,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithPostalCode(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            PostalCode postalCode)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                postalCode,
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithHouseNumber(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                houseNumber,
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithBoxNumber(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            BoxNumber? boxNumber)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                boxNumber,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithExtendedWkbGeometry(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            string extendedWkbGeometry)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(extendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithGeometryMethod(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            GeometryMethod geometryMethod)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                geometryMethod,
                @event.GeometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static AddressWasProposedBecauseOfMunicipalityMerger WithGeometrySpecification(
            this AddressWasProposedBecauseOfMunicipalityMerger @event,
            GeometrySpecification geometrySpecification)
        {
            var newEvent = new AddressWasProposedBecauseOfMunicipalityMerger(
                new StreetNamePersistentLocalId(@event.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                @event.ParentPersistentLocalId is not null ? new AddressPersistentLocalId(@event.ParentPersistentLocalId.Value) : null,
                new PostalCode(@event.PostalCode),
                new HouseNumber(@event.HouseNumber),
                @event.BoxNumber is not null ? new BoxNumber(@event.BoxNumber) : null,
                @event.GeometryMethod,
                geometrySpecification,
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.OfficiallyAssigned,
                new AddressPersistentLocalId(@event.MergedAddressPersistentLocalId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
