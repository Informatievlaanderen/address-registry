namespace AddressRegistry.Api.BackOffice.Abstractions.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using StreetName;

    public static class GeometrySpecificationConverter
    {
        public static GeometrySpecification Map(this PositieSpecificatie specificatie)
        {
            return specificatie switch
            {
                PositieSpecificatie.Perceel => GeometrySpecification.Parcel,
                PositieSpecificatie.Lot => GeometrySpecification.Lot,
                PositieSpecificatie.Standplaats => GeometrySpecification.Stand,
                PositieSpecificatie.Ligplaats => GeometrySpecification.Berth,
                PositieSpecificatie.Ingang => GeometrySpecification.Entry,
                PositieSpecificatie.Gebouweenheid => GeometrySpecification.BuildingUnit,
                _ => throw new ArgumentOutOfRangeException(nameof(specificatie), specificatie, null)
            };
        }
    }
}
