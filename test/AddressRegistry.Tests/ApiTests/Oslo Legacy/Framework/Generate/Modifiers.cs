namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Generate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Syndication.Municipality;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using AddressRegistry.Projections.Syndication.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using NodaTime;

    public static class Modifiers
    {
        public static MunicipalityLatestItem WithNisGemeenteCode(
            this MunicipalityLatestItem original,
            string nisGemeenteCode)
            => new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = nisGemeenteCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                Version = original.Version
            };

        public static MunicipalityLatestItem WithGemeenteVersion(
            this MunicipalityLatestItem original,
            string version)
            => new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = original.NisCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                Version = version
            };

        public static MunicipalityLatestItem WithGemeenteId(
            this MunicipalityLatestItem original,
            Guid gemeenteId)
            => new MunicipalityLatestItem
            {
                MunicipalityId = gemeenteId,
                NisCode = original.NisCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                Version = original.Version
            };

        public static MunicipalityLatestItem WithGemeenteNaam(
            this MunicipalityLatestItem original,
            string gemeenteNaam)
            => new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = original.NisCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = gemeenteNaam,
                NameDutchSearch = gemeenteNaam.RemoveDiacritics(),
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                Version = original.Version
            };

        public static StreetNameLatestItem WithGemeenteId(
            this StreetNameLatestItem original,
            string nisCode)
            => new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = nisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };

        public static StreetNameLatestItem WithPersistentLocalId(
            this StreetNameLatestItem original,
            string persistentLocalId)
            => new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                PersistentLocalId = persistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };

        public static StreetNameLatestItem WithStraatNaamId
            (this StreetNameLatestItem original,
            Guid straatnaamId)
            => new StreetNameLatestItem
            {
                StreetNameId = straatnaamId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };

        public static StreetNameLatestItem WithStraatNaamVersion(
            this StreetNameLatestItem original,
            string version)
            => new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = version,
            };

        public static StreetNameLatestItem WithStraatNaam(
            this StreetNameLatestItem original,
            string straatNaam)
            => new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = original.NisCode,
                NameDutch = straatNaam,
                NameDutchSearch = straatNaam.RemoveDiacritics(),
                NameFrench = original.NameFrench,
                NameFrenchSearch = original.NameFrenchSearch,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };

        public static AddressDetailItem WithStraatNaamId(
            this AddressDetailItem original,
            Guid straatNaamId)
            => new AddressDetailItem
            {
                AddressId = original.AddressId,
                StreetNameId = straatNaamId,
                HouseNumber = original.HouseNumber,
                BoxNumber = original.BoxNumber,
                PostalCode = original.PostalCode,
                PersistentLocalId = original.PersistentLocalId,
                Complete = original.Complete,
                Status = original.Status,
                Position = original.Position,
                PositionMethod = original.PositionMethod,
                PositionSpecification = original.PositionSpecification,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static AddressDetailItem WithPersistentLocalId(
            this AddressDetailItem original,
            int persistentLocalId)
            => new AddressDetailItem
            {
                AddressId = original.AddressId,
                StreetNameId = original.StreetNameId,
                HouseNumber = original.HouseNumber,
                BoxNumber = original.BoxNumber,
                PostalCode = original.PostalCode,
                PersistentLocalId = persistentLocalId,
                Complete = original.Complete,
                Status = original.Status,
                Position = original.Position,
                PositionMethod = original.PositionMethod,
                PositionSpecification = original.PositionSpecification,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static AddressDetailItem WithHuisNummer(
            this AddressDetailItem original,
            string huisNummer)
            => new AddressDetailItem
            {
                AddressId = original.AddressId,
                StreetNameId = original.StreetNameId,
                HouseNumber = huisNummer,
                BoxNumber = original.BoxNumber,
                PostalCode = original.PostalCode,
                PersistentLocalId = original.PersistentLocalId,
                Complete = original.Complete,
                Status = original.Status,
                Position = original.Position,
                PositionMethod = original.PositionMethod,
                PositionSpecification = original.PositionSpecification,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static AddressDetailItem WithBusnummer(
            this AddressDetailItem original,
            string busNummer)
            => new AddressDetailItem
            {
                AddressId = original.AddressId,
                StreetNameId = original.StreetNameId,
                HouseNumber = original.HouseNumber,
                BoxNumber = busNummer,
                PostalCode = original.PostalCode,
                PersistentLocalId = original.PersistentLocalId,
                Complete = original.Complete,
                Status = original.Status,
                Position = original.Position,
                PositionMethod = original.PositionMethod,
                PositionSpecification = original.PositionSpecification,
                VersionTimestamp = original.VersionTimestamp,
            };

        public static AddressDetailItem WithVersion(
            this AddressDetailItem original,
            Instant version)
            => new AddressDetailItem
            {
                AddressId = original.AddressId,
                StreetNameId = original.StreetNameId,
                HouseNumber = original.HouseNumber,
                PostalCode = original.PostalCode,
                BoxNumber = original.BoxNumber,
                PersistentLocalId = original.PersistentLocalId,
                Complete = original.Complete,
                Status = original.Status,
                Position = original.Position,
                PositionMethod = original.PositionMethod,
                PositionSpecification = original.PositionSpecification,
                VersionTimestamp = version,
            };

        public static AddressDetailItem WithTblPostKanton(
            this AddressDetailItem original,
            string postcode)
            => new AddressDetailItem
            {
                AddressId = original.AddressId,
                StreetNameId = original.StreetNameId,
                HouseNumber = original.HouseNumber,
                BoxNumber = original.BoxNumber,
                PostalCode = postcode,
                PersistentLocalId = original.PersistentLocalId,
                Complete = original.Complete,
                Status = original.Status,
                Position = original.Position,
                PositionMethod = original.PositionMethod,
                PositionSpecification = original.PositionSpecification,
                VersionTimestamp = original.VersionTimestamp
            };

        public static PostalInfoLatestItem WithPostcode(
            this PostalInfoLatestItem original,
            string postcode)
            => new PostalInfoLatestItem
            {
                PostalCode = postcode,
                Version = original.Version,
                PostalNames = original.PostalNames,
                NisCode = original.NisCode,
            };

        public static PostalInfoLatestItem WithNisCode(
            this PostalInfoLatestItem original,
            string nisCode)
            => new PostalInfoLatestItem
            {
                PostalCode = original.PostalCode,
                Version = original.Version,
                PostalNames = original.PostalNames,
                NisCode = nisCode,
            };

        public static PostalInfoLatestItem WithPostnamen(
            this PostalInfoLatestItem original,
            IEnumerable<PostalInfoPostalName> postnamen)
            => new PostalInfoLatestItem
            {
                PostalCode = original.PostalCode,
                Version = original.Version,
                PostalNames = postnamen.ToList(),
                NisCode = original.NisCode,
            };

        public static PostalInfoLatestItem WithPostnaam(
            this PostalInfoLatestItem original,
            string postnaam)
            => new PostalInfoLatestItem
            {
                PostalCode = original.PostalCode,
                Version = original.Version,
                PostalNames = new List<PostalInfoPostalName>
                {
                    new PostalInfoPostalName
                    {
                        Language = Taal.NL,
                        PostalCode = original.PostalCode,
                        PostalName = postnaam
                    }
                },
                NisCode = original.NisCode,
            };
    }
}
