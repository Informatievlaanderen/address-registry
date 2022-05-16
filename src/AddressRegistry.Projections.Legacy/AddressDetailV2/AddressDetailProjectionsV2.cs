namespace AddressRegistry.Projections.Legacy.AddressDetailV2
{
    using System;
    using AddressDetail;
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
                    position: null,
                    positionMethod: null,
                    positionSpecification: null,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(addressDetailItemV2, message);

                await context
                    .AddressDetailV2
                    .AddAsync(addressDetailItemV2, ct);
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

        private static void UpdateVersionTimestamp(AddressDetailItem addressDetailItem, Instant versionTimestamp)
            => addressDetailItem.VersionTimestamp = versionTimestamp;
    }
}
