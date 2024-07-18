namespace AddressRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using StreetName;
    using StreetName.Events;

    public static class AddressWasProposedForMunicipalityMergerExtensions
    {
        public static AddressWasProposedForMunicipalityMerger AsHouseNumberAddress(
            this AddressWasProposedForMunicipalityMerger @event, HouseNumber? houseNumber = null)
        {
            var houseNumberAddressWasProposedForMunicipalityMerger = @event
                .WithParentAddressPersistentLocalId(null)
                .WithBoxNumber(null);

            if (houseNumber is not null)
            {
                houseNumberAddressWasProposedForMunicipalityMerger = houseNumberAddressWasProposedForMunicipalityMerger.WithHouseNumber(houseNumber);
            }

            return houseNumberAddressWasProposedForMunicipalityMerger;
        }

        public static AddressWasProposedForMunicipalityMerger AsBoxNumberAddress(
            this AddressWasProposedForMunicipalityMerger @event,
            AddressPersistentLocalId houseNumberAddressPersistentLocalId,
            BoxNumber? boxNumber = null)
        {
            var boxNumberToUse = boxNumber ?? (!string.IsNullOrWhiteSpace(@event.BoxNumber) ? new BoxNumber(@event.BoxNumber!) : null);

            return @event
                .WithParentAddressPersistentLocalId(houseNumberAddressPersistentLocalId)
                .WithBoxNumber(boxNumberToUse);
        }

        public static AddressWasProposedForMunicipalityMerger WithStreetNamePersistentLocalId(
            this AddressWasProposedForMunicipalityMerger @event,
            StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithAddressPersistentLocalId(
            this AddressWasProposedForMunicipalityMerger @event,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithAddressPersistentLocalId(
            this AddressWasProposedForMunicipalityMerger @event,
            int addressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithParentAddressPersistentLocalId(
            this AddressWasProposedForMunicipalityMerger @event,
            AddressPersistentLocalId? parentAddressPersistentLocalId)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithPostalCode(
            this AddressWasProposedForMunicipalityMerger @event,
            PostalCode postalCode)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithHouseNumber(
            this AddressWasProposedForMunicipalityMerger @event,
            HouseNumber houseNumber)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithBoxNumber(
            this AddressWasProposedForMunicipalityMerger @event,
            BoxNumber? boxNumber)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithExtendedWkbGeometry(
            this AddressWasProposedForMunicipalityMerger @event,
            string extendedWkbGeometry)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithGeometryMethod(
            this AddressWasProposedForMunicipalityMerger @event,
            GeometryMethod geometryMethod)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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

        public static AddressWasProposedForMunicipalityMerger WithGeometrySpecification(
            this AddressWasProposedForMunicipalityMerger @event,
            GeometrySpecification geometrySpecification)
        {
            var newEvent = new AddressWasProposedForMunicipalityMerger(
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
