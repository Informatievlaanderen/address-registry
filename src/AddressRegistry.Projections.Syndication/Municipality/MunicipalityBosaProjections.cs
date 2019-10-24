namespace AddressRegistry.Projections.Syndication.Municipality
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class MunicipalityBosaProjections : AtomEntryProjectionHandlerModule<MunicipalityEvent, SyndicationItem<Municipality>, SyndicationContext>
    {
        public MunicipalityBosaProjections()
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
            When(MunicipalityEvent.MunicipalityFacilitiesLanguageWasAdded, DoNothing);
            When(MunicipalityEvent.MunicipalityFacilitiesLanguageWasRemoved, DoNothing);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Municipality>> entry, SyndicationContext context, CancellationToken ct)
        {
            var municipalityItem = await context
                .MunicipalityBosaItems
                .FindAsync(entry.Content.Object.Id);

            if (municipalityItem == null)
            {
                municipalityItem = new MunicipalityBosaItem
                {
                    MunicipalityId = entry.Content.Object.Id,
                    NisCode = entry.Content.Object.Identificator?.ObjectId,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PrimaryLanguage = entry.Content.Object.OfficialLanguages.FirstOrDefault(),
                    IsFlemishRegion = RegionFilter.IsFlemishRegion(entry.Content.Object.Identificator?.ObjectId)
                };

                UpdateNamesByGemeentenamen(municipalityItem, entry.Content.Object.MunicipalityNames);

                await context
                    .MunicipalityBosaItems
                    .AddAsync(municipalityItem, ct);
            }
            else
            {
                municipalityItem.NisCode = entry.Content.Object.Identificator?.ObjectId;
                municipalityItem.Version = entry.Content.Object.Identificator?.Versie;
                municipalityItem.Position = long.Parse(entry.FeedEntry.Id);
                municipalityItem.PrimaryLanguage = entry.Content.Object.OfficialLanguages.FirstOrDefault();
                municipalityItem.IsFlemishRegion = RegionFilter.IsFlemishRegion(entry.Content.Object.Identificator?.ObjectId);

                UpdateNamesByGemeentenamen(municipalityItem, entry.Content.Object.MunicipalityNames);
            }
        }

        private static void UpdateNamesByGemeentenamen(MunicipalityBosaItem syndicationItem, IReadOnlyCollection<GeografischeNaam> gemeentenamen)
        {
            if (gemeentenamen == null || !gemeentenamen.Any())
                return;

            foreach (var naam in gemeentenamen)
            {
                switch (naam.Taal)
                {
                    default:
                    case Taal.NL:
                        syndicationItem.NameDutch = naam.Spelling;
                        syndicationItem.NameDutchSearch = naam.Spelling.SanitizeForBosaSearch();
                        break;
                    case Taal.FR:
                        syndicationItem.NameFrench = naam.Spelling;
                        syndicationItem.NameFrenchSearch = naam.Spelling.SanitizeForBosaSearch();
                        break;
                    case Taal.DE:
                        syndicationItem.NameGerman = naam.Spelling;
                        syndicationItem.NameGermanSearch = naam.Spelling.SanitizeForBosaSearch();
                        break;
                    case Taal.EN:
                        syndicationItem.NameEnglish = naam.Spelling;
                        syndicationItem.NameEnglishSearch = naam.Spelling.SanitizeForBosaSearch();
                        break;
                }
            }
        }

        private static Task DoNothing(AtomEntry<SyndicationItem<Municipality>> entry, SyndicationContext context, CancellationToken ct) => Task.CompletedTask;
    }
}
