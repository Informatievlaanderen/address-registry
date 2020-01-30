namespace AddressRegistry.Api.Legacy.Address
{
    using System.Collections.Generic;
    using AddressRegistry.Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    public class AddressMapper
    {
        public static VolledigAdres GetVolledigAdres(string houseNumber, string boxNumber, string postalCode, StreetNameLatestItem streetName, MunicipalityLatestItem municipality)
        {
            var defaultMunicipalityName = GetDefaultMunicipalityName(municipality);

            return new VolledigAdres(
                GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage).Value,
                houseNumber,
                boxNumber,
                postalCode,
                defaultMunicipalityName.Value,
                defaultMunicipalityName.Key);
        }

        public static VolledigAdres GetVolledigAdres(string houseNumber, string boxNumber, string postalCode, StreetNameBosaItem streetName, MunicipalityBosaItem municipality)
        {
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
            var geometry = WKBReaderFactory.Create().Read(point);

            return new Point
            {
                XmlPoint = new GmlPoint { Pos = $"{geometry.Coordinate.X.ToGeometryCoordinateValueFormat()} {geometry.Coordinate.Y.ToGeometryCoordinateValueFormat()}" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { geometry.Coordinate.X, geometry.Coordinate.Y } }
            };
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

        public static AddressStatus? ConvertFromAdresStatus(AdresStatus? status)
        {
            switch (status)
            {
                case null:
                    return null;
                case AdresStatus.Voorgesteld:
                    return AddressStatus.Proposed;

                case AdresStatus.Gehistoreerd:
                    return AddressStatus.Retired;

                default:
                case AdresStatus.InGebruik:
                    return AddressStatus.Current;
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

        public static KeyValuePair<Taal, string> GetDefaultMunicipalityName(MunicipalityBosaItem municipality)
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

        public static KeyValuePair<Taal, string> GetDefaultStreetNameName(StreetNameLatestItem streetName, Taal? municipalityLanguage)
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

        public static KeyValuePair<Taal, string>? GetDefaultHomonymAddition(StreetNameLatestItem streetName, Taal? municipalityLanguage)
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

        public static KeyValuePair<Taal, string> GetDefaultStreetNameName(StreetNameBosaItem streetName, Taal? municipalityLanguage)
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
    }
}
