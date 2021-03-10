namespace AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;

    [ConnectedProjectionName("Legacy - CrabIdToPersistentLocalId")]
    [ConnectedProjectionDescription("Linking the CRAB-id to the GRAR-id")]
    public class CrabIdToPersistentLocalIdProjections : ConnectedProjection<LegacyContext>
    {
        public CrabIdToPersistentLocalIdProjections()
        {
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .CrabIdToPersistentLocalIds
                    .AddAsync(new CrabIdToPersistentLocalIdItem
                    {
                        AddressId = message.Message.AddressId,
                        StreetNameId = message.Message.StreetNameId,
                        HouseNumber = message.Message.HouseNumber,
                    });
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.HouseNumber = message.Message.HouseNumber;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.HouseNumber = message.Message.HouseNumber;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.BoxNumber = message.Message.BoxNumber;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.BoxNumber = message.Message.BoxNumber;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.BoxNumber = null;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.StreetNameId = message.Message.StreetNameId;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.StreetNameId = message.Message.StreetNameId;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PostalCode = message.Message.PostalCode;
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PostalCode = message.Message.PostalCode;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PostalCode = null;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PersistentLocalId = message.Message.PersistentLocalId;
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.IsComplete = true;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.IsComplete = false;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.IsRemoved = true;
                UpdateVersion(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) =>
            {
                var addressId = (Guid)AddressId.CreateFor(new CrabHouseNumberId(message.Message.HouseNumberId));
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(addressId, cancellationToken: ct);

                if (item != null)
                    item.HouseNumberId = message.Message.HouseNumberId;
            });

            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) =>
            {
                var addressId = (Guid)AddressId.CreateFor(new CrabSubaddressId(message.Message.SubaddressId));
                var item = await context.CrabIdToPersistentLocalIds.FindAsync(addressId, cancellationToken: ct);

                item.SubaddressId = message.Message.SubaddressId;
            });

            // Below here - only update version

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });
            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await FindAndUpdateVersion(
                    context,
                    message.Message.AddressId,
                    message.Message.Provenance.Timestamp,
                    ct);
            });

            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static async Task FindAndUpdateVersion(LegacyContext context, Guid addressId, Instant version, CancellationToken ct)
        {
            var item = await context.CrabIdToPersistentLocalIds.FindAsync(addressId, cancellationToken: ct);
            UpdateVersion(item, version);
        }

        private static void UpdateVersion(CrabIdToPersistentLocalIdItem item, Instant timestamp) => item.VersionTimestamp = timestamp;

        private static void DoNothing() { }
    }
}
