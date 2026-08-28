namespace AddressRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using StreetName.Events;

    [ConnectedProjectionName(ProjectionName)]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel adressen de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedListProjectionsV3 : LastChangedListConnectedProjection
    {
        public const string ProjectionName = "Cache markering adressen (v3)";
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.JsonLd };

        public LastChangedListProjectionsV3(ICacheValidator cacheValidator)
            : base(SupportedAcceptTypes, cacheValidator)
        {
            #region StreetNames

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(addressPersistentLocalId.ToString()), message.Position, context, ct);
                }
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(addressPersistentLocalId.ToString()), message.Position, context, ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(addressPersistentLocalId.ToString()), message.Position, context, ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(addressPersistentLocalId.ToString()), message.Position, context, ct);
                }
            });

            When<Envelope<MigratedStreetNameWasImported>>(DoNothing);
            When<Envelope<StreetNameWasImported>>(DoNothing);
            When<Envelope<StreetNameWasApproved>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(DoNothing);
            When<Envelope<StreetNameWasRejected>>(DoNothing);
            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<StreetNameWasRetired>>(DoNothing);
            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<StreetNameWasRemoved>>(DoNothing);
            When<Envelope<StreetNameWasReaddressed>>(DoNothing);
            When<Envelope<StreetNameWasRenamed>>(DoNothing);
            #endregion StreetNames

            When<Envelope<AddressWasMigrated>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);

                context.LastChangedList.RemoveRange(attachedRecords);
            });

            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.AddressPersistentLocalId);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.AddressPersistentLocalId);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.AddressPersistentLocalId);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(boxNumberPersistentLocalId.ToString()), message.Position, context, ct);
                }
            });


            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(boxNumberPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(boxNumberPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var (boxNumberPersistentLocalId, _) in message.Message.AddressBoxNumbers)
                {
                    await GetLastChangedRecordsAndUpdatePosition(boxNumberPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressPositionCrsWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(readdressedBoxNumber.DestinationAddressPersistentLocalId.ToString()), message.Position, context, ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.AddressPersistentLocalId);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.AddressPersistentLocalId.ToString()), message.Position, context, ct);
            });
        }

        private static string GetIdentifier(string persistentLocalId)
        {
            return $"v3.{persistentLocalId}";
        }

        private static void RebuildKeyAndUri(IEnumerable<LastChangedRecord>? attachedRecords, int persistentLocalId)
        {
            if (attachedRecords == null)
            {
                return;
            }

            foreach (var record in attachedRecords)
            {
                if (record.CacheKey != null)
                {
                    record.CacheKey = string.Format(record.CacheKey, persistentLocalId);
                }

                if (record.Uri != null)
                {
                    record.Uri = string.Format(record.Uri, persistentLocalId);
                }
            }
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.JsonLd => $"oslo-v3/address:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.JsonLd => "/v3/adressen/{0}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static Task DoNothing<T>(LastChangedListContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
