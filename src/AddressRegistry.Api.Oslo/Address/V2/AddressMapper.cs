namespace AddressRegistry.Api.Oslo.Address.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Utilities;
    using Projections.Elastic.AddressList;
    using Projections.Elastic.AddressSearch;
    using StreetName;
    using AddressStatus = AddressRegistry.Address.AddressStatus;
    using MunicipalityLanguage = Consumer.Read.Municipality.Projections.MunicipalityLanguage;
    using Point = Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools.Point;
    using SystemReferenceId = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.SystemReferenceId;

    public static class AddressMapper
    {
        /// <summary>
        /// Positions are persisted at centimetre precision, which is also what the Lambert transform is
        /// accurate to. See ADR 0004.
        /// </summary>
        private const int PositionCoordinateDecimals = 2;

        public static VolledigAdres? GetVolledigAdres(AddressListDocument addressListDocument)
        {
            if (string.IsNullOrEmpty(addressListDocument.Municipality.NisCode))
            {
                return null;
            }

            var defaultMunicipalityName = addressListDocument.Municipality.Names.FirstOrDefault(x => x.Language == Language.nl);
            if(defaultMunicipalityName == null)
                defaultMunicipalityName = addressListDocument.Municipality.Names.First();
            return new VolledigAdres(
                addressListDocument.StreetName.Names.FirstOrDefault(x => x.Language == defaultMunicipalityName.Language)?.Spelling ?? addressListDocument.StreetName.Names.First().Spelling,
                addressListDocument.HouseNumber,
                addressListDocument.BoxNumber,
                addressListDocument.PostalInfo?.PostalCode,
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
            => GetAddressPoint(point, SystemReferenceId.SridLambert72);

        /// <summary>
        /// The syndication feed's object is the one version 2 response whose reference system the caller
        /// chooses, through <c>objectCrs</c>. Every other version 2 consumer keeps calling the overload above
        /// and stays pinned to Lambert 72. See ADR 0004.
        /// </summary>
        public static Point GetAddressPoint(byte[] point, int srid)
        {
            var geometry = ReadPosition(point, srid);

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
                // Fixed on purpose: everything reaching here went through ReadPositionAsLambert72.
                // The https scheme is part of the version 2 contract, unlike ConvertToGml's http one.
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
            var geometry = ReadPositionAsLambert72(point);
            var gml = GetGml(geometry);
            var positieSpecificatie = ConvertFromGeometrySpecification(specification);
            var positieGeometrieMethode = ConvertFromGeometryMethod(method);
            return new AddressPosition(new GmlJsonPoint(gml), positieGeometrieMethode, positieSpecificatie);
        }

        /// <summary>
        /// Reads a persisted position in whatever reference system it was stored in and returns it in
        /// Lambert 72, which is the reference system every version 2 response answers in except the
        /// syndication object. See ADR 0004.
        /// </summary>
        private static Geometry ReadPositionAsLambert72(byte[] point)
            => ReadPosition(point, SystemReferenceId.SridLambert72);

        /// <summary>
        /// Reads a persisted position in whatever reference system it was stored in and returns it in
        /// <paramref name="srid"/>. Only a position that has to move is transformed: one already in the
        /// requested system is returned untouched and therefore unrounded, so today's output does not change.
        /// </summary>
        private static Geometry ReadPosition(byte[] point, int srid)
        {
            var geometry = WKBReaderFactory.CreateForEwkb(point).Read(point);

            // A transformed position carries floating point noise far below the centimetre the transform
            // is accurate to; rounding it away keeps an 08 -> 72 position identical to how the same
            // position reads while the event store still holds Lambert 72.
            if (srid == SystemReferenceId.SridLambert2008)
            {
                return geometry.IsLambert08()
                    ? geometry
                    : geometry.EnsureLambert08(PositionCoordinateDecimals);
            }

            return geometry.IsLambert72()
                ? geometry
                : geometry.EnsureLambert72().RoundCoordinates(PositionCoordinateDecimals);
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
