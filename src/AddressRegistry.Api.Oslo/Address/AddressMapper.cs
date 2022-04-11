namespace AddressRegistry.Api.Oslo.Address
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using AddressRegistry.Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using NetTopologySuite.Geometries;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;
    using Responses;

    public static class AddressMapper
    {
        public static VolledigAdres GetVolledigAdres(string houseNumber, string boxNumber, string postalCode,
            StreetNameLatestItem streetName, MunicipalityLatestItem municipality)
        {
            if (streetName == null || municipality == null)
                return null;

            var defaultMunicipalityName = GetDefaultMunicipalityName(municipality);
            return new VolledigAdres(
                GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage).Value,
                houseNumber,
                boxNumber,
                postalCode,
                defaultMunicipalityName.Value,
                defaultMunicipalityName.Key);
        }

        private static string GetGml(Geometry geometry)
        {
            StringBuilder builder = new();
            XmlWriterSettings settings = new() { Indent = false, OmitXmlDeclaration = true };
            using (XmlWriter xmlwriter = XmlWriter.Create(builder, settings))
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
            writer.WriteValue(string.Format(NetTopologySuite.Utilities.Global.GetNfi(), "{0} {1}", coordinate.X.ToPointGeometryCoordinateValueFormat(),
                coordinate.Y.ToPointGeometryCoordinateValueFormat()));
            writer.WriteEndElement();
        }

        public static AddressPosition GetAddressPoint(byte[] point, GeometryMethod? method,
            GeometrySpecification? specification)
        {
            var geometry = WKBReaderFactory.CreateForLegacy().Read(point);
            var gml = GetGml(geometry);
            var positieSpecificatie = ConvertFromGeometrySpecification(specification);
            var positieGeometrieMethode = ConvertFromGeometryMethod(method);
            return new AddressPosition(new GmlJsonPoint(gml), positieGeometrieMethode, positieSpecificatie);
        }

        public static PositieGeometrieMethode ConvertFromGeometryMethod(GeometryMethod? method)
        {
            switch (method)
            {
                case GeometryMethod.DerivedFromObject:
                    return PositieGeometrieMethode.AfgeleidVanObject;

                case GeometryMethod.Interpolated:
                    return PositieGeometrieMethode.Geinterpoleerd;

                default:
                case GeometryMethod.AppointedByAdministrator:
                    return PositieGeometrieMethode.AangeduidDoorBeheerder;
            }
        }

        public static PositieSpecificatie ConvertFromGeometrySpecification(GeometrySpecification? specification)
        {
            switch (specification)
            {
                case GeometrySpecification.Street:
                    return PositieSpecificatie.Straat;

                case GeometrySpecification.Parcel:
                    return PositieSpecificatie.Perceel;

                case GeometrySpecification.Lot:
                    return PositieSpecificatie.Lot;

                case GeometrySpecification.Stand:
                    return PositieSpecificatie.Standplaats;

                case GeometrySpecification.Berth:
                    return PositieSpecificatie.Ligplaats;

                case GeometrySpecification.Building:
                    return PositieSpecificatie.Gebouw;

                case GeometrySpecification.BuildingUnit:
                    return PositieSpecificatie.Gebouweenheid;

                case GeometrySpecification.Entry:
                    return PositieSpecificatie.Ingang;

                case GeometrySpecification.RoadSegment:
                    return PositieSpecificatie.Wegsegment;

                default:
                case GeometrySpecification.Municipality:
                    return PositieSpecificatie.Gemeente;
            }
        }

        public static AdresStatus ConvertFromAddressStatus(AddressStatus? status)
        {
            switch (status)
            {
                case AddressStatus.Proposed:
                    return AdresStatus.Voorgesteld;

                case AddressStatus.Retired:
                    return AdresStatus.Gehistoreerd;

                default:
                case AddressStatus.Current:
                    return AdresStatus.InGebruik;
            }
        }

        public static KeyValuePair<Taal, string> GetDefaultMunicipalityName(MunicipalityLatestItem municipality)
        {
            switch (municipality.PrimaryLanguage)
            {
                default:
                case Taal.NL:
                    return new KeyValuePair<Taal, string>(Taal.NL, municipality.NameDutch);

                case Taal.FR:
                    return new KeyValuePair<Taal, string>(Taal.FR, municipality.NameFrench);

                case Taal.DE:
                    return new KeyValuePair<Taal, string>(Taal.DE, municipality.NameGerman);

                case Taal.EN:
                    return new KeyValuePair<Taal, string>(Taal.EN, municipality.NameEnglish);
            }
        }

        public static KeyValuePair<Taal, string> GetDefaultStreetNameName(StreetNameLatestItem streetName,
            Taal? municipalityLanguage)
        {
            switch (municipalityLanguage)
            {
                default:
                case Taal.NL:
                    return new KeyValuePair<Taal, string>(Taal.NL, streetName.NameDutch);

                case Taal.FR:
                    return new KeyValuePair<Taal, string>(Taal.FR, streetName.NameFrench);

                case Taal.DE:
                    return new KeyValuePair<Taal, string>(Taal.DE, streetName.NameGerman);

                case Taal.EN:
                    return new KeyValuePair<Taal, string>(Taal.EN, streetName.NameEnglish);
            }
        }

        public static KeyValuePair<Taal, string>? GetDefaultHomonymAddition(StreetNameLatestItem streetName,
            Taal? municipalityLanguage)
        {
            if (!streetName.HasHomonymAddition)
                return null;

            switch (municipalityLanguage)
            {
                default:
                case Taal.NL:
                    return new KeyValuePair<Taal, string>(Taal.NL, streetName.HomonymAdditionDutch);

                case Taal.FR:
                    return new KeyValuePair<Taal, string>(Taal.FR, streetName.HomonymAdditionFrench);

                case Taal.DE:
                    return new KeyValuePair<Taal, string>(Taal.DE, streetName.HomonymAdditionGerman);

                case Taal.EN:
                    return new KeyValuePair<Taal, string>(Taal.EN, streetName.HomonymAdditionEnglish);
            }
        }
    }
}
