namespace AddressRegistry.Projections.Legacy.AddressDetailV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NodaTime;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("API endpoint detail adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het adressen detail voorziet.")]
    public class AddressDetailProjectionsV2 : ConnectedProjection<LegacyContext>
    {
        public AddressDetailProjectionsV2()
        {
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var addressDetailItemV2 = new AddressDetailItemV2(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.Status,
                    message.Message.OfficiallyAssigned,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    message.Message.GeometryMethod,
                    message.Message.GeometrySpecification,
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressDetailItemV2, message);

                await context
                    .AddressDetailV2
                    .AddAsync(addressDetailItemV2, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressDetailItemV2 = new AddressDetailItemV2(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    AddressStatus.Proposed,
                    officiallyAssigned: true,
                    position: message.Message.ExtendedWkbGeometry.ToByteArray(),
                    positionMethod: message.Message.GeometryMethod,
                    positionSpecification: message.Message.GeometrySpecification,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressDetailItemV2, message);

                await context
                    .AddressDetailV2
                    .AddAsync(addressDetailItemV2, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
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
                    var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
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
                    var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
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
                    var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
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

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetailV2(
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
                var item = await context.FindAndUpdateAddressDetailV2(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                UpdateHash(item, message);
            });
        }

        private static void UpdateHash<T>(AddressDetailItemV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static void UpdateVersionTimestamp(AddressDetailItemV2 addressDetailItem, Instant versionTimestamp)
            => addressDetailItem.VersionTimestamp = versionTimestamp;
    }
}
