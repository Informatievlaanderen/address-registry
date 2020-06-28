namespace AddressRegistry.Api.Legacy.Tests.Framework.Generate
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using NetTopologySuite.Geometries;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;

    public static class Generate
    {
        public static readonly Generator<MunicipalityLatestItem> TblGemeente =
            new Generator<MunicipalityLatestItem>(r =>
            {
                var nameDutch = Produce.AlphaNumericString(10).Generate(r);

                return new MunicipalityLatestItem
                {
                    MunicipalityId = Guid.NewGuid(),
                    NisCode = NisCode.Generate(r),
                    PrimaryLanguage = Taal.NL,
                    NameDutch = nameDutch,
                    NameDutchSearch = nameDutch.ToLowerInvariant(),
                };
            });

        public static readonly Generator<PostalInfoLatestItem> TblPostInfo =
            new Generator<PostalInfoLatestItem>(r =>
            {
                var nameDutch = Produce.AlphaNumericString(10).Generate(r);

                return new PostalInfoLatestItem
                {
                    NisCode = NisCode.Generate(r),
                    PostalCode = Postcode.Generate(r),
                    PostalNames = new List<PostalInfoPostalName>
                    {
                        new PostalInfoPostalName
                        {
                            Language = Taal.NL,
                            PostalName = nameDutch
                        }
                    }
                };
            });

        public static readonly Generator<StreetNameLatestItem> TblStraatNaam =
            new Generator<StreetNameLatestItem>(r =>
                new StreetNameLatestItem
                {
                    StreetNameId = Guid.NewGuid(),
                    PersistentLocalId = Id.Generate(r).ToString(),
                    NisCode = NisCode.Generate(r),
                    NameDutch = Straatnaam.Generate(r),
                    IsComplete = true
                });

        public static readonly Generator<AddressDetailItem> TblHuisNummer =
            new Generator<AddressDetailItem>(r =>
                new AddressDetailItem
                {
                    AddressId = Guid.NewGuid(),
                    StreetNameId = Guid.NewGuid(),
                    HouseNumber = Huisnummer.Generate(r),
                    BoxNumber = Busnummer.Generate(r),
                    Status = AddressStatus.Current,
                    Position = DbGeometry.Generate(r).AsBinary(),
                    PositionMethod = GeometryMethod.AppointedByAdministrator,
                    PositionSpecification = GeometrySpecification.BuildingUnit,
                    PostalCode = Postcode.Generate(r),
                    PersistentLocalId = Id.Generate(r),
                    Complete = true,
                    OfficiallyAssigned = true,
                });

        public static readonly Generator<string> NisCode = Produce.NumericString(5);

        public static readonly Generator<string> VbrObjectId = Produce.NumericString(5);

        public static readonly Generator<string> CapaKey =
            new Generator<string>(r =>
                $"{Produce.NumericString(5).Generate(r)}{Produce.UpperCaseString(1).Generate(r)}{Produce.NumericString(4).Generate(r)}-{Produce.NumericString(2).Generate(r)}{Produce.UpperCaseString(1).Generate(r)}{Produce.NumericString(3).Generate(r)}");

        public static readonly Generator<int> VbrObjectIdInt = Produce.Integer(1, 100000);

        public static readonly Generator<int> Version = Produce.Integer(1, 20);

        public static readonly Generator<string> Gemeentenaam = Produce.AlphaNumericString(4, 15);

        public static readonly Generator<string> Straatnaam = Produce.AlphaNumericString(3, 25);

        public static readonly Generator<int> Id = Produce.Integer();

        public static readonly Generator<string> Busnummer = Produce.NumericString(2);

        public static readonly Generator<string> Huisnummer = Produce.NumericString(2);

        public static readonly Generator<string> Postcode = Produce.NumericString(4);

        public static readonly Generator<Point> DbGeometry = new Generator<Point>(r =>
        {
            //pick a random point on planet earth
            var longitude = r.Next(-180, 181);
            var latitude = r.Next(-90, 90);
            return new Point(longitude, latitude);
        });

        public static Generator<PositieSpecificatie> AdresPositieSpecificatieEnum = Produce.Integer(1, 12).Select(i => i == 10 ? 1 : i).Select(i => (PositieSpecificatie)i);//id 10 does not exist

    }
}
