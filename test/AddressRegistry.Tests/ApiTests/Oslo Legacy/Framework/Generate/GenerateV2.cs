namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Generate
{
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public static class GenerateV2
    {
        public static readonly Generator<MunicipalityLatestItem> TblGemeente =
            new Generator<MunicipalityLatestItem>(r =>
            {
                var nameDutch = Produce.AlphaNumericString(10).Generate(r);

                return new MunicipalityLatestItem
                {
                    MunicipalityId = Guid.NewGuid(),
                    NisCode = NisCode.Generate(r),
                    OfficialLanguages = new List<string>() { MunicipalityLanguage.Dutch.ToString() },
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
                    PersistentLocalId = Id.Generate(r),
                    NisCode = NisCode.Generate(r),
                    NameDutch = Straatnaam.Generate(r)
                });

        public static readonly Generator<AddressDetailItemV2> TblHuisNummer =
            new Generator<AddressDetailItemV2>(r =>
                new AddressDetailItemV2(Id.Generate(r), Id.Generate(r), Postcode.Generate(r), Huisnummer.Generate(r),
                    Busnummer.Generate(r), AddressStatus.Current, true,
                    DbGeometry.Generate(r).AsBinary(), GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.BuildingUnit,false, SystemClock.Instance.GetCurrentInstant()));

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
