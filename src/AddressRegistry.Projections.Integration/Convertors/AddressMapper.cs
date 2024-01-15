namespace AddressRegistry.Projections.Integration.Convertors
{
    using System;
    using System.Text;
    using System.Xml;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;

    public static class AddressMapper
    {
        public static string Map(this AddressStatus status)
        {
            switch (status)
            {
                case AddressStatus.Proposed: return AdresStatus.Voorgesteld.ToString();
                case AddressStatus.Current: return AdresStatus.InGebruik.ToString();
                case AddressStatus.Retired: return AdresStatus.Gehistoreerd.ToString();
                case AddressStatus.Rejected: return AdresStatus.Afgekeurd.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static string ToPositieGeometrieMethode(this GeometryMethod method)
        {
            return method switch
            {
                GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject.ToString(),
                GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd.ToString(),
                GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder.ToString(),
                _ => PositieGeometrieMethode.AangeduidDoorBeheerder.ToString()
            };
        }

        public static string ToPositieGeometrieMethode(this Address.GeometryMethod method)
        {
            return method switch
            {
                Address.GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject.ToString(),
                Address.GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd.ToString(),
                Address.GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder.ToString(),
                _ => PositieGeometrieMethode.AangeduidDoorBeheerder.ToString()
            };
        }

        public static GeometryMethod ToGeometryMethod(this Address.GeometryMethod method)
        {
            return method switch
            {
                Address.GeometryMethod.DerivedFromObject => GeometryMethod.DerivedFromObject,
                Address.GeometryMethod.Interpolated => GeometryMethod.Interpolated,
                Address.GeometryMethod.AppointedByAdministrator => GeometryMethod.AppointedByAdministrator,
                _ => GeometryMethod.AppointedByAdministrator
            };
        }

        public static string ToPositieSpecificatie(this GeometrySpecification specification)
        {
            return specification switch
            {
                GeometrySpecification.Street => PositieSpecificatie.Straat.ToString(),
                GeometrySpecification.Parcel => PositieSpecificatie.Perceel.ToString(),
                GeometrySpecification.Lot => PositieSpecificatie.Lot.ToString(),
                GeometrySpecification.Stand => PositieSpecificatie.Standplaats.ToString(),
                GeometrySpecification.Berth => PositieSpecificatie.Ligplaats.ToString(),
                GeometrySpecification.Building => PositieSpecificatie.Gebouw.ToString(),
                GeometrySpecification.BuildingUnit => PositieSpecificatie.Gebouweenheid.ToString(),
                GeometrySpecification.Entry => PositieSpecificatie.Ingang.ToString(),
                GeometrySpecification.RoadSegment => PositieSpecificatie.Wegsegment.ToString(),
                GeometrySpecification.Municipality => PositieSpecificatie.Gemeente.ToString(),
                _ => PositieSpecificatie.Gemeente.ToString()
            };
        }

        public static string ToPositieSpecificatie(this Address.GeometrySpecification specification)
        {
            return specification switch
            {
                Address.GeometrySpecification.Street => PositieSpecificatie.Straat.ToString(),
                Address.GeometrySpecification.Parcel => PositieSpecificatie.Perceel.ToString(),
                Address.GeometrySpecification.Lot => PositieSpecificatie.Lot.ToString(),
                Address.GeometrySpecification.Stand => PositieSpecificatie.Standplaats.ToString(),
                Address.GeometrySpecification.Berth => PositieSpecificatie.Ligplaats.ToString(),
                Address.GeometrySpecification.Building => PositieSpecificatie.Gebouw.ToString(),
                Address.GeometrySpecification.BuildingUnit => PositieSpecificatie.Gebouweenheid.ToString(),
                Address.GeometrySpecification.Entry => PositieSpecificatie.Ingang.ToString(),
                Address.GeometrySpecification.RoadSegment => PositieSpecificatie.Wegsegment.ToString(),
                Address.GeometrySpecification.Municipality => PositieSpecificatie.Gemeente.ToString(),
                _ => PositieSpecificatie.Gemeente.ToString()
            };
        }

        public static GeometrySpecification ToGeometrySpecification(this Address.GeometrySpecification specification)
        {
            return specification switch
            {
                Address.GeometrySpecification.Street => GeometrySpecification.Street,
                Address.GeometrySpecification.Parcel => GeometrySpecification.Parcel,
                Address.GeometrySpecification.Lot => GeometrySpecification.Lot,
                Address.GeometrySpecification.Stand => GeometrySpecification.Stand,
                Address.GeometrySpecification.Berth => GeometrySpecification.Berth,
                Address.GeometrySpecification.Building => GeometrySpecification.Building,
                Address.GeometrySpecification.BuildingUnit => GeometrySpecification.BuildingUnit,
                Address.GeometrySpecification.Entry => GeometrySpecification.Entry,
                Address.GeometrySpecification.RoadSegment => GeometrySpecification.RoadSegment,
                Address.GeometrySpecification.Municipality => GeometrySpecification.Municipality,
                _ => GeometrySpecification.Municipality
            };
        }
    }
}
