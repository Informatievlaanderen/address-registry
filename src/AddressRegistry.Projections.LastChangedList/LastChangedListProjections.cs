namespace AddressRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using StreetName.Events;

    [ConnectedProjectionName("Cache markering adressen")]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel adressen de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml, AcceptType.JsonLd };

        public LastChangedListProjections()
            : base(SupportedAcceptTypes)
        {
            #region Legacy Events
            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);

                foreach (var record in attachedRecords)
                {
                    if (record.CacheKey != null)
                    {
                        record.CacheKey = string.Format(record.CacheKey, message.Message.PersistentLocalId);
                    }

                    if (record.Uri != null)
                    {
                        record.Uri = string.Format(record.Uri, message.Message.PersistentLocalId);
                    }
                }
            });

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
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

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            #endregion Legacy Events

            When<Envelope<AddressWasMigrated>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);

                context.LastChangedList.RemoveRange(attachedRecords);
            });

            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.AddressPersistentLocalId);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.AddressPersistentLocalId);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(boxNumberPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressPersistentLocalId.ToString(), message.Position, context, ct);
            });
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
                AcceptType.Json => $"legacy/address:{{0}}.{shortenedAcceptType}",
                AcceptType.Xml => $"legacy/address:{{0}}.{shortenedAcceptType}",
                AcceptType.JsonLd => $"oslo/address:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => "/v1/adressen/{0}",
                AcceptType.Xml => "/v1/adressen/{0}",
                AcceptType.JsonLd => "/v2/adressen/{0}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
