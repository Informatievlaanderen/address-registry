namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Generate
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using NodaTime;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Modifiers
    {
        public static MunicipalityLatestItem WithNisGemeenteCode(this MunicipalityLatestItem original, string nisGemeenteCode)
        {
            return new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = nisGemeenteCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrenchSearch = original.NameFrenchSearch,
                NameFrench = original.NameFrench,
                Version = original.Version
            };
        }

        public static MunicipalityLatestItem WithGemeenteVersion(this MunicipalityLatestItem original, string version)
        {
            return new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = original.NisCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrenchSearch = original.NameFrenchSearch,
                NameFrench = original.NameFrench,
                Version = version
            };
        }

        public static MunicipalityLatestItem WithGemeenteId(this MunicipalityLatestItem original, Guid gemeenteId)
        {
            return new MunicipalityLatestItem
            {
                MunicipalityId = gemeenteId,
                NisCode = original.NisCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = original.NameDutch,
                NameDutchSearch = original.NameDutchSearch,
                NameFrenchSearch = original.NameFrenchSearch,
                NameFrench = original.NameFrench,
                Version = original.Version
            };
        }

        public static MunicipalityLatestItem WithGemeenteNaam(this MunicipalityLatestItem original, string gemeenteNaam)
        {
            return new MunicipalityLatestItem
            {
                MunicipalityId = original.MunicipalityId,
                NisCode = original.NisCode,
                PrimaryLanguage = original.PrimaryLanguage,
                NameDutch = gemeenteNaam,
                NameDutchSearch = gemeenteNaam.ToLowerInvariant(),
                NameFrenchSearch = original.NameFrenchSearch,
                NameFrench = original.NameFrench,
                Version = original.Version
            };
        }

        public static StreetNameLatestItem WithGemeenteId(this StreetNameLatestItem original, string nisCode)
        {
            return new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = nisCode,
                NameDutch = original.NameDutch,
                NameFrench = original.NameFrench,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };
        }

        public static StreetNameLatestItem WithPersistentLocalId(this StreetNameLatestItem original, string persistentLocalId)
        {
            return new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameFrench = original.NameFrench,
                PersistentLocalId = persistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };
        }

        public static StreetNameLatestItem WithStraatNaamId(this StreetNameLatestItem original, Guid straatnaamId)
        {
            return new StreetNameLatestItem
            {
                StreetNameId = straatnaamId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameFrench = original.NameFrench,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };
        }

        public static StreetNameLatestItem WithStraatNaamVersion(this StreetNameLatestItem original, string version)
        {
            return new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = original.NisCode,
                NameDutch = original.NameDutch,
                NameFrench = original.NameFrench,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = version,
            };
        }

        public static StreetNameLatestItem WithStraatNaam(this StreetNameLatestItem original, string straatNaam)
        {
            return new StreetNameLatestItem
            {
                StreetNameId = original.StreetNameId,
                NisCode = original.NisCode,
                NameDutch = straatNaam,
                NameFrench = original.NameFrench,
                PersistentLocalId = original.PersistentLocalId,
                IsComplete = original.IsComplete,
                Version = original.Version,
            };
        }

        public static AddressDetailItem WithStraatNaamId(this AddressDetailItem original, Guid straatNaamId)
        {
            return new AddressDetailItem
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
        }

        public static AddressDetailItem WithPersistentLocalId(this AddressDetailItem original, int persistentLocalId)
        {
            return new AddressDetailItem
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
        }

        public static AddressDetailItem WithHuisNummer(this AddressDetailItem original, string huisNummer)
        {
            return new AddressDetailItem
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
        }

        public static AddressDetailItem WithBusnummer(this AddressDetailItem original, string busNummer)
        {
            return new AddressDetailItem
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
        }

        public static AddressDetailItem WithVersion(this AddressDetailItem original, Instant version)
        {
            return new AddressDetailItem
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
        }

        public static AddressDetailItem WithTblPostKanton(this AddressDetailItem original, string postcode)
        {
            return new AddressDetailItem
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
        }

        //public static AdresMappingQueryResult WithCrabHuisnummerID(this AdresMappingQueryResult original, int crabHuisnummerID)
        //{
        //    return new AdresMappingQueryResult
        //    {
        //        CrabHuisnummerID = crabHuisnummerID,
        //        CrabSubadresID = original.CrabSubadresID
        //    };
        //}

        public static PostalInfoLatestItem WithPostcode(this PostalInfoLatestItem original, string postcode)
        {
            return new PostalInfoLatestItem
            {
                PostalCode = postcode,
                Version = original.Version,
                PostalNames = original.PostalNames,
                NisCode = original.NisCode,
            };
        }

        public static PostalInfoLatestItem WithNISCode(this PostalInfoLatestItem original, string nisCode)
        {
            return new PostalInfoLatestItem
            {
                PostalCode = original.PostalCode,
                Version = original.Version,
                PostalNames = original.PostalNames,
                NisCode = nisCode,
            };
        }

        public static PostalInfoLatestItem WithPostnamen(this PostalInfoLatestItem original, IEnumerable<PostalInfoPostalName> postnamen)
        {
            return new PostalInfoLatestItem
            {
                PostalCode = original.PostalCode,
                Version = original.Version,
                PostalNames = postnamen.ToList(),
                NisCode = original.NisCode,
            };
        }

        public static PostalInfoLatestItem WithPostnaam(this PostalInfoLatestItem original, string postnaam)
        {
            return new PostalInfoLatestItem
            {
                PostalCode = original.PostalCode,
                Version = original.Version,
                PostalNames = new List<PostalInfoPostalName> { new PostalInfoPostalName { Language = Taal.NL, PostalCode = original.PostalCode, PostalName = postnaam } },
                NisCode = original.NisCode,
            };
        }
    }
}
