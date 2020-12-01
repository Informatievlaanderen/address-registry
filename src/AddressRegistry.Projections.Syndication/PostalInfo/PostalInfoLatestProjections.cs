namespace AddressRegistry.Projections.Syndication.PostalInfo
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class PostalInfoLatestProjections : AtomEntryProjectionHandlerModule<PostalInfoEvent, SyndicationItem<PostalInfo>, SyndicationContext>
    {
        public PostalInfoLatestProjections()
        {
            When(PostalInfoEvent.PostalInformationWasRegistered, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationWasRealized, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationWasRetired, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationPostalNameWasAdded, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationPostalNameWasRemoved, AddSyndicationItemEntry);
            When(PostalInfoEvent.MunicipalityWasAttached, AddSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<PostalInfo>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .PostalInfoLatestItems
                .FindAsync(entry.Content.Object.PostalCode);

            if (latestItem == null)
            {
                latestItem = new PostalInfoLatestItem
                {
                    PostalCode = entry.Content.Object.PostalCode,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    NisCode = entry.Content.Object.MunicipalityNisCode,
                };

                UpdateNames(latestItem, entry.Content.Object.PostalNames);

                await context
                    .PostalInfoLatestItems
                    .AddAsync(latestItem, ct);
            }
            else
            {
                await context.Entry(latestItem).Collection(x => x.PostalNames).LoadAsync(ct);

                latestItem.Version = entry.Content.Object.Identificator?.Versie;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.NisCode = entry.Content.Object.MunicipalityNisCode;

                UpdateNames(latestItem, entry.Content.Object.PostalNames);
            }
        }

        private static void UpdateNames(PostalInfoLatestItem latestItem, IEnumerable<Postnaam> postalNames)
        {
            latestItem.PostalNames.Clear();

            foreach (var postalName in postalNames)
            {
                latestItem.PostalNames.Add(new PostalInfoPostalName
                {
                    Language = postalName.GeografischeNaam.Taal,
                    PostalCode = latestItem.PostalCode,
                    PostalName = postalName.GeografischeNaam.Spelling
                });
            }
        }
    }
}
