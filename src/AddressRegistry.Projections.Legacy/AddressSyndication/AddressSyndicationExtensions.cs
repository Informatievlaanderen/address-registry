namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class AddressSyndicationExtensions
    {
        // Using PersistentLocalId
        public static async Task CreateNewAddressSyndicationItem<T>(
            this LegacyContext context,
            int addressPersistentLocalId,
            Envelope<T> message,
            Action<AddressSyndicationItem> applyEventInfoOn,
            CancellationToken ct)
            where T : IHasProvenance, IMessage
        {
            var addressSyndicationItem = await context.AddressSyndication.LatestPosition(addressPersistentLocalId, ct);

            if (addressSyndicationItem == null)
                throw DatabaseItemNotFound(addressPersistentLocalId);

            var provenance = message.Message.Provenance;

            var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newAddressSyndicationItem.ApplyProvenance(provenance);
            newAddressSyndicationItem.SetEventData(message.Message, message.EventName);

            await context
                .AddressSyndication
                .AddAsync(newAddressSyndicationItem, ct);
        }

        // Using PersistentLocalId
        public static async Task<AddressSyndicationItem> LatestPosition(
            this DbSet<AddressSyndicationItem> addressSyndicationItems,
            int addressPersistentLocalId,
            CancellationToken ct)
            => addressSyndicationItems
                   .Local
                   .Where(x => x.PersistentLocalId == addressPersistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await addressSyndicationItems
                   .Where(x => x.PersistentLocalId == addressPersistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        // Using PersistentLocalId
        public static ProjectionItemNotFoundException<AddressSyndicationProjections> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressSyndicationProjections>(addressPersistentLocalId.ToString());

        public static async Task CreateNewAddressSyndicationItem<T>(
            this LegacyContext context,
            Guid addressId,
            Envelope<T> message,
            Action<AddressSyndicationItem> applyEventInfoOn,
            CancellationToken ct)
            where T : IHasProvenance, IMessage
        {
            var addressSyndicationItem = await context.AddressSyndication.LatestPosition(addressId, ct);

            if (addressSyndicationItem == null)
                throw DatabaseItemNotFound(addressId);

            var provenance = message.Message.Provenance;

            var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newAddressSyndicationItem.ApplyProvenance(provenance);
            newAddressSyndicationItem.SetEventData(message.Message, message.EventName);

            await context
                .AddressSyndication
                .AddAsync(newAddressSyndicationItem, ct);
        }

        public static async Task<AddressSyndicationItem> LatestPosition(
            this DbSet<AddressSyndicationItem> addressSyndicationItems,
            Guid addressId,
            CancellationToken ct)
            => addressSyndicationItems
                   .Local
                   .Where(x => x.AddressId == addressId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await addressSyndicationItems
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

        public static void SetEventData<T>(this AddressSyndicationItem syndicationItem, T message, string eventName)
        {
            var xmlElement = message.ToXml(eventName);
            using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
            {
                using (var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
                    xmlElement.WriteTo(xmlWriter);

                syndicationItem.EventDataAsXml = stringWriter.GetStringBuilder().ToString();
            }
        }

        public static ProjectionItemNotFoundException<AddressSyndicationProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<AddressSyndicationProjections>(addressId.ToString("D"));
    }
}
