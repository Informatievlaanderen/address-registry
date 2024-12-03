namespace AddressRegistry.Projections.Elastic.AddressList
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.Postal.Projections;
    using Consumer.Read.StreetName.Projections;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public sealed class AddressListDocument
    {
        public int AddressPersistentLocalId { get; set; }
        public int? ParentAddressPersistentLocalId { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public AddressStatus Status { get; set; }
        public bool OfficiallyAssigned { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public Municipality Municipality { get; set; }
        public PostalInfo? PostalInfo { get; set; }
        public StreetName StreetName { get; set; }

        public AddressPosition AddressPosition { get; set; }

        public AddressListDocument()
        { }

        public AddressListDocument(
            int addressPersistentLocalId,
            int? parentAddressPersistentLocalId,
            Instant versionTimestamp,
            AddressStatus status,
            bool officiallyAssigned,
            string houseNumber,
            string? boxNumber,
            Municipality municipality,
            PostalInfo? postalInfo,
            StreetName streetName,
            AddressPosition addressPosition)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            ParentAddressPersistentLocalId = parentAddressPersistentLocalId;
            VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();
            Status = status;
            OfficiallyAssigned = officiallyAssigned;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            Municipality = municipality;
            PostalInfo = postalInfo;
            StreetName = streetName;
            AddressPosition = addressPosition;
        }
    }

    public sealed class Municipality
    {
        public string NisCode { get; set; }
        public Name[] Names { get; set; }

        public Municipality()
        { }

        public Municipality(string nisCode, IEnumerable<Name> names)
        {
            NisCode = nisCode;
            Names = names.ToArray();
        }

        public static Municipality FromMunicipalityLatestItem(MunicipalityLatestItem municipalityLatestItem)
        {
            return new Municipality(
                municipalityLatestItem.NisCode,
                new[]
                    {
                        new Name(municipalityLatestItem.NameDutch, Language.nl),
                        new Name(municipalityLatestItem.NameFrench, Language.fr),
                        new Name(municipalityLatestItem.NameGerman, Language.de),
                        new Name(municipalityLatestItem.NameEnglish, Language.en)
                    }
                    .Where(x => !string.IsNullOrEmpty(x.Spelling)));
        }
    }

    public sealed class PostalInfo
    {
        public string PostalCode { get; set; }
        public Name[] Names { get; set; }

        public PostalInfo()
        { }

        public PostalInfo(string postalCode, IEnumerable<Name> names)
        {
            PostalCode = postalCode;
            Names = names.ToArray();
        }

        public static PostalInfo FromPostalLatestItem(PostalLatestItem postalInfoLatestItem)
        {
            var names = new List<Name>();
            foreach (var postalName in postalInfoLatestItem.PostalNames.Where(x => !string.IsNullOrWhiteSpace(x.PostalName)))
            {
                switch (postalName.Language)
                {
                    case PostalLanguage.Dutch:
                        names.Add(new Name(postalName.PostalName!, Language.nl));
                        break;
                    case PostalLanguage.French:
                        names.Add(new Name(postalName.PostalName!, Language.fr));
                        break;
                    case PostalLanguage.German:
                        names.Add(new Name(postalName.PostalName!, Language.de));
                        break;
                    case PostalLanguage.English:
                        names.Add(new Name(postalName.PostalName!, Language.en));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new PostalInfo(postalInfoLatestItem.PostalCode, names);
        }
    }

    public sealed class StreetName
    {
        public int StreetNamePersistentLocalId { get; set; }
        public Name[] Names { get; set; }

        public Name[] HomonymAdditions { get; set; }

        public StreetName()
        { }

        public StreetName(int streetNamePersistentLocalId, IEnumerable<Name> names, IEnumerable<Name> homonymAdditions)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Names = names.ToArray();
            HomonymAdditions = homonymAdditions.ToArray();
        }

        public static StreetName FromStreetNameLatestItem(StreetNameLatestItem streetName)
        {
            return new StreetName(
                streetName.PersistentLocalId,
                new[]
                    {
                        new Name(streetName.NameDutch, Language.nl),
                        new Name(streetName.NameFrench, Language.fr),
                        new Name(streetName.NameGerman, Language.de),
                        new Name(streetName.NameEnglish, Language.en)
                    }
                    .Where(x => !string.IsNullOrEmpty(x.Spelling)),
                new[]
                    {
                        new Name(streetName.HomonymAdditionDutch, Language.nl),
                        new Name(streetName.HomonymAdditionFrench, Language.fr),
                        new Name(streetName.HomonymAdditionGerman, Language.de),
                        new Name(streetName.HomonymAdditionEnglish, Language.en)
                    }
                    .Where(x => !string.IsNullOrEmpty(x.Spelling))
            );
        }
    }

    public sealed class AddressPosition
    {
        public string GeometryAsWkt { get; set; }
        public string GeometryAsWgs84 { get; set; }
        public GeometryMethod GeometryMethod { get; set; }
        public GeometrySpecification GeometrySpecification { get; set; }

        public AddressPosition()
        { }

        public AddressPosition(
            Point point,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification)
        {
            GeometryAsWkt = point.AsText();

            var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84(point);
            GeometryAsWgs84 = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", pointAsWgs84.X, pointAsWgs84.Y);

            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
        }

        public AddressPosition(
            string geometryAsWkt,
            string geometryAsWgs84,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification)
        {
            GeometryAsWkt = geometryAsWkt;
            GeometryAsWgs84 = geometryAsWgs84;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
        }
    }
}
