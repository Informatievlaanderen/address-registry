namespace AddressRegistry.Api.Legacy.AddressMatch.Mappers
{
    using Convertors;
    using SyndicationMunicipalityLatestItem = Projections.Syndication.Municipality.MunicipalityLatestItem;
    using ConsumerMunicipalityLatestItem = Consumer.Read.Municipality.Projections.MunicipalityLatestItem;

    public static class MunicipalityLatestItemMapper
    {
        public static SyndicationMunicipalityLatestItem Map(this ConsumerMunicipalityLatestItem municipalityLatestItem)
        {
            return new SyndicationMunicipalityLatestItem
            {
                MunicipalityId = municipalityLatestItem.MunicipalityId,
                NisCode = municipalityLatestItem.NisCode,
                NameDutch = municipalityLatestItem.NameDutch,
                NameDutchSearch = municipalityLatestItem.NameDutchSearch,
                NameFrench = municipalityLatestItem.NameFrench,
                NameFrenchSearch = municipalityLatestItem.NameFrenchSearch,
                NameGerman = municipalityLatestItem.NameGerman,
                NameGermanSearch = municipalityLatestItem.NameGermanSearch,
                NameEnglish = municipalityLatestItem.NameEnglish,
                NameEnglishSearch = municipalityLatestItem.NameEnglishSearch,
                PrimaryLanguage = municipalityLatestItem.PrimaryLanguage.ToTaal(),
                // Version =
                // Position =
            };
        }
    }
}
