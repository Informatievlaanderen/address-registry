namespace AddressRegistry.Projections.Elastic.AddressSearch
{
    using System;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using DotSpatial.Projections;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class AddressSearchDocument
    {
        public int AddressPersistentLocalId { get; set; }
        public int? ParentAddressPersistentLocalId { get; set; }

        public DateTimeOffset VersionTimestamp { get; set; }
        public AddressStatus Status { get; set; }
        public bool Active => Status is AddressStatus.Proposed or AddressStatus.Current;
        public bool OfficiallyAssigned { get; set; }
        public string HouseNumber { get; set; } = string.Empty;
        public string? BoxNumber { get; set; }

        public Municipality Municipality { get; set; }
        public PostalInfo PostalInfo { get; set; }
        public StreetName StreetName { get; set; }
        public Name[] FullAddress { get; set; }

        public AddressPosition AddressPosition { get; set; }

        public AddressSearchDocument()
        { }

        public AddressSearchDocument(
            int addressPersistentLocalId,
            int? parentAddressPersistentLocalId,
            Instant versionTimestamp,
            AddressStatus status,
            bool officiallyAssigned,
            string? boxNumber,
            Municipality municipality,
            PostalInfo postalInfo,
            StreetName streetName,
            Name[] fullAddress,
            AddressPosition addressPosition)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            ParentAddressPersistentLocalId = parentAddressPersistentLocalId;
            VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();
            Status = status;
            OfficiallyAssigned = officiallyAssigned;
            BoxNumber = boxNumber;
            Municipality = municipality;
            PostalInfo = postalInfo;
            StreetName = streetName;
            FullAddress = fullAddress;
            AddressPosition = addressPosition;
        }
    }

    public class Municipality
    {
        public string NisCode { get; set; }
        public Name[] Names { get; set; }

        public Municipality()
        { }

        public Municipality(string nisCode, Name[] names)
        {
            NisCode = nisCode;
            Names = names;
        }
    }

    public class PostalInfo
    {
        public string PostalCode { get; set; }
        public Name[] Names { get; set; }

        public PostalInfo()
        { }

        public PostalInfo(string postalCode, Name[] names)
        {
            PostalCode = postalCode;
            Names = names;
        }
    }

    public class StreetName
    {
        public int StreetNamePersistentLocalId { get; set; }
        public Name[] Names { get; set; }

        public StreetName()
        { }

        public StreetName(int streetNamePersistentLocalId, Name[] names)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Names = names;
        }
    }

    public class Name
    {
        public string Spelling { get; set; }
        public Language Language { get; set; }

        public Name()
        { }

        public Name(string spelling, Language language)
        {
            Spelling = spelling;
            Language = language;
        }
    }

    public enum Language
    {
        nl,
        en,
        fr,
        de
    }

    public class AddressPosition
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
            GeometryAsWkt = point.AsText(); // todo-rik check if srid is added or needs to be added?
            double[] coordinates = [point.X, point.Y];
            Reproject.ReprojectPoints(
                coordinates,
                [],
                KnownCoordinateSystems.Geographic.Europe.Belge1972,
                KnownCoordinateSystems.Geographic.World.WGS1984,
                0,
                1);
            GeometryAsWgs84 = $"{coordinates[0]}, {coordinates[1]}";
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
