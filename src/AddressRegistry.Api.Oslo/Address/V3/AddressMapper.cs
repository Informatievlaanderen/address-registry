namespace AddressRegistry.Api.Oslo.Address.V3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using Projections.Elastic.AddressList;
    using StreetName;
    using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;
    // Spelled out: AddressRegistry's reader falls back to Lambert 72 for EWKB without an SRID, where
    // GrAr's throws. A bare `using WKBReaderFactory = WKBReaderFactory;` resolved to the right one only
    // by accident of which namespaces this file happens to import.
    using WKBReaderFactory = AddressRegistry.WKBReaderFactory;

    public static class AddressMapper
    {
        public static VolledigAdres? GetVolledigAdres(AddressListDocument addressListDocument)
        {
            if (string.IsNullOrEmpty(addressListDocument.Municipality.NisCode))
            {
                return null;
            }

            var volledigAdres = new VolledigAdres();
            foreach (var streetNameName in addressListDocument.StreetName.Names)
            {
                var language = streetNameName.Language;

                var municipalityName = addressListDocument.Municipality.Names.FirstOrDefault(x => x.Language == language);

                volledigAdres.Add(streetNameName.Spelling,
                    addressListDocument.HouseNumber,
                    addressListDocument.BoxNumber ?? string.Empty,
                    addressListDocument.PostalInfo?.PostalCode ?? string.Empty,
                    municipalityName?.Spelling ?? addressListDocument.Municipality.Names.First().Spelling,
                    MapElasticLanguageToTaal(language));
            }

            return volledigAdres;
        }

        private static Taal MapElasticLanguageToTaal(Language language)
        {
            return language switch
            {
                Language.nl => Taal.Nl,
                Language.fr => Taal.Fr,
                Language.de => Taal.De,
                Language.en => Taal.En,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };
        }

        public static VolledigAdres? GetVolledigAdres(string houseNumber, string boxNumber, string postalCode,
            StreetNameLatestItem? streetName, MunicipalityLatestItem? municipality)
        {
            if (streetName == null || municipality == null)
            {
                return null;
            }

            var municipalityNames = new List<GeografischeNaam>
            {
                new GeografischeNaam(municipality.NameDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(municipality.NameFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(municipality.NameGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(municipality.NameEnglish ?? string.Empty, Taal.En)
            };

            municipalityNames = municipalityNames.Where(n => !string.IsNullOrEmpty(n.Spelling)).ToList();

            var streetNameNames = new List<GeografischeNaam>
            {
                new GeografischeNaam(streetName.NameDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(streetName.NameFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(streetName.NameGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(streetName.NameEnglish ?? string.Empty, Taal.En)
            };

            streetNameNames = streetNameNames.Where(n => !string.IsNullOrEmpty(n.Spelling)).ToList();

            var volledigAdres = new VolledigAdres();
            foreach (var streetNameName in streetNameNames)
            {
                var taal = streetNameName.Taal;

                var municipalityName = municipalityNames.FirstOrDefault(x => x.Taal == taal);

                volledigAdres.Add(streetNameName.Spelling,
                    houseNumber,
                    boxNumber,
                    postalCode,
                    municipalityName?.Spelling ?? municipalityNames.First().Spelling,
                    taal);
            }
            return volledigAdres;
        }

        public static AddressPositionV3 GetAddressPoint(
            byte[] point,
            GeometryMethod? method,
            GeometrySpecification? specification)
        {
            var geometry = WKBReaderFactory.CreateForEwkb(point).Read(point);
            var gmls = new List<string>();

            // Version 3 answers in the reference system the position is persisted in, preceded by its
            // Lambert 72 equivalent for as long as that is not Lambert 72 itself. See ADR 0004.
            if (!geometry.IsLambert72())
            {
                gmls.Add(GeometryExtensions.ConvertToGml(geometry.EnsureLambert72(), false));
            }

            gmls.Add(GeometryExtensions.ConvertToGml(geometry, false));

            var positieSpecificatie = ConvertFromGeometrySpecification(specification);
            var positieGeometrieMethode = ConvertFromGeometryMethod(method);
            return new AddressPositionV3(gmls.Select(x => new PointGeometrie(x)), positieGeometrieMethode, positieSpecificatie);
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

        public static AdresStatusValue ConvertFromAddressStatus(AddressStatus? status)
        {
            return status switch
            {
                AddressStatus.Proposed => AdresStatusValue.Voorgesteld,
                AddressStatus.Retired => AdresStatusValue.Gehistoreerd,
                AddressStatus.Current => AdresStatusValue.InGebruik,
                AddressStatus.Rejected => AdresStatusValue.Afgekeurd,
                _ => AdresStatusValue.InGebruik
            };
        }

        public static IEnumerable<GeografischeNaam> GetMunicipalityNames(MunicipalityLatestItem municipality)
        {
            var names = new List<GeografischeNaam>
            {
                new GeografischeNaam(municipality.NameDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(municipality.NameFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(municipality.NameGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(municipality.NameEnglish ?? string.Empty, Taal.En)
            };
            return names.Where(n => !string.IsNullOrEmpty(n.Spelling));
        }

        public static IEnumerable<GeografischeNaam> GetStreetNameNames(StreetNameLatestItem streetName)
        {
            var names = new List<GeografischeNaam>
            {
                new GeografischeNaam(streetName.NameDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(streetName.NameFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(streetName.NameGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(streetName.NameEnglish ?? string.Empty, Taal.En)
            };

            return names.Where(n => !string.IsNullOrEmpty(n.Spelling));
        }

        public static IEnumerable<GeografischeNaam>? GetHomonymAdditions(StreetNameLatestItem streetName)
        {
            if (!streetName.HasHomonymAddition)
            {
                return null;
            }

            var homonyms = new List<GeografischeNaam>
            {
                new GeografischeNaam(streetName.HomonymAdditionDutch ?? string.Empty, Taal.Nl),
                new GeografischeNaam(streetName.HomonymAdditionFrench ?? string.Empty, Taal.Fr),
                new GeografischeNaam(streetName.HomonymAdditionGerman ?? string.Empty, Taal.De),
                new GeografischeNaam(streetName.HomonymAdditionEnglish ?? string.Empty, Taal.En)
            };

            return homonyms.Where(n => !string.IsNullOrEmpty(n.Spelling));
        }
    }
}
