namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using NodaTime.Text;
    using Postal;

    [ConnectedProjectionName("API endpoint search straatnamen")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor adres zoeken in Elastic Search synchroniseert.")]
    public class StreetNameSearchProjections : ConnectedProjection<StreetNameConsumerContext>
    {
        private readonly Dictionary<Guid, Municipality> _municipalities = new Dictionary<Guid, Municipality>();
        private readonly IDictionary<string, PostalInfo[]> _postalInfos = new Dictionary<string, PostalInfo[]>();

        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;

        public StreetNameSearchProjections(
            IStreetNameElasticsearchClient elasticsearchClient,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory)
        {
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;
            _postalConsumerContextFactory = postalConsumerContextFactory;

            When<StreetNameWasMigratedToMunicipality>(async (_, message, ct) =>
            {
                if(message.IsRemoved)
                    return;

                var municipality = await GetMunicipality(Guid.Parse(message.MunicipalityId), ct);

                var document = new StreetNameSearchDocument(
                    message.PersistentLocalId,
                    InstantPattern.General.Parse(message.Provenance.Timestamp).Value.ToBelgianDateTimeOffset(),
                    StreetNameLatestItem.ConvertStringToStatus(message.Status),
                    municipality,
                    await GetPostalInfo(municipality.NisCode, ct),
                    message.Names.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray(),
                    message.HomonymAdditions.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray());

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<StreetNameWasProposedV2>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(Guid.Parse(message.MunicipalityId), ct);

                var document = new StreetNameSearchDocument(
                    message.PersistentLocalId,
                    InstantPattern.General.Parse(message.Provenance.Timestamp).Value.ToBelgianDateTimeOffset(),
                    StreetNameStatus.Proposed,
                    municipality,
                    await GetPostalInfo(municipality.NisCode, ct),
                    message.StreetNameNames.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray());

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<StreetNameWasProposedForMunicipalityMerger>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(Guid.Parse(message.MunicipalityId), ct);

                var document = new StreetNameSearchDocument(
                    message.PersistentLocalId,
                    InstantPattern.General.Parse(message.Provenance.Timestamp).Value.ToBelgianDateTimeOffset(),
                    StreetNameStatus.Proposed,
                    municipality,
                    await GetPostalInfo(municipality.NisCode, ct),
                    message.StreetNameNames.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray(),
                    message.HomonymAdditions.Select(x => new Name(x.Value, MapToLanguage(x.Key))).ToArray());

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<StreetNameWasApproved>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Current,
                    }, ct);
            });

            When<StreetNameWasCorrectedFromApprovedToProposed>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Proposed
                    }, ct);
            });

            When<StreetNameWasRejected>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Rejected
                    }, ct);
            });

            When<StreetNameWasRejectedBecauseOfMunicipalityMerger>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Rejected
                    }, ct);
            });

            When<StreetNameWasCorrectedFromRejectedToProposed>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Proposed
                    }, ct);
            });

            When<StreetNameWasRetiredV2>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Retired
                    }, ct);
            });

            When<StreetNameWasRetiredBecauseOfMunicipalityMerger>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Retired
                    }, ct);
            });

            When<StreetNameWasRenamed>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Retired
                    }, ct);
            });

            When<StreetNameWasCorrectedFromRetiredToCurrent>(async (_, message, ct) =>
            {
                await elasticsearchClient.PartialUpdateDocument(message.PersistentLocalId,
                    new StreetNameSearchPartialDocument(InstantPattern.General.Parse(message.Provenance.Timestamp).Value)
                    {
                        Status = StreetNameStatus.Rejected
                    }, ct);
            });

            When<StreetNameNamesWereCorrected>(async (_, message, ct) =>
            {
                var document = await elasticsearchClient.GetDocument(message.PersistentLocalId, ct);
                UpdateNames(document, message.StreetNameNames);
                UpdateVersion(document, message.Provenance.Timestamp);

                await elasticsearchClient.UpdateDocument(document, ct);
            });

            When<StreetNameNamesWereChanged>(async (_, message, ct) =>
            {
                var document = await elasticsearchClient.GetDocument(message.PersistentLocalId, ct);
                UpdateNames(document, message.StreetNameNames);
                UpdateVersion(document, message.Provenance.Timestamp);

                await elasticsearchClient.UpdateDocument(document, ct);
            });

            When<StreetNameHomonymAdditionsWereCorrected>(async (_, message, ct) =>
            {
                var document = await elasticsearchClient.GetDocument(message.PersistentLocalId, ct);
                UpdateHomonyms(document, message.HomonymAdditions);
                UpdateVersion(document, message.Provenance.Timestamp);

                await elasticsearchClient.UpdateDocument(document, ct);
            });

            When<StreetNameHomonymAdditionsWereRemoved>(async (_, message, ct) =>
            {
                var document = await elasticsearchClient.GetDocument(message.PersistentLocalId, ct);
                foreach (var language in message.Languages)
                {
                    document.HomonymAdditions = document
                        .HomonymAdditions
                        .Where(x => x.Language != MapToLanguage(language))
                        .ToArray();
                }
                UpdateVersion(document, message.Provenance.Timestamp);

                await elasticsearchClient.UpdateDocument(document, ct);
            });

            When<StreetNameWasRemovedV2>(async (_, message, ct) =>
            {
                await elasticsearchClient.DeleteDocument(message.PersistentLocalId, ct);
            });
        }

        private static void UpdateVersion(StreetNameSearchDocument document, string timestamp)
            => document.VersionTimestamp = InstantPattern.General.Parse(timestamp).Value.ToBelgianDateTimeOffset();

        private static void UpdateHomonyms(StreetNameSearchDocument item, IDictionary<string, string> homonyms)
        {
            var caseInsensitiveDict = new Dictionary<string, string>(homonyms, StringComparer.OrdinalIgnoreCase);

            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.Dutch, out var homonym))
            {
                item.HomonymAdditions.Single(x => x.Language == Language.nl).Spelling = homonym;
            }
            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.French, out homonym))
            {
                item.HomonymAdditions.Single(x => x.Language == Language.fr).Spelling = homonym;
            }
            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.German, out homonym))
            {
                item.HomonymAdditions.Single(x => x.Language == Language.de).Spelling = homonym;
            }
            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.English, out homonym))
            {
                item.HomonymAdditions.Single(x => x.Language == Language.en).Spelling = homonym;
            }
        }

        private static void UpdateNames(StreetNameSearchDocument item, IDictionary<string, string> names)
        {
            var caseInsensitiveDict = new Dictionary<string, string>(names, StringComparer.OrdinalIgnoreCase);

            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.Dutch, out var name))
            {
                UpdateOrAddName(item, Language.nl, name);
            }
            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.French, out name))
            {
                UpdateOrAddName(item, Language.fr, name);
            }
            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.German, out name))
            {
                UpdateOrAddName(item, Language.de, name);
            }
            if (caseInsensitiveDict.TryGetValue(StreetNameLatestItemProjections.English, out name))
            {
                UpdateOrAddName(item, Language.en, name);
            }
        }

        private static void UpdateOrAddName(StreetNameSearchDocument item, Language language, string spelling)
        {
            var name = item.Names.SingleOrDefault(x => x.Language == language);
            if (name is null)
            {
                name = new Name(spelling, language);

                item.Names = item.Names
                    .Concat([name])
                    .ToArray();
            }

            name.Spelling = spelling;
        }

        public static Language MapToLanguage(string language)
        {
            if (string.Equals(language, StreetNameLatestItemProjections.Dutch, StringComparison.OrdinalIgnoreCase))
                return Language.nl;
            if (string.Equals(language, StreetNameLatestItemProjections.French, StringComparison.OrdinalIgnoreCase))
                return Language.fr;
            if (string.Equals(language, StreetNameLatestItemProjections.German, StringComparison.OrdinalIgnoreCase))
                return Language.de;
            if (string.Equals(language, StreetNameLatestItemProjections.English, StringComparison.OrdinalIgnoreCase))
                return Language.en;

            throw new ArgumentOutOfRangeException(nameof(language), language, null);
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

        private async Task<PostalInfo[]> GetPostalInfo(string nisCode, CancellationToken ct)
        {
            if (_postalInfos.TryGetValue(nisCode, out var value))
                return value;

            await using var context = await _postalConsumerContextFactory.CreateDbContextAsync(ct);
            var postalInfoLatestItems = await context.PostalLatestItems
                .Include(x => x.PostalNames)
                .AsNoTracking()
                .Where(p => p.NisCode == nisCode)
                .ToListAsync(ct);

            if (postalInfoLatestItems.Count == 0)
                _postalInfos.Add(nisCode, []);
            else
                _postalInfos.Add(nisCode, postalInfoLatestItems.Select(PostalInfo.FromPostalLatestItem).ToArray());

            return _postalInfos[nisCode];
        }
    }
}
