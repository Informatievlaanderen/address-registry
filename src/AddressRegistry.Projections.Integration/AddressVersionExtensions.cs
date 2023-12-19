﻿namespace AddressRegistry.Projections.Integration
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class AddressVersionExtensions
    {
        public static async Task CreateNewAddressVersion<T>(
            this IntegrationContext context,
            PersistentLocalId persistentLocalId,
            Envelope<T> message,
            Action<AddressVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var municipalitySyndicationItem = await context.LatestPosition(persistentLocalId, ct);

            if (municipalitySyndicationItem is null)
            {
                throw DatabaseItemNotFound(persistentLocalId);
            }

            var provenance = message.Message.Provenance;

            var newAddressVersion = municipalitySyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                provenance.Timestamp,
                applyEventInfoOn);

            await context
                .AddressVersions
                .AddAsync(newAddressVersion, ct);
        }

        private static async Task<AddressVersion?> LatestPosition(
            this IntegrationContext context,
            PersistentLocalId persistentLocalId,
            CancellationToken ct)
            => context
                   .AddressVersions
                   .Local
                   .Where(x => x.PersistentLocalId == persistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .AddressVersions
                   .Where(x => x.PersistentLocalId == persistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<AddressVersionProjections> DatabaseItemNotFound(PersistentLocalId persistentLocalId)
            => new(persistentLocalId);
    }
}
