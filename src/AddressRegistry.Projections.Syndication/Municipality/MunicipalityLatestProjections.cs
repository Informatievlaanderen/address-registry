namespace AddressRegistry.Projections.Syndication.Municipality
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class MunicipalityLatestProjections : AtomEntryProjectionHandlerModule<MunicipalityEvent, SyndicationItem<Municipality>, SyndicationContext>
    {
        public MunicipalityLatestProjections()
        {
            When(MunicipalityEvent.MunicipalityWasRegistered, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNisCodeWasDefined, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNisCodeWasCorrected, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityWasNamed, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCleared, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCorrected, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCorrectedToCleared, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityOfficialLanguageWasAdded, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityOfficialLanguageWasRemoved, AddSyndicationItemEntry);

            When(MunicipalityEvent.MunicipalityBecameCurrent, DoNothing);
            When(MunicipalityEvent.MunicipalityWasCorrectedToCurrent, DoNothing);
            When(MunicipalityEvent.MunicipalityWasRetired, DoNothing);
            When(MunicipalityEvent.MunicipalityWasCorrectedToRetired, DoNothing);
            When(MunicipalityEvent.MunicipalityFacilityLanguageWasAdded, DoNothing);
            When(MunicipalityEvent.MunicipalityFacilityLanguageWasRemoved, DoNothing);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Municipality>> entry, SyndicationContext context, CancellationToken ct)
        {
            var municipalityLatestItem = await context
                .MunicipalityLatestItems
                .FindAsync(entry.Content.Object.Id);

            if (municipalityLatestItem == null)
            {
                municipalityLatestItem = new MunicipalityLatestItem
                {
                    MunicipalityId = entry.Content.Object.Id,
                    NisCode = entry.Content.Object.Identificator?.ObjectId,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PrimaryLanguage = entry.Content.Object.OfficialLanguages.FirstOrDefault()
                };

                UpdateNamesByGemeentenamen(municipalityLatestItem, entry.Content.Object.MunicipalityNames);

                await context
                    .MunicipalityLatestItems
                    .AddAsync(municipalityLatestItem, ct);
            }
            else
            {
                municipalityLatestItem.NisCode = entry.Content.Object.Identificator?.ObjectId;
                municipalityLatestItem.Version = entry.Content.Object.Identificator?.Versie;
                municipalityLatestItem.Position = long.Parse(entry.FeedEntry.Id);
                municipalityLatestItem.PrimaryLanguage = entry.Content.Object.OfficialLanguages.FirstOrDefault();

                UpdateNamesByGemeentenamen(municipalityLatestItem, entry.Content.Object.MunicipalityNames);
            }
        }

        private static void UpdateNamesByGemeentenamen(MunicipalityLatestItem syndicationItem, IReadOnlyCollection<GeografischeNaam> gemeentenamen)
        {
            if (gemeentenamen == null || !gemeentenamen.Any())
            {
                return;
            }

            foreach (var naam in gemeentenamen)
            {
                switch (naam.Taal)
                {
                    default:
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

        private static Task DoNothing(AtomEntry<SyndicationItem<Municipality>> entry, SyndicationContext context, CancellationToken ct) => Task.CompletedTask;
    }
}
