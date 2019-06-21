namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Generate
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;

    public static class Generate
    {
        public static Generator<MunicipalityLatestItem> tblGemeente =
            new Generator<MunicipalityLatestItem>(r =>
            {
                var nameDutch = Produce.AlphaNumericString(10).Generate(r);
                return new MunicipalityLatestItem
                {
                    MunicipalityId = Guid.NewGuid(),
                    NisCode = NISCode.Generate(r),
                    PrimaryLanguage = Taal.NL,
                    NameDutch = nameDutch,
                    NameDutchSearch = nameDutch.ToLowerInvariant(),
                };
            });

        public static Generator<PostalInfoLatestItem> tblPostInfo =
            new Generator<PostalInfoLatestItem>(r =>
            {
                var nameDutch = Produce.AlphaNumericString(10).Generate(r);
                return new PostalInfoLatestItem
                {
                    NisCode = NISCode.Generate(r),
                    PostalCode = Postcode.Generate(r),
                    PostalNames = new List<PostalInfoPostalName> { new PostalInfoPostalName { Language = Taal.NL, PostalName = nameDutch } }
                };
            });

        public static Generator<StreetNameLatestItem> tblStraatNaam =
            new Generator<StreetNameLatestItem>(r => new StreetNameLatestItem
            {
                StreetNameId = Guid.NewGuid(),
                OsloId = Id.Generate(r).ToString(),
                NisCode = NISCode.Generate(r),
                NameDutch = Straatnaam.Generate(r),
                IsComplete = true
            });

        public static Generator<AddressDetailItem> tblHuisNummer = new Generator<AddressDetailItem>(r =>
        {
            return new AddressDetailItem
            {
                AddressId = Guid.NewGuid(),
                StreetNameId = Guid.NewGuid(),
                HouseNumber = Huisnummer.Generate(r),
                BoxNumber = Busnummer.Generate(r),
                Status = AddressStatus.Current,
                Position = DbGeometry.Generate(r),
                PositionMethod = GeometryMethod.AppointedByAdministrator,
                PositionSpecification = GeometrySpecification.BuildingUnit,
                PostalCode = Postcode.Generate(r),
                OsloId = Id.Generate(r),
                Complete = true,
                OfficiallyAssigned = true,
            };
        });

        public static Generator<string> NISCode = Produce.NumericString(5);
        public static Generator<string> VbrObjectID = Produce.NumericString(5);
        public static Generator<string> CapaKey = new Generator<string>(r => string.Format($"{Produce.NumericString(5).Generate(r)}{Produce.UpperCaseString(1).Generate(r)}{Produce.NumericString(4).Generate(r)}-{Produce.NumericString(2).Generate(r)}{Produce.UpperCaseString(1).Generate(r)}{Produce.NumericString(3).Generate(r)}"));
        public static Generator<int> VbrObjectIDInt = Produce.Integer(1, 100000);
        public static Generator<int> Version = Produce.Integer(1, 20);
        public static Generator<string> Gemeentenaam = Produce.AlphaNumericString(4, 15);
        public static Generator<string> Straatnaam = Produce.AlphaNumericString(3, 25);
        public static Generator<int> Id = Produce.Integer();
        public static Generator<string> Busnummer = Produce.NumericString(2);
        public static Generator<string> Huisnummer = Produce.NumericString(2);
        public static Generator<string> Postcode = Produce.NumericString(4);
        public static Generator<IPoint> DbGeometry = new Generator<IPoint>(r =>
        {
            //pick a random point on planet earth
            int longitude = r.Next(-180, 181);
            int latitude = r.Next(-90, 90);
            return new Point(longitude, latitude);
        });

        public static Generator<PositieSpecificatie> AdresPositieSpecificatieEnum = Produce.Integer(1, 12).Select(i => i == 10 ? 1 : i).Select(i => (PositieSpecificatie)i);//id 10 does not exist

    }
}
