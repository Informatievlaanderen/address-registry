namespace AddressRegistry.Api.Legacy.Address
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.SpatialTools.GeometryCoordinates;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using Projections.Legacy.AddressList;
    using Projections.Legacy.AddressListV2;
    using StreetName;
    using AddressStatus = AddressRegistry.Address.AddressStatus;
    using MunicipalityBosaItem = Projections.Syndication.Municipality.MunicipalityBosaItem;
    using MunicipalityLanguage = Consumer.Read.Municipality.Projections.MunicipalityLanguage;
    using StreetNameBosaItem = Projections.Syndication.StreetName.StreetNameBosaItem;

    public static class AddressMapper
    {
        public static VolledigAdres? GetVolledigAdres(AddressListViewItem addressListViewItem)
        {
            if (string.IsNullOrEmpty(addressListViewItem.StreetNamePersistentLocalId)
                || string.IsNullOrEmpty(addressListViewItem.NisCode))
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

        public static VolledigAdres? GetVolledigAdres(string houseNumber, string boxNumber, string postalCode, Projections.Syndication.StreetName.StreetNameLatestItem? streetName, Projections.Syndication.Municipality.MunicipalityLatestItem? municipality)
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

        public static VolledigAdres GetVolledigAdres(
            string houseNumber,
            string? boxNumber,
            string? postalCode,
            Consumer.Read.StreetName.Projections.StreetNameBosaItem streetName,
            MunicipalityLatestItem municipality)
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

        public static PositieGeometrieMethode ConvertFromGeometryMethod(AddressRegistry.Address.GeometryMethod? method)
        {
            return method switch
            {
                AddressRegistry.Address.GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                AddressRegistry.Address.GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
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
                MunicipalityLanguage.French => new KeyValuePair<Taal, string?>(Taal.FR, municipality.NameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string?>(Taal.DE, municipality.NameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string?>(Taal.EN, municipality.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, municipality.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultMunicipalityName(Projections.Syndication.Municipality.MunicipalityLatestItem municipality)
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

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(Projections.Syndication.StreetName.StreetNameLatestItem streetName, Taal? taal)
        {
            return taal switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(Projections.Syndication.StreetName.StreetNameLatestItem streetName, MunicipalityLanguage municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                MunicipalityLanguage.French => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
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

        public static KeyValuePair<Taal, string?>? GetDefaultHomonymAddition(Projections.Syndication.StreetName.StreetNameLatestItem streetName, Taal? municipalityLanguage)
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

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(StreetNameBosaItem streetName, Taal? taal)
        {
            return taal switch
            {
                Taal.FR => new KeyValuePair<Taal, string?>(Taal.FR, streetName.NameFrench),
                Taal.DE => new KeyValuePair<Taal, string?>(Taal.DE, streetName.NameGerman),
                Taal.EN => new KeyValuePair<Taal, string?>(Taal.EN, streetName.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetName.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(StreetNameBosaItem streetNameBosaItem, MunicipalityLanguage municipalityLanguage)
        {
            return municipalityLanguage switch
            {
                MunicipalityLanguage.French => new KeyValuePair<Taal, string?>(Taal.FR, streetNameBosaItem.NameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string?>(Taal.DE, streetNameBosaItem.NameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string?>(Taal.EN, streetNameBosaItem.NameEnglish),
                _ => new KeyValuePair<Taal, string?>(Taal.NL, streetNameBosaItem.NameDutch)
            };
        }

        public static KeyValuePair<Taal, string?> GetDefaultStreetNameName(
            Consumer.Read.StreetName.Projections.StreetNameBosaItem streetName,
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
    }
}
