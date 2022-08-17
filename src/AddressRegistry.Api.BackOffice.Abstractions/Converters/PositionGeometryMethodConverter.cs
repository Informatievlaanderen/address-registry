namespace AddressRegistry.Api.BackOffice.Abstractions.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using StreetName;

    public static class PositionGeometryMethodConverter
    {
        public static PositionGeometryMethod Map(this PositieGeometrieMethode? methode)
        {
            if (methode is null)
            {
                return PositionGeometryMethod.DerivedFromObject;
            }

            switch (methode)
            {
                case PositieGeometrieMethode.AangeduidDoorBeheerder:
                    return PositionGeometryMethod.AppointedByAdministrator;
                case PositieGeometrieMethode.AfgeleidVanObject:
                    return PositionGeometryMethod.DerivedFromObject;
                default:
                    throw new ArgumentOutOfRangeException(nameof(methode), methode, null);
            }
        }
    }
}
