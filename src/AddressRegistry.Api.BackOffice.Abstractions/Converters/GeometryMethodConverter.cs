namespace AddressRegistry.Api.BackOffice.Abstractions.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using StreetName;

    public static class GeometryMethodConverter
    {
        public static GeometryMethod Map(this PositieGeometrieMethode? methode)
        {
            if (methode is null)
            {
                return GeometryMethod.DerivedFromObject;
            }

            switch (methode)
            {
                case PositieGeometrieMethode.AangeduidDoorBeheerder:
                    return GeometryMethod.AppointedByAdministrator;
                case PositieGeometrieMethode.AfgeleidVanObject:
                    return GeometryMethod.DerivedFromObject;
                default:
                    throw new ArgumentOutOfRangeException(nameof(methode), methode, null);
            }
        }
    }
}
