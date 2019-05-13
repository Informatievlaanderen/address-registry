namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Generate
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
                Status = AddressStatus.Current,
                Position = DbGeometry.Generate(r).AsBinary(),
                PositionMethod = GeometryMethod.AppointedByAdministrator,
                PositionSpecification = GeometrySpecification.BuildingUnit,
                PostalCode = Postcode.Generate(r),
                OsloId = Id.Generate(r),
                Complete = true,
            };
        });

        //public static Generator<IEnumerable<IObjectEnricher<ICrabGemeenteBuilder>>> CrabGemeenteTimeline(int gemeenteId, string nisCode, string gemeenteNaam)
        //{
        //    return new Generator<IEnumerable<IObjectEnricher<ICrabGemeenteBuilder>>>(r =>
        //        Produce.One(Generate.tblGemeente
        //               .Select(g => g.WithNisGemeenteCode(nisCode))
        //               .Select(g => g.WithGemeenteId(gemeenteId)))
        //               .Generate(r).Cast<IObjectEnricher<ICrabGemeenteBuilder>>()
        //           .Concat(
        //           Produce.One(Generate.tblGemeenteNaam
        //               .Select(g => g.WithGemeenteNaam(gemeenteNaam))
        //               .Select(g => g.WithGemeenteId(gemeenteId))).Generate(r)));
        //}

        //public static Generator<IEnumerable<IObjectEnricher<ICrabStraatnaamBuilder>>> CrabStraatnaamTimeline(int gemeenteId, string nisCode, string gemeenteNaam, int straatNaamId, string straatNaam)
        //{
        //    return new Generator<IEnumerable<IObjectEnricher<ICrabStraatnaamBuilder>>>(r =>
        //        Produce.One(Generate.tblStraatNaam
        //                .Select(g => g.WithStraatNaamId(straatNaamId))
        //                .Select(g => g.WithStraatNaam(straatNaam))
        //                .Select(g => g.WithGemeenteId(gemeenteId)))
        //                .Generate(r).Cast<IObjectEnricher<ICrabStraatnaamBuilder>>()
        //            .Concat(
        //            Produce.One(Generate.tblStraatnaamstatus
        //                .Select(g => g.WithStraatnaamid(straatNaamId)))
        //                .Generate(r))
        //            .Concat(
        //            CrabGemeenteTimeline(gemeenteId, nisCode, gemeenteNaam)
        //                .Generate(r)
        //                .Where(e => e.GetType().IsAssignableTo<IObjectEnricher<ICrabStraatnaamBuilder>>())
        //                .Cast<IObjectEnricher<ICrabStraatnaamBuilder>>()));
        //}

        //public static Generator<IEnumerable<IObjectEnricher<ICrabHuisnummerBuilder>>> CrabHuisnummerTimeline(int adresID, int straatNaamId, int huisNummerId, string huisNummer, string postKantCode)
        //{
        //    return new Generator<IEnumerable<IObjectEnricher<ICrabHuisnummerBuilder>>>(r =>
        //        Produce.One(tblHuisNummer
        //                .Select(g => g.WithStraatNaamId(straatNaamId))
        //                .Select(g => g.WithHuisNummerId(huisNummerId))
        //                .Select(g => g.WithHuisNummer(huisNummer)))
        //                .Generate(r).Cast<IObjectEnricher<ICrabHuisnummerBuilder>>()
        //            .Concat(
        //            Produce.One(tblHuisnummerstatus
        //                .Select(g => g.WithHuisNummerId(huisNummerId)))
        //                .Generate(r))
        //            .Concat(
        //            Produce.One(tblAdrespositie
        //                .Select(g => g.WithAdresID(adresID)))
        //                .Generate(r))
        //            .Concat(
        //            Produce.One(tblHuisNummer_postKanton
        //                .Select(g => g.WithHuisNummerId(huisNummerId))
        //                .Select(g => g.WithTblPostKanton(tblPostKanton.Select(pc => pc.WithPostKantonCode(postKantCode)).Generate(r))))
        //                .Generate(r)));
        //}

        //public static Generator<GemeenteLatestVersionWithObjectID> GemeenteLatestVersionWithObjectID = new Generator<GemeenteLatestVersionWithObjectID>(r =>
        //{
        //    return new GemeenteLatestVersionWithObjectID
        //    {
        //        NisCode = NISCode.Generate(r),
        //        Gemeentenaam = Gemeentenaam.Generate(r),
        //        TaalCode = 1,
        //        IsValid = true,
        //        IsRemoved = false,
        //        Version = 1,
        //        VersionTimestamp = DateTime.Now,
        //    };
        //});


        //public static Generator<StraatnaamLatestVersionWithObjectID> StraatnaamLatestVersionWithObjectID = new Generator<StraatnaamLatestVersionWithObjectID>(r =>
        //{
        //    return new StraatnaamLatestVersionWithObjectID
        //    {
        //        StraatnaamID = VbrObjectIDInt.Generate(r),
        //        Gemeentenaam = Gemeentenaam.Generate(r),
        //        Straatnaam = Straatnaam.Generate(r),
        //        TaalCode = 1,
        //        IsValid = true,
        //        IsRemoved = false,
        //        Version = 1,
        //        VersionTimestamp = DateTime.Now,
        //        HomoniemToevoeging = string.Empty
        //    };
        //});

        //public static Generator<AdresLatestVersionWithObjectID> AdresLatestVersionWithObjectID = new Generator<AdresLatestVersionWithObjectID>(r =>
        //    {
        //        return new AdresLatestVersionWithObjectID
        //        {
        //            AdresID = VbrObjectIDInt.Generate(r),
        //            Gemeentenaam = Gemeentenaam.Generate(r),
        //            Postcode = Postcode.Generate(r),
        //            Straatnaam = Straatnaam.Generate(r),
        //            Huisnummer = Huisnummer.Generate(r),
        //            Busnummer = Busnummer.Generate(r),
        //            TaalCode = 1,
        //            IsValid = true,
        //            IsRemoved = false,
        //            Version = 1,
        //            VersionTimestamp = DateTime.Now,
        //            HomoniemToevoeging = string.Empty,
        //        };
        //    });

        //public static Generator<AdresMappingQueryResult> AdresMappingQueryResult_ToHuisnummer = new Generator<AdresMappingQueryResult>(r =>
        //{
        //    return new AdresMappingQueryResult
        //    {
        //        CrabHuisnummerID = Id.Generate(r)
        //    };
        //});

        //public static Generator<PostinfoDTOWithGemeente> PostinfoDTOWithGemeente = new Generator<PostinfoDTOWithGemeente>(r =>
        //{
        //    return new PostinfoDTOWithGemeente(Postcode.Generate(r), Version.Generate(r), Produce.One(PostnaamDTO).Generate(r).ToList(), Produce.One(NISCode).Generate(r).ToList());
        //});

        //public static Generator<PostnaamDTO> PostnaamDTO = new Generator<PostnaamDTO>(r =>
        //{
        //    return new PostnaamDTO(Gemeentenaam.Generate(r), GeografischeNaamTaalCode.NL);
        //});

        //public static Generator<LatestGemeente> LatestGemeente = new Generator<LatestGemeente>(r =>
        //{
        //    return new LatestGemeente
        //    {
        //        Gemeentenaam = Gemeentenaam.Generate(r),
        //        VbrObjectID = NISCode.Generate(r),
        //        TaalCode = 1,
        //        Version = Produce.Integer().Generate(r),
        //        VersionTimestamp = DateTime.Now,
        //        Geometry = SimpleGeometry.FromDbGeometry(DbGeometry.Generate(r))
        //    };
        //});

        //public static Generator<LatestStraatnaam> LatestStraatnaam = new Generator<LatestStraatnaam>(r =>
        //{
        //    return new LatestStraatnaam
        //    {
        //        Gemeentenaam = Gemeentenaam.Generate(r),
        //        VbrObjectID = VbrObjectIDInt.Generate(r),
        //        HomoniemToevoeging = null,
        //        Naam = Straatnaam.Generate(r),
        //        TaalCode = 1,
        //        Version = Produce.Integer().Generate(r),
        //        VersionTimestamp = DateTime.Now,
        //        NisCode = NISCode.Generate(r),
        //        Status = Straatnaamstatus.InGebruik
        //    };
        //});

        //public static Generator<LatestAdres> LatestAdres = new Generator<LatestAdres>(r =>
        //{
        //    var gemeente = LatestGemeente.Generate(r);
        //    var straatnaam = LatestStraatnaam.Select(s => s.WithGemeentenaam(gemeente.Gemeentenaam).WithNisCode(gemeente.VbrObjectID)).Generate(r);
        //    return new LatestAdres
        //    {
        //        Busnummer = Busnummer.Generate(r),
        //        Gemeente = gemeente,
        //        Huisnummer = Huisnummer.Generate(r),
        //        Officieus = false,
        //        Positie = SimpleGeometry.FromDbGeometry(DbGeometry.Generate(r)),
        //        PositieMethode = AdresPositieMethode.FromId(Produce.Integer(1, 4).Generate(r)),
        //        PositieSpecificatie = AdresPositieSpecificatieEnum.Generate(r),
        //        Postcode = Postcode.Generate(r),
        //        Status = AdresStatus.FromID(Produce.Integer(1, 4).Generate(r)),
        //        Straatnaam = straatnaam,
        //        StraatnaamId = straatnaam.VbrObjectID,
        //        VbrObjectID = VbrObjectIDInt.Generate(r),
        //        Version = Version.Generate(r),
        //        VersionTimestamp = DateTime.Now,
        //        GebouweenheidObjectIds = Produce.Many(VbrObjectIDInt).Generate(r),
        //        PerceelCapaKeys = Produce.Many(CapaKey).Generate(r)
        //    };
        //});


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
