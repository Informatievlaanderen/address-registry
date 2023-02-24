namespace AddressRegistry.Consumer.Read.StreetName.Projections
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using NodaTime.Text;

    public class StreetNameBosaItemProjections : ConnectedProjection<StreetNameConsumerContext>
    {
        public static string Dutch => StreetNameLanguage.Dutch.ToString();
        public static string French => StreetNameLanguage.French.ToString();
        public static string German => StreetNameLanguage.German.ToString();
        public static string English => StreetNameLanguage.English.ToString();

        public StreetNameBosaItemProjections()
        {
            When<StreetNameWasMigratedToMunicipality>(async (context, message, ct) =>
            {
                var item = new StreetNameBosaItem(message.PersistentLocalId, message.NisCode);
                item.Status = StreetNameBosaItem.ConvertStringToStatus(message.Status);
                item.IsRemoved = message.IsRemoved;

                UpdateNames(item, message.Names);
                UpdateHomonyms(item, message.HomonymAdditions);
                UpdateVersionTimestamp(item, message.Provenance.Timestamp);

                await context.StreetNameBosaItems.AddAsync(item, ct);
            });

            When<StreetNameWasProposedV2>(async (context, message, ct) =>
            {
                var item = new StreetNameBosaItem(message.PersistentLocalId, message.NisCode);
                item.Status = StreetNameStatus.Proposed;

                UpdateNames(item, message.StreetNameNames);
                UpdateVersionTimestamp(item, message.Provenance.Timestamp);

                await context.StreetNameBosaItems.AddAsync(item, ct);
            });

            When<StreetNameWasApproved>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Current;
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameWasCorrectedFromApprovedToProposed>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Proposed;
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameWasRejected>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Rejected;
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameWasCorrectedFromRejectedToProposed>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Proposed;
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });


            When<StreetNameWasRetiredV2>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Retired;
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameWasCorrectedFromRetiredToCurrent>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    item.Status = StreetNameStatus.Current;
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameNamesWereCorrected>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    UpdateNames(item, message.StreetNameNames);
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameHomonymAdditionsWereCorrected>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    UpdateHomonyms(item, message.HomonymAdditions);
                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });

            When<StreetNameHomonymAdditionsWereRemoved>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBosaItem(message.PersistentLocalId, item =>
                {
                    foreach (var language in message.Languages)
                    {
                        switch (language)
                        {
                            case nameof(StreetNameLanguage.Dutch):item.HomonymAdditionDutch = null;
                                break;
                            case nameof(StreetNameLanguage.French): item.HomonymAdditionFrench = null;
                                 break;
                            case nameof(StreetNameLanguage.German): item.HomonymAdditionGerman = null;
                                break;
                            case nameof(StreetNameLanguage.English): item.HomonymAdditionEnglish = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    UpdateVersionTimestamp(item, message.Provenance.Timestamp);
                }, ct);
            });
        }

        private void UpdateNames(StreetNameBosaItem item, IDictionary<string, string> names)
        {
            var caseInsensitiveDict = new Dictionary<string, string>(names, StringComparer.OrdinalIgnoreCase);

            if (caseInsensitiveDict.TryGetValue(Dutch, out var name))
            {
                item.NameDutch = name;
                item.NameDutchSearch = name.SanitizeForBosaSearch();
            }
            if (caseInsensitiveDict.TryGetValue(French, out name))
            {
                item.NameFrench = name;
                item.NameFrenchSearch = name.SanitizeForBosaSearch();
            }
            if (caseInsensitiveDict.TryGetValue(German, out name))
            {
                item.NameGerman = name;
                item.NameGermanSearch = name.SanitizeForBosaSearch();
            }
            if (caseInsensitiveDict.TryGetValue(English, out name))
            {
                item.NameEnglish = name;
                item.NameEnglishSearch = name.SanitizeForBosaSearch();
            }
        }

        private void UpdateHomonyms(StreetNameBosaItem item, IDictionary<string, string> homonyms)
        {
            var caseInsensitiveDict = new Dictionary<string, string>(homonyms, StringComparer.OrdinalIgnoreCase);

            if (caseInsensitiveDict.TryGetValue(Dutch, out var homonym))
            {
                item.HomonymAdditionDutch = homonym;
            }
            if (caseInsensitiveDict.TryGetValue(French, out homonym))
            {
                item.HomonymAdditionFrench = homonym;
            }
            if (caseInsensitiveDict.TryGetValue(German, out homonym))
            {
                item.HomonymAdditionGerman = homonym;
            }
            if (caseInsensitiveDict.TryGetValue(English, out homonym))
            {
                item.HomonymAdditionEnglish = homonym;
            }
        }

        private void UpdateVersionTimestamp(StreetNameBosaItem item, string timestamp)
        {
            item.VersionTimestamp = InstantPattern.General.Parse(timestamp).Value;
        }
    }
}
