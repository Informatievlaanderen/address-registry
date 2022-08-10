namespace AddressRegistry.Api.Oslo.Address
{
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
        public static VolledigAdres? GetVolledigAdres(string houseNumber, string boxNumber, string postalCode,
            StreetNameLatestItem? streetName, MunicipalityLatestItem? municipality)
        {
            if (streetName == null || municipality == null)
            {
                return null;
            }

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

        public static AddressPosition GetAddressPoint(
            byte[] point,
            AddressRegistry.StreetName.GeometryMethod? method,
            AddressRegistry.StreetName.GeometrySpecification? specification)
        {
            var geometry = WKBReaderFactory.CreateForLegacy().Read(point);
            var gml = GetGml(geometry);
            var positieSpecificatie = ConvertFromGeometrySpecification(specification);
            var positieGeometrieMethode = ConvertFromGeometryMethod(method);
            return new AddressPosition(new GmlJsonPoint(gml), positieGeometrieMethode, positieSpecificatie);
        }

        public static PositieGeometrieMethode ConvertFromGeometryMethod(GeometryMethod? method)
        {
            return method switch
            {
                GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
                GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder,
                _ => PositieGeometrieMethode.AangeduidDoorBeheerder
            };
        }

        public static PositieGeometrieMethode ConvertFromGeometryMethod(AddressRegistry.StreetName.GeometryMethod? method)
        {
            return method switch
            {
                AddressRegistry.StreetName.GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                AddressRegistry.StreetName.GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
                AddressRegistry.StreetName.GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder,
                _ => PositieGeometrieMethode.AangeduidDoorBeheerder
            };
        }

        public static PositieSpecificatie ConvertFromGeometrySpecification(GeometrySpecification? specification)
        {
            return specification switch
            {
                GeometrySpecification.Street => PositieSpecificatie.Straat,
                GeometrySpecification.Parcel => PositieSpecificatie.Perceel,
                GeometrySpecification.Lot => PositieSpecificatie.Lot,
                GeometrySpecification.Stand => PositieSpecificatie.Standplaats,
                GeometrySpecification.Berth => PositieSpecificatie.Ligplaats,
                GeometrySpecification.Building => PositieSpecificatie.Gebouw,
                GeometrySpecification.BuildingUnit => PositieSpecificatie.Gebouweenheid,
                GeometrySpecification.Entry => PositieSpecificatie.Ingang,
                GeometrySpecification.RoadSegment => PositieSpecificatie.Wegsegment,
                GeometrySpecification.Municipality => PositieSpecificatie.Gemeente,
                _ => PositieSpecificatie.Gemeente
            };
        }

        public static PositieSpecificatie ConvertFromGeometrySpecification(AddressRegistry.StreetName.GeometrySpecification? specification)
        {
            return specification switch
            {
                AddressRegistry.StreetName.GeometrySpecification.Street => PositieSpecificatie.Straat,
                AddressRegistry.StreetName.GeometrySpecification.Parcel => PositieSpecificatie.Perceel,
                AddressRegistry.StreetName.GeometrySpecification.Lot => PositieSpecificatie.Lot,
                AddressRegistry.StreetName.GeometrySpecification.Stand => PositieSpecificatie.Standplaats,
                AddressRegistry.StreetName.GeometrySpecification.Berth => PositieSpecificatie.Ligplaats,
                AddressRegistry.StreetName.GeometrySpecification.Building => PositieSpecificatie.Gebouw,
                AddressRegistry.StreetName.GeometrySpecification.BuildingUnit => PositieSpecificatie.Gebouweenheid,
                AddressRegistry.StreetName.GeometrySpecification.Entry => PositieSpecificatie.Ingang,
                AddressRegistry.StreetName.GeometrySpecification.RoadSegment => PositieSpecificatie.Wegsegment,
                AddressRegistry.StreetName.GeometrySpecification.Municipality => PositieSpecificatie.Gemeente,
                _ => PositieSpecificatie.Gemeente
            };
        }

        public static AdresStatus ConvertFromAddressStatus(AddressStatus? status)
        {
            return status switch
            {
                AddressStatus.Proposed => AdresStatus.Voorgesteld,
                AddressStatus.Retired => AdresStatus.Gehistoreerd,
                AddressStatus.Current => AdresStatus.InGebruik,
                AddressStatus.Rejected => AdresStatus.Afgekeurd,
                _ => AdresStatus.InGebruik
            };
        }

        public static AdresStatus ConvertFromAddressStatus(AddressRegistry.StreetName.AddressStatus? status)
        {
            return status switch
            {
                AddressRegistry.StreetName.AddressStatus.Proposed => AdresStatus.Voorgesteld,
                AddressRegistry.StreetName.AddressStatus.Retired => AdresStatus.Gehistoreerd,
                AddressRegistry.StreetName.AddressStatus.Current => AdresStatus.InGebruik,
                AddressRegistry.StreetName.AddressStatus.Rejected => AdresStatus.Afgekeurd,
                _ => AdresStatus.InGebruik
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultMunicipalityName(MunicipalityLatestItem municipality)
        {
            return municipality.PrimaryLanguage switch
            {
                Taal.NL => new KeyValuePair<Taal, string?>(Taal.NL, municipality.NameDutch),
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, municipality.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, municipality.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, municipality.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, municipality.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(StreetNameLatestItem streetName,
            Taal? municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                Taal.NL => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch),
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?>? GetDefaultHomonymAddition(StreetNameLatestItem streetName,
            Taal? municipalityLanguage)
        {
            if (!streetName.HasHomonymAddition)
            {
                return null;
            }

            return municipalityLanguage switch
            {
                Taal.NL => new KeyValuePair<Taal, string?>(Taal.NL, streetName.HomonymAdditionDutch),
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.HomonymAdditionFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.HomonymAdditionGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.HomonymAdditionEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.HomonymAdditionDutch)
            };
        }
    }
}
