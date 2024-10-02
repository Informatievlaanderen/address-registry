namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.IO;
    using NodaTime;
    using NodaTime.Text;

    [ConnectedProjectionName("API endpoint search straatnamen")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor adres zoeken in Elastic Search synchroniseert.")]
    public class StreetNameSearchProjections : ConnectedProjection<StreetNameConsumerContext>
    {
        private readonly Dictionary<Guid, Municipality> _municipalities = new Dictionary<Guid, Municipality>();

        private readonly IStreetNameElasticsearchClient _elasticsearchClient;
        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;

        public StreetNameSearchProjections(
            IStreetNameElasticsearchClient elasticsearchClient,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory)
        {
            _elasticsearchClient = elasticsearchClient;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;

            When<StreetNameWasMigratedToMunicipality>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(Guid.Parse(message.MunicipalityId), ct);

                var document = new StreetNameSearchDocument(
                    message.PersistentLocalId,
                    InstantPattern.General.Parse(message.Provenance.Timestamp).Value.ToBelgianDateTimeOffset(),
                    StreetNameLatestItem.ConvertStringToStatus(message.Status),
                    municipality,
                    message.Names.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray(),
                    message.HomonymAdditions.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray());

                await elasticsearchClient.CreateDocument(document, ct);
            });
        }

        private static Language MapToLanguage(string language)
        {
            if (language == StreetNameLatestItemProjections.Dutch)
                return Language.nl;
            if (language == StreetNameLatestItemProjections.French)
                return Language.fr;
            if (language == StreetNameLatestItemProjections.German)
                return Language.de;
            if (language == StreetNameLatestItemProjections.English)
                return Language.en;

            throw new ArgumentOutOfRangeException(nameof(language), language, null);
        }

        private async Task UpdateDocuments(
            ICollection<int> streetNamePersistentLocalIds,
            Action<StreetNameSearchDocument> update,
            Instant versionTimestamp,
            CancellationToken ct)
        {
            var documents = await _elasticsearchClient.GetDocuments(streetNamePersistentLocalIds, ct);

            foreach (var persistentLocalId in streetNamePersistentLocalIds)
            {
                var document = documents.SingleOrDefault(x => x.StreetNamePersistentLocalId == persistentLocalId);
                if (document is null)
                    throw new NullReferenceException($"No document received for {persistentLocalId}");

                update(document);

                document.VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();

                await _elasticsearchClient.UpdateDocument(document, ct);
            }
        }

        private async Task<Municipality> GetMunicipality(Guid municipalityId, CancellationToken ct)
        {
            if (_municipalities.TryGetValue(municipalityId, out var value))
                return value;

            await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync(ct);
            var municipalityLatestItem = await context.MunicipalityLatestItems
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.MunicipalityId == municipalityId, ct);

            if (municipalityLatestItem == null)
                throw new InvalidOperationException($"Municipality with MunicipalityId {municipalityId} not found");

            _municipalities.Add(municipalityId, Municipality.FromMunicipalityLatestItem(municipalityLatestItem));

            return _municipalities[municipalityId];
        }
    }
}
