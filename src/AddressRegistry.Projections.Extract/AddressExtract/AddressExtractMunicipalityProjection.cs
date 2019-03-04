namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Municipality;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AddressExtractMunicipalityProjection : AtomEntryProjectionHandlerModule<MunicipalityEvent, Municipality, ExtractContext>
    {
        public AddressExtractMunicipalityProjection()
        {
            When(MunicipalityEvent.MunicipalityWasRegistered, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNisCodeWasDefined, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNisCodeWasCorrected, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityWasNamed, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCleared, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCorrected, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityNameWasCorrectedToCleared, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityOfficialLanuageWasAdded, AddSyndicationItemEntry);
            When(MunicipalityEvent.MunicipalityOfficialLanuageWasRemoved, AddSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<Municipality> entry, ExtractContext context, CancellationToken ct)
        {
            var municipalityLatestItem = await context
                .AddressExtractMunicipalities
                .FindAsync(entry.Content.Id);

            if (municipalityLatestItem == null)
            {
                municipalityLatestItem = new AddressExtractMunicipality
                {
                    MunicipalityId = entry.Content.Id,
                    NisCode = entry.Content.Identificator?.ObjectId,
                    Version = entry.Content.Identificator?.Versie.Value,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PrimaryLanguage = entry.Content.OfficialLanguages.FirstOrDefault()
                };

                UpdateNamesByGemeentenamen(municipalityLatestItem, entry.Content.MunicipalityNames);

                await context
                    .AddressExtractMunicipalities
                    .AddAsync(municipalityLatestItem, ct);
            }
            else
            {
                municipalityLatestItem.NisCode = entry.Content.Identificator?.ObjectId;
                municipalityLatestItem.Version = entry.Content.Identificator?.Versie.Value;
                municipalityLatestItem.Position = long.Parse(entry.FeedEntry.Id);
                municipalityLatestItem.PrimaryLanguage = entry.Content.OfficialLanguages.FirstOrDefault();

                UpdateNamesByGemeentenamen(municipalityLatestItem, entry.Content.MunicipalityNames);
            }
        }

        private static void UpdateNamesByGemeentenamen(AddressExtractMunicipality syndicationItem, IReadOnlyCollection<GeografischeNaam> gemeentenamen)
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
    }
}
