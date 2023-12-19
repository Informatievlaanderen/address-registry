namespace AddressRegistry.Projections.Integration.Convertors
{
    using System;
    using System.Text;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using StreetName;

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

        private static string GetGml(Geometry geometry)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };
            using (var xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Point", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
                Write(geometry.Coordinate, xmlwriter);
                xmlwriter.WriteEndElement();
            }

            return builder.ToString();
        }

        private static void Write(Coordinate coordinate, XmlWriter writer)
        {
            writer.WriteStartElement("gml", "pos", "http://www.opengis.net/gml/3.2");
            writer.WriteValue(string.Format(Global.GetNfi(), "{0} {1}", coordinate.X.ToPointGeometryCoordinateValueFormat(),
                coordinate.Y.ToPointGeometryCoordinateValueFormat()));
            writer.WriteEndElement();
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
    }
}
