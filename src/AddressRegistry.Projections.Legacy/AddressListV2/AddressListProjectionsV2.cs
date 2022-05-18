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
        }

        private static void UpdateVersionTimestamp(AddressListItemV2 addressListItemV2, Instant timestamp)
            => addressListItemV2.VersionTimestamp = timestamp;

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
