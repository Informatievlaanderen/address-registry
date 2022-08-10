namespace AddressRegistry.Api.Legacy.Address
{
    using System.Collections.Generic;
    using AddressRegistry.Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    public static class AddressMapper
    {
        public static VolledigAdres? GetVolledigAdres(string houseNumber, string boxNumber, string postalCode, StreetNameLatestItem? streetName, MunicipalityLatestItem? municipality)
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
            var geometry = WKBReaderFactory.CreateForLegacy().Read(point);

            return new Point
            {
                XmlPoint = new GmlPoint { Pos = $"{geometry.Coordinate.X.ToPointGeometryCoordinateValueFormat()} {geometry.Coordinate.Y.ToPointGeometryCoordinateValueFormat()}" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { geometry.Coordinate.X, geometry.Coordinate.Y } }
            };
        }

        public static PositieGeometrieMethode ConvertFromGeometryMethod(AddressRegistry.StreetName.GeometryMethod? method)
        {
            return method switch
            {
                AddressRegistry.StreetName.GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                AddressRegistry.StreetName.GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
                _ => PositieGeometrieMethode.AangeduidDoorBeheerder
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
                _ => PositieSpecificatie.Gemeente
            };
        }

        public static PositieGeometrieMethode ConvertFromGeometryMethod(GeometryMethod? method)
        {
            return method switch
            {
                GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
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
                _ => PositieSpecificatie.Gemeente
            };
        }

        public static AdresStatus ConvertFromAddressStatus(AddressStatus? status)
        {
            return status switch
            {
                AddressStatus.Proposed => AdresStatus.Voorgesteld,
                AddressStatus.Retired => AdresStatus.Gehistoreerd,
                AddressStatus.Rejected => AdresStatus.Afgekeurd,
                _ => AdresStatus.InGebruik
            };
        }

        public static AddressStatus? ConvertFromAdresStatus(AdresStatus? status)
        {
            return status switch
            {
                null => null,
                AdresStatus.Voorgesteld => AddressStatus.Proposed,
                AdresStatus.Gehistoreerd => AddressStatus.Retired,
                AdresStatus.Afgekeurd => AddressStatus.Rejected,
                _ => AddressStatus.Current
            };
        }

        public static AddressRegistry.StreetName.AddressStatus? ConvertFromAdresStatusV2(AdresStatus? status)
        {
            return status switch
            {
                null => null,
                AdresStatus.Voorgesteld => AddressRegistry.StreetName.AddressStatus.Proposed,
                AdresStatus.Gehistoreerd => AddressRegistry.StreetName.AddressStatus.Retired,
                AdresStatus.Afgekeurd => AddressRegistry.StreetName.AddressStatus.Rejected,
                _ => AddressRegistry.StreetName.AddressStatus.Current
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultMunicipalityName(MunicipalityLatestItem municipality)
        {
            return municipality.PrimaryLanguage switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, municipality.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, municipality.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, municipality.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, municipality.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultMunicipalityName(MunicipalityBosaItem municipality)
        {
            return municipality.PrimaryLanguage switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, municipality.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, municipality.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, municipality.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, municipality.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(StreetNameLatestItem streetName, Taal? municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?>? GetDefaultHomonymAddition(StreetNameLatestItem streetName, Taal? municipalityLanguage)
        {
            if (!streetName.HasHomonymAddition)
            {
                return null;
            }

            return municipalityLanguage switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.HomonymAdditionFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.HomonymAdditionGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.HomonymAdditionEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.HomonymAdditionDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(StreetNameBosaItem streetName, Taal? municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
            };
        }
    }
}
