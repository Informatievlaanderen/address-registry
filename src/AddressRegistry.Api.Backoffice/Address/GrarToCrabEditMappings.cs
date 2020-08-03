namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using CrabEdit.Client.Contexts.Address;

    public static class GrarToCrabEditMappings
    {
        public static AddressStatus Map(this AdresStatus status)
            => Map(StatusMapping, status);

        public static AddressPositionMethod Map(this PositieGeometrieMethode positionMethod)
            => Map(GeometryMethodMapping, positionMethod);

        public static AddressPositionSpecification Map(this PositieSpecificatie positionSpecification)
            => Map(PositionSpecificationMapping, positionSpecification);

        private static TMapped Map<TValue, TMapped>(
            IDictionary<TValue, TMapped> mapping,
            TValue value)
            => mapping.ContainsKey(value)
                ? mapping[value]
                : throw new ArgumentException($"No mapping defined for value {value}");
        
        private static readonly Dictionary<AdresStatus, AddressStatus> StatusMapping =
            new Dictionary<AdresStatus, AddressStatus>
            {
                { AdresStatus.Voorgesteld, AddressStatus.Proposed },
                { AdresStatus.InGebruik, AddressStatus.InUse },
                { AdresStatus.Gehistoreerd, AddressStatus.Retired }
            };

        private static IDictionary<PositieGeometrieMethode, AddressPositionMethod> GeometryMethodMapping
            => new Dictionary<PositieGeometrieMethode, AddressPositionMethod>
            {
                {PositieGeometrieMethode.AangeduidDoorBeheerder, AddressPositionMethod.AppointedByAdministrator  },
                {PositieGeometrieMethode.AfgeleidVanObject, AddressPositionMethod.DerivedFromObject  }
            };

        private static IDictionary<PositieSpecificatie, AddressPositionSpecification> PositionSpecificationMapping
            => new Dictionary<PositieSpecificatie, AddressPositionSpecification>
            {
                { PositieSpecificatie.Ligplaats, AddressPositionSpecification.Berth },
                { PositieSpecificatie.Gebouweenheid, AddressPositionSpecification.BuildingUnit },
                { PositieSpecificatie.Ingang, AddressPositionSpecification.Entry },
                { PositieSpecificatie.Lot, AddressPositionSpecification.Lot },
                { PositieSpecificatie.Perceel, AddressPositionSpecification.Parcel },
                { PositieSpecificatie.Standplaats, AddressPositionSpecification.Stand }
            };
    }
}
