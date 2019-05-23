namespace AddressRegistry.Projections.Syndication.StreetName
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class StreetNameSyndicationItemProjections : AtomEntryProjectionHandlerModule<StreetNameEvent, SyndicationItem<StreetName>, SyndicationContext>
    {
        public StreetNameSyndicationItemProjections()
        {
            When(StreetNameEvent.StreetNameBecameComplete, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameBecameIncomplete, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameOsloIdWasAssigned, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameNameWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasNamed, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameHomonymAdditionWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameHomonymAdditionWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameHomonymAdditionWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameHomonymAdditionWasDefined, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNamePrimaryLanguageWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePrimaryLanguageWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePrimaryLanguageWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePrimaryLanguageWasDefined, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameSecondaryLanguageWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameSecondaryLanguageWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameSecondaryLanguageWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameSecondaryLanguageWasDefined, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameWasCorrectedToProposed, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasProposed, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasCorrectedToRetired, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasRetired, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameBecameCurrent, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasCorrectedToCurrent, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameStatusWasCorrectedToRemoved, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameWasRegistered, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasRemoved, AddSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<StreetName>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = new StreetNameSyndicationItem
            {
                StreetNameId = entry.Content.Object.StreetNameId,
                NisCode = entry.Content.Object.NisCode,
                Version = entry.Content.Object.Identificator?.Versie.Value,
                Position = long.Parse(entry.FeedEntry.Id),
                OsloId = entry.Content.Object.Identificator?.ObjectId,
            };

            UpdateNames(latestItem, entry.Content.Object.StreetNames);
            UpdateHomonymAdditions(latestItem, entry.Content.Object.HomonymAdditions);

            await context
                .StreetNameSyndicationItems
                .AddAsync(latestItem, ct);
        }

        private static void UpdateNames(StreetNameSyndicationItem syndicationItem, IReadOnlyCollection<GeografischeNaam> streetNames)
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
                        break;
                    case Taal.FR:
                        syndicationItem.NameFrench = naam.Spelling;
                        break;
                    case Taal.DE:
                        syndicationItem.NameGerman = naam.Spelling;
                        break;
                    case Taal.EN:
                        syndicationItem.NameEnglish = naam.Spelling;
                        break;
                }
            }
        }

        private static void UpdateHomonymAdditions(StreetNameSyndicationItem syndicationItem, IReadOnlyCollection<GeografischeNaam> homonymAdditions)
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
