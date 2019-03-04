namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using StreetName;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AddressExtractStreetNameProjection : AtomEntryProjectionHandlerModule<StreetNameEvent, StreetName, ExtractContext>
    {
        public AddressExtractStreetNameProjection()
        {
            When(StreetNameEvent.StreetNameOsloIdWasAssigned, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameNameWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameNameWasNamed, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNamePrimaryLanguageWasCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePrimaryLanguageWasCorrected, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePrimaryLanguageWasCorrectedToCleared, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNamePrimaryLanguageWasDefined, AddSyndicationItemEntry);

            When(StreetNameEvent.StreetNameWasRegistered, AddSyndicationItemEntry);
            When(StreetNameEvent.StreetNameWasRemoved, AddSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<StreetName> entry, ExtractContext context, CancellationToken ct)
        {
            var latestItem = await context
                .AddressExtractStreetNames
                .FindAsync(entry.Content.StreetNameId);

            if (latestItem == null)
            {
                latestItem = new AddressExtractStreetName
                {
                    StreetNameId = entry.Content.StreetNameId,
                    NisCode = entry.Content.NisCode,
                    Version = entry.Content.Identificator?.Versie.Value,
                    Position = long.Parse(entry.FeedEntry.Id),
                    OsloId = entry.Content.Identificator?.ObjectId,
                };

                UpdateNames(latestItem, entry.Content.StreetNames);

                await context
                    .AddressExtractStreetNames
                    .AddAsync(latestItem, ct);
            }
            else
            {
                latestItem.NisCode = entry.Content.NisCode;
                latestItem.Version = entry.Content.Identificator?.Versie.Value;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.OsloId = entry.Content.Identificator?.ObjectId;

                UpdateNames(latestItem, entry.Content.StreetNames);
            }

            UpdateExtracts(entry, context);
        }

        private static void UpdateExtracts(AtomEntry<StreetName> entry, ExtractContext context)
        {
            //context.
        }

        private static void UpdateNames(AddressExtractStreetName syndicationItem, IReadOnlyCollection<GeografischeNaam> streetNames)
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
    }
}
