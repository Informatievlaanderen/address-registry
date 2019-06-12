namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class AddressSyndicationExtensions
    {
        public static async Task CreateNewAddressSyndicationItem<T>(
            this LegacyContext context,
            Guid addressId,
            Envelope<T> message,
            Action<AddressSyndicationItem> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance
        {
            var addressSyndicationItem = await context.LatestPosition(addressId, ct);

            if (addressSyndicationItem == null)
                throw DatabaseItemNotFound(addressId);

            var provenance = message.Message.Provenance;

            var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newAddressSyndicationItem.ApplyProvenance(provenance);
            newAddressSyndicationItem.SetEventData(message.Message);

            await context
                .AddressSyndication
                .AddAsync(newAddressSyndicationItem, ct);
        }

        public static async Task<AddressSyndicationItem> LatestPosition(
            this LegacyContext context,
            Guid addressId,
            CancellationToken ct)
            => context
                   .AddressSyndication
                   .Local
                   .Where(x => x.AddressId == addressId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .AddressSyndication
                   .Where(x => x.AddressId == addressId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        public static void ApplyProvenance(this AddressSyndicationItem item, ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Reason = provenance.Reason;
        }

        public static void SetEventData<T>(this AddressSyndicationItem syndicationItem, T message)
            => syndicationItem.EventDataAsXml = message.ToXml(message.GetType().Name).ToString(SaveOptions.DisableFormatting);

        public static ProjectionItemNotFoundException<AddressSyndicationProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<AddressSyndicationProjections>(addressId.ToString("D"));
    }
}
