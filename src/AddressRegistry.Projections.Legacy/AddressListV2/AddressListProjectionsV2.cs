namespace AddressRegistry.Projections.Legacy.AddressListV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("API endpoint lijst adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor de adressen lijst voorziet.")]
    public class AddressListProjectionsV2 : ConnectedProjection<LegacyContext>
    {
        public AddressListProjectionsV2()
        {
            // StreetName
            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressListItemV2(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(item, message);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressListItemV2(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(item, message);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressListItemV2(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(item, message);
                }
            });

            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var addressListItemV2 = new AddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.Status,
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressListItemV2, message);

                await context
                    .AddressListV2
                    .AddAsync(addressListItemV2, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressListItemV2 = new AddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    status: AddressStatus.Proposed,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressListItemV2, message);

                await context
                    .AddressListV2
                    .AddAsync(addressListItemV2, ct);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var addressListItemV2 = new AddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    status: AddressStatus.Proposed,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressListItemV2, message);

                await context
                    .AddressListV2
                    .AddAsync(addressListItemV2, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressListItemV2(
                        boxNumberPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(boxNumberItem, message);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressListItemV2(
                        boxNumberPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(boxNumberItem, message);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressListItemV2(
                        boxNumberPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.HouseNumber = message.Message.HouseNumber;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(boxNumberItem, message);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                var houseNumberItem = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = message.Message.ReaddressedHouseNumber.SourceStatus;
                        item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(houseNumberItem, message);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressListItemV2(
                        readdressedBoxNumber.DestinationAddressPersistentLocalId,
                        item =>
                        {
                            item.Status = readdressedBoxNumber.SourceStatus;
                            item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            item.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    UpdateHash(boxNumberItem, message);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var addressListItemV2 = new AddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    status: AddressStatus.Proposed,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressListItemV2, message);

                await context
                    .AddressListV2
                    .AddAsync(addressListItemV2, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

             When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressListItemV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status =  message.Message.Status;
                        item.PostalCode = message.Message.PostalCode;
                        item.HouseNumber = message.Message.HouseNumber;
                        item.BoxNumber = message.Message.BoxNumber;
                        item.Removed = false;

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });
        }

        private static void UpdateVersionTimestamp(AddressListItemV2 addressListItemV2, Instant timestamp)
            => addressListItemV2.VersionTimestamp = timestamp;

        private static void UpdateVersionTimestampIfNewer(AddressListItemV2 addressListItemV2, Instant versionTimestamp)
        {
            if(versionTimestamp > addressListItemV2.VersionTimestamp)
            {
                addressListItemV2.VersionTimestamp = versionTimestamp;
            }
        }

        private static void UpdateHash<T>(AddressListItemV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }
    }
}
