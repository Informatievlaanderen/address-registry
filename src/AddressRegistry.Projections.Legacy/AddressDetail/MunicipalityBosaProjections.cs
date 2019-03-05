namespace AddressRegistry.Projections.Legacy.AddressDetail
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class MunicipalityBosaProjections : AtomEntryProjectionHandlerModule<MunicipalityEvent, Municipality, LegacyContext>
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
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<Municipality> entry, LegacyContext context, CancellationToken ct)
        {
            var municipalityItem = await context
                .MunicipalityBosaItems
                .FindAsync(entry.Content.Id);

            if (municipalityItem == null)
            {
                municipalityItem = new MunicipalityBosaItem
                {
                    MunicipalityId = entry.Content.Id,
                    NisCode = entry.Content.Identificator?.ObjectId,
                    Version = entry.Content.Identificator?.Versie.Value,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PrimaryLanguage = entry.Content.OfficialLanguages.FirstOrDefault(),
                    IsFlemishRegion = RegionFilter.IsFlemishRegion(entry.Content.Identificator?.ObjectId)
                };

                UpdateNamesByGemeentenamen(municipalityItem, entry.Content.MunicipalityNames);

                await context
                    .MunicipalityBosaItems
                    .AddAsync(municipalityItem, ct);
            }
            else
            {
                municipalityItem.NisCode = entry.Content.Identificator?.ObjectId;
                municipalityItem.Version = entry.Content.Identificator?.Versie.Value;
                municipalityItem.Position = long.Parse(entry.FeedEntry.Id);
                municipalityItem.PrimaryLanguage = entry.Content.OfficialLanguages.FirstOrDefault();
                municipalityItem.IsFlemishRegion = RegionFilter.IsFlemishRegion(entry.Content.Identificator?.ObjectId);

                UpdateNamesByGemeentenamen(municipalityItem, entry.Content.MunicipalityNames);
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
    }
}
