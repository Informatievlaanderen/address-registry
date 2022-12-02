namespace AddressRegistry.Api.BackOffice.Abstractions.Converters
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using StreetName;

    public static class GeometryMethodConverter
    {
        public static GeometryMethod Map(this PositieGeometrieMethode methode)
        {
            return methode switch
            {
                PositieGeometrieMethode.AangeduidDoorBeheerder => GeometryMethod.AppointedByAdministrator,
                PositieGeometrieMethode.AfgeleidVanObject => GeometryMethod.DerivedFromObject,
                _ => throw new ArgumentOutOfRangeException(nameof(methode), methode, null)
            };
        }
    }
}
