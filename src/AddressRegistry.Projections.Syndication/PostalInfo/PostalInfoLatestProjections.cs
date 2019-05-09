namespace AddressRegistry.Projections.Syndication.PostalInfo
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.PostInfo;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class PostalInfoLatestProjections : AtomEntryProjectionHandlerModule<PostalInfoEvent, PostalInfo, SyndicationContext>
    {
        public PostalInfoLatestProjections()
        {
            When(PostalInfoEvent.PostalInformationWasRegistered, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationBecameCurrent, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationWasRetired, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationPostalNameWasAdded, AddSyndicationItemEntry);
            When(PostalInfoEvent.PostalInformationPostalNameWasRemoved, AddSyndicationItemEntry);
            When(PostalInfoEvent.MunicipalityWasLinkedToPostalInformation, AddSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<PostalInfo> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .PostalInfoLatestItems
                .FindAsync(entry.Content.PostalCode);

            if (latestItem == null)
            {
                latestItem = new PostalInfoLatestItem
                {
                    PostalCode = entry.Content.PostalCode,
                    Version = entry.Content.Identificator?.Versie.Value,
                    Position = long.Parse(entry.FeedEntry.Id),
                    NisCode = entry.Content.MunicipalityOsloId,
                };

                UpdateNames(latestItem, entry.Content.PostalNames);

                await context
                    .PostalInfoLatestItems
                    .AddAsync(latestItem, ct);
            }
            else
            {
                await context.Entry(latestItem).Collection(x => x.PostalNames).LoadAsync(ct);

                latestItem.Version = entry.Content.Identificator?.Versie.Value;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.NisCode = entry.Content.MunicipalityOsloId;

                UpdateNames(latestItem, entry.Content.PostalNames);
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
