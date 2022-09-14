namespace AddressRegistry.Api.Legacy.AddressMatch.Mappers
{
    using SyndicationStreetNameLatestItem = Projections.Syndication.StreetName.StreetNameLatestItem;
    using ConsumerStreetNameLatestItem = Consumer.Read.StreetName.Projections.StreetNameLatestItem;

    public static class StreetNameLatestItemMapper
    {
        public static SyndicationStreetNameLatestItem Map(this ConsumerStreetNameLatestItem streetNameLatestItem)
        {
            return new SyndicationStreetNameLatestItem
            {
                // StreetNameId = streetNameLatestItem.
                PersistentLocalId = streetNameLatestItem.PersistentLocalId.ToString(),
                NisCode = streetNameLatestItem.NisCode,
                NameDutch = streetNameLatestItem.NameDutch,
                NameDutchSearch = streetNameLatestItem.NameDutchSearch,
                NameFrench = streetNameLatestItem.NameFrench,
                NameFrenchSearch = streetNameLatestItem.NameFrenchSearch,
                NameGerman = streetNameLatestItem.NameGerman,
                NameGermanSearch = streetNameLatestItem.NameGermanSearch,
                NameEnglish = streetNameLatestItem.NameEnglish,
                NameEnglishSearch = streetNameLatestItem.NameEnglishSearch,
                HomonymAdditionDutch = streetNameLatestItem.HomonymAdditionDutch,
                HomonymAdditionFrench = streetNameLatestItem.HomonymAdditionFrench,
                HomonymAdditionGerman = streetNameLatestItem.HomonymAdditionGerman,
                HomonymAdditionEnglish = streetNameLatestItem.HomonymAdditionEnglish,
                // Version =
                // Position =
                IsComplete = true,
                IsRemoved = streetNameLatestItem.IsRemoved
            };
        }
    }
}
