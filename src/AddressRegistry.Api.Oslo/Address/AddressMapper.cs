namespace AddressRegistry.Api.Oslo.Address
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using Projections.Elastic.AddressSearch;
    using Projections.Legacy.AddressListV2;
    using StreetName;
    using AddressStatus = AddressRegistry.Address.AddressStatus;
    using MunicipalityLanguage = Consumer.Read.Municipality.Projections.MunicipalityLanguage;
    using Point = Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools.Point;

    public static class AddressMapper
    {
        public static VolledigAdres? GetVolledigAdres(AddressSearchDocument addressSearchDocument)
        {
            if (string.IsNullOrEmpty(addressSearchDocument.Municipality.NisCode))
            {
                return null;
            }

            var defaultMunicipalityName = addressSearchDocument.Municipality.Names.FirstOrDefault(x => x.Language == Language.nl);
            if(defaultMunicipalityName == null)
                defaultMunicipalityName = addressSearchDocument.Municipality.Names.First();
            return new VolledigAdres(
                addressSearchDocument.StreetName.Names.FirstOrDefault(x => x.Language == defaultMunicipalityName.Language)?.Spelling ?? addressSearchDocument.StreetName.Names.First().Spelling,
                addressSearchDocument.HouseNumber,
                addressSearchDocument.BoxNumber,
                addressSearchDocument.PostalInfo?.PostalCode,
                defaultMunicipalityName.Spelling,
                MapElasticLanguageToTaal(defaultMunicipalityName.Language));
        }

        private static Taal MapElasticLanguageToTaal(Language language)
        {
            return language switch
            {
                Language.nl => Taal.NL,
                Language.fr => Taal.FR,
                Language.de => Taal.DE,
                Language.en => Taal.EN,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };
        }

        public static VolledigAdres? GetVolledigAdres(AddressListViewItemV2 addressListViewItem)
        {
            if (string.IsNullOrEmpty(addressListViewItem.NisCode))
            {
                return null;
            }

            var defaultMunicipalityName = addressListViewItem.DefaultMunicipalityName;
            return new VolledigAdres(
                addressListViewItem.DefaultStreetNameName.Value,
                addressListViewItem.HouseNumber,
                addressListViewItem.BoxNumber,
                addressListViewItem.PostalCode,
                defaultMunicipalityName.Value,
                defaultMunicipalityName.Key);
        }

        public static VolledigAdres? GetVolledigAdres(string houseNumber, string boxNumber, string postalCode,
            StreetNameLatestItem streetName, MunicipalityLatestItem? municipality)
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

        public static Point GetAddressPoint(byte[] point)
        {
            var geometry = WKBReaderFactory.CreateForLegacy().Read(point);

            return new Point
            {
                XmlPoint = new GmlPoint { Pos = $"{geometry.Coordinate.X.ToPointGeometryCoordinateValueFormat()} {geometry.Coordinate.Y.ToPointGeometryCoordinateValueFormat()}" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { geometry.Coordinate.X, geometry.Coordinate.Y } }
            };
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

        public static AddressPosition GetAddressPoint(
            byte[] point,
            GeometryMethod? method,
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
            return method switch
            {
                GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
                GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder,
                _ => PositieGeometrieMethode.AangeduidDoorBeheerder
            };
        }

        public static PositieSpecificatie ConvertFromGeometrySpecification(AddressRegistry.Address.GeometrySpecification? specification)
        {
            return specification switch
            {
                AddressRegistry.Address.GeometrySpecification.Street => PositieSpecificatie.Straat,
                AddressRegistry.Address.GeometrySpecification.Parcel => PositieSpecificatie.Perceel,
                AddressRegistry.Address.GeometrySpecification.Lot => PositieSpecificatie.Lot,
                AddressRegistry.Address.GeometrySpecification.Stand => PositieSpecificatie.Standplaats,
                AddressRegistry.Address.GeometrySpecification.Berth => PositieSpecificatie.Ligplaats,
                AddressRegistry.Address.GeometrySpecification.Building => PositieSpecificatie.Gebouw,
                AddressRegistry.Address.GeometrySpecification.BuildingUnit => PositieSpecificatie.Gebouweenheid,
                AddressRegistry.Address.GeometrySpecification.Entry => PositieSpecificatie.Ingang,
                AddressRegistry.Address.GeometrySpecification.RoadSegment => PositieSpecificatie.Wegsegment,
                AddressRegistry.Address.GeometrySpecification.Municipality => PositieSpecificatie.Gemeente,
                _ => PositieSpecificatie.Gemeente
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
                MunicipalityLanguage.French => new KeyValuePair<Taal, string?>(Taal.FR, municipality.NameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string?>(Taal.DE, municipality.NameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string?>(Taal.EN, municipality.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, municipality.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(
            StreetNameLatestItem streetName,
            MunicipalityLanguage? municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                MunicipalityLanguage.French => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?>? GetDefaultHomonymAddition(
            StreetNameLatestItem streetName,
            MunicipalityLanguage? municipalityLanguage)
        {
            if (!streetName.HasHomonymAddition)
            {
                return null;
            }

            return municipalityLanguage switch
            {
                MunicipalityLanguage.Dutch => new KeyValuePair<Taal, string?>(Taal.NL, streetName.HomonymAdditionDutch),
                MunicipalityLanguage.French => new KeyValuePair<Taal, string?>(Taal.FR, streetName.HomonymAdditionFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string?>(Taal.DE, streetName.HomonymAdditionGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string?>(Taal.EN, streetName.HomonymAdditionEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.HomonymAdditionDutch)
            };
        }
    }
}
