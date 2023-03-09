namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Generate
{
    using System;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NodaTime;

    public static class ModifiersV2
    {
        public static MunicipalityLatestItem WithNisGemeenteCode(
            this MunicipalityLatestItem original,
            string nisGemeenteCode)
            => new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = nisGemeenteCode,
                OfficialLanguages = original.OfficialLanguages,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static MunicipalityLatestItem WithGemeenteVersion(
            this MunicipalityLatestItem original,
            Instant version)
            => new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = original.NisCode,
                OfficialLanguages = original.OfficialLanguages,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp =  version
            };

        public static MunicipalityLatestItem WithGemeenteId(
            this MunicipalityLatestItem original,
            Guid gemeenteId)
            => new MunicipalityLatestItem
            {
                MunicipalityId = gemeenteId,
                NisCode = original.NisCode,
                OfficialLanguages = original.OfficialLanguages,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = original.VersionTimestamp
            };

        public static MunicipalityLatestItem WithGemeenteNaam(
            this MunicipalityLatestItem original,
            string gemeenteNaam)
            => new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = original.NisCode,
                OfficialLanguages = original.OfficialLanguages,
                NameDutch = gemeenteNaam,
                NameDutchSearch = gemeenteNaam.RemoveDiacritics(),
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = original.VersionTimestamp
            };

        public static StreetNameLatestItem WithGemeenteId(
            this StreetNameLatestItem original,
            string nisCode)
            => new StreetNameLatestItem
            {
                PersistentLocalId = original.PersistentLocalId,
                NisCode = nisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static StreetNameLatestItem WithPersistentLocalId(
            this StreetNameLatestItem original,
            int persistentLocalId)
            => new StreetNameLatestItem
            {
                PersistentLocalId = persistentLocalId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static StreetNameLatestItem WithStraatNaamVersion(
            this StreetNameLatestItem original,
            Instant version)
            => new StreetNameLatestItem
            {
                PersistentLocalId = original.PersistentLocalId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = version,
            };

        public static StreetNameLatestItem WithStraatNaam(
            this StreetNameLatestItem original,
            string straatNaam)
            => new StreetNameLatestItem
            {
                PersistentLocalId = original.PersistentLocalId,
                NisCode = original.NisCode,
                NameDutch = straatNaam,
                NameDutchSearch = straatNaam.RemoveDiacritics(),
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static AddressDetailItemV2 WithStraatNaamId(
            this AddressDetailItemV2 original,
            int streetNamePersistentLocalId)
            => new AddressDetailItemV2(original.AddressPersistentLocalId, streetNamePersistentLocalId,
                original.PostalCode, original.HouseNumber,
                original.BoxNumber, original.Status, original.OfficiallyAssigned, original.Position,
                original.PositionMethod, original.PositionSpecification,
                original.Removed, original.VersionTimestamp);

        public static AddressDetailItemV2 WithPersistentLocalId(
            this AddressDetailItemV2 original,
            int persistentLocalId)
            => new AddressDetailItemV2(persistentLocalId, original.StreetNamePersistentLocalId,
                original.PostalCode, original.HouseNumber,
                original.BoxNumber, original.Status, original.OfficiallyAssigned, original.Position,
                original.PositionMethod, original.PositionSpecification,
                original.Removed, original.VersionTimestamp);

        public static AddressDetailItemV2 WithHuisNummer(
            this AddressDetailItemV2 original,
            string huisNummer)
            => new AddressDetailItemV2(original.AddressPersistentLocalId, original.StreetNamePersistentLocalId,
                original.PostalCode, huisNummer,
                original.BoxNumber, original.Status, original.OfficiallyAssigned, original.Position,
                original.PositionMethod, original.PositionSpecification,
                original.Removed, original.VersionTimestamp);

        public static AddressDetailItemV2 WithBusnummer(
            this AddressDetailItemV2 original,
            string busNummer)
            => new AddressDetailItemV2(original.AddressPersistentLocalId, original.StreetNamePersistentLocalId,
                original.PostalCode, original.HouseNumber,
                busNummer, original.Status, original.OfficiallyAssigned, original.Position,
                original.PositionMethod, original.PositionSpecification,
                original.Removed, original.VersionTimestamp);

        public static AddressDetailItemV2 WithVersion(
            this AddressDetailItemV2 original,
            Instant version)
            => new AddressDetailItemV2(original.AddressPersistentLocalId, original.StreetNamePersistentLocalId,
                original.PostalCode, original.HouseNumber,
                original.BoxNumber, original.Status, original.OfficiallyAssigned, original.Position,
                original.PositionMethod, original.PositionSpecification,
                original.Removed, version);

        public static AddressDetailItemV2 WithTblPostKanton(
            this AddressDetailItemV2 original,
            string postcode)
            => new AddressDetailItemV2(original.AddressPersistentLocalId, original.StreetNamePersistentLocalId,
                postcode, original.HouseNumber,
                original.BoxNumber, original.Status, original.OfficiallyAssigned, original.Position,
                original.PositionMethod, original.PositionSpecification,
                original.Removed, original.VersionTimestamp);

        //    public static PostalInfoLatestItem WithPostcode(
        //        this PostalInfoLatestItem original,
        //        string postcode)
        //        => new PostalInfoLatestItem
        //        {
        //            PostalCode = postcode,
        //            Version = original.Version,
        //            PostalNames = original.PostalNames,
        //            NisCode = original.NisCode,
        //        };

        //    public static PostalInfoLatestItem WithNisCode(
        //        this PostalInfoLatestItem original,
        //        string nisCode)
        //        => new PostalInfoLatestItem
        //        {
        //            PostalCode = original.PostalCode,
        //            Version = original.Version,
        //            PostalNames = original.PostalNames,
        //            NisCode = nisCode,
        //        };

        //    public static PostalInfoLatestItem WithPostnamen(
        //        this PostalInfoLatestItem original,
        //        IEnumerable<PostalInfoPostalName> postnamen)
        //        => new PostalInfoLatestItem
        //        {
        //            PostalCode = original.PostalCode,
        //            Version = original.Version,
        //            PostalNames = postnamen.ToList(),
        //            NisCode = original.NisCode,
        //        };

        //    public static PostalInfoLatestItem WithPostnaam(
        //        this PostalInfoLatestItem original,
        //        string postnaam)
        //        => new PostalInfoLatestItem
        //        {
        //            PostalCode = original.PostalCode,
        //            Version = original.Version,
        //            PostalNames = new List<PostalInfoPostalName>
        //            {
        //                new PostalInfoPostalName
        //                {
        //                    Language = Taal.NL,
        //                    PostalCode = original.PostalCode,
        //                    PostalName = postnaam
        //                }
        //            },
        //            NisCode = original.NisCode,
        //        };
    }
}
