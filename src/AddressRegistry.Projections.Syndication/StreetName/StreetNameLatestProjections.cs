namespace AddressRegistry.Projections.Syndication.StreetName
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class StreetNameLatestProjections : AtomEntryProjectionHandlerModule<StreetNameEvent, SyndicationItem<StreetName>, SyndicationContext>
    {
        public StreetNameLatestProjections()
        {
            When(StreetNameEvent.StreetNameBecameComplete, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameBecameIncomplete, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePersistentLocalIdentifierWasAssigned, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameNameWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasNamed, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameHomonymAdditionWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameHomonymAdditionWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameHomonymAdditionWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameHomonymAdditionWasDefined, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameWasCorrectedToProposed, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasProposed, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasCorrectedToRetired, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasRetired, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameBecameCurrent, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasCorrectedToCurrent, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameStatusWasCorrectedToRemoved, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameStatusWasRemoved, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameWasRegistered, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasRemoved, RemoveStreetName);
        }

        private static async Task RemoveStreetName(AtomEntry<SyndicationItem<StreetName>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem =
                await context
                    .StreetNameLatestItems
                    .FindAsync(entry.Content.Object.StreetNameId);

            latestItem.IsRemoved = true;
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<StreetName>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .StreetNameLatestItems
                .FindAsync(entry.Content.Object.StreetNameId);

            if (latestItem == null)
            {
                latestItem = new StreetNameLatestItem
                {
                    StreetNameId = entry.Content.Object.StreetNameId,
                    NisCode = entry.Content.Object.NisCode,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PersistentLocalId = entry.Content.Object.Identificator?.ObjectId,
                    IsComplete = entry.Content.Object.IsComplete
                };

                UpdateNames(latestItem, entry.Content.Object.StreetNames);
                UpdateHomonymAdditions(latestItem, entry.Content.Object.HomonymAdditions);

                await context
                    .StreetNameLatestItems
                    .AddAsync(latestItem, ct);
            }
            else
            {
                latestItem.NisCode = entry.Content.Object.NisCode;
                latestItem.Version = entry.Content.Object.Identificator?.Versie;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.PersistentLocalId = entry.Content.Object.Identificator?.ObjectId;
                latestItem.IsComplete = entry.Content.Object.IsComplete;

                UpdateNames(latestItem, entry.Content.Object.StreetNames);
                UpdateHomonymAdditions(latestItem, entry.Content.Object.HomonymAdditions);
            }
        }

        private static void UpdateNames(StreetNameLatestItem syndicationItem, IReadOnlyCollection<GeografischeNaam> streetNames)
        {
            if (streetNames == null || !streetNames.Any())
                return;

            foreach (var naam in streetNames)
            {
                switch (naam.Taal)
                {
                    default:
                    case Taal.NL:
                        syndicationItem.NameDutch = naam.Spelling;
                        syndicationItem.NameDutchSearch = naam.Spelling.RemoveDiacritics();
                        break;
                    case Taal.FR:
                        syndicationItem.NameFrench = naam.Spelling;
                        syndicationItem.NameFrenchSearch = naam.Spelling.RemoveDiacritics();
                        break;
                    case Taal.DE:
                        syndicationItem.NameGerman = naam.Spelling;
                        syndicationItem.NameGermanSearch = naam.Spelling.RemoveDiacritics();
                        break;
                    case Taal.EN:
                        syndicationItem.NameEnglish = naam.Spelling;
                        syndicationItem.NameEnglishSearch = naam.Spelling.RemoveDiacritics();
                        break;
                }
            }
        }

        private static void UpdateHomonymAdditions(StreetNameLatestItem syndicationItem, IReadOnlyCollection<GeografischeNaam> homonymAdditions)
        {
            if (homonymAdditions == null || !homonymAdditions.Any())
                return;

            foreach (var naam in homonymAdditions)
            {
                switch (naam.Taal)
                {
                    default:
                    case Taal.NL:
                        syndicationItem.HomonymAdditionDutch = naam.Spelling;
                        break;
                    case Taal.FR:
                        syndicationItem.HomonymAdditionFrench = naam.Spelling;
                        break;
                    case Taal.DE:
                        syndicationItem.HomonymAdditionGerman = naam.Spelling;
                        break;
                    case Taal.EN:
                        syndicationItem.HomonymAdditionEnglish = naam.Spelling;
                        break;
                }
            }
        }
    }
}
