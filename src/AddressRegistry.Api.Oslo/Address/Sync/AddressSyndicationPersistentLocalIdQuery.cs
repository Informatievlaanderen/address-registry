namespace AddressRegistry.Api.Oslo.Address.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressSyndication;

    public class AddressSyndicationPersistentLocalIdQuery : Query<AddressSyndicationItem, AddressSyndicationPersistentLocalIdFilter, AddressSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embedEvent;
        private readonly bool _embedObject;

        public AddressSyndicationPersistentLocalIdQuery(LegacyContext context, SyncEmbedValue? embed)
        {
            _context = context;
            _embedEvent = embed?.Event ?? false;
            _embedObject = embed?.Object ?? false;
        }

        protected override ISorting Sorting => new AddressSyndicationPersistentLocalIdSorting();

        protected override Expression<Func<AddressSyndicationItem, AddressSyndicationQueryResult>> Transformation
        {
            get
            {
                if (_embedEvent && _embedObject)
                    return x => new AddressSyndicationQueryResult(
                        x.AddressId.Value,
                        x.FeedPosition,
                        x.StreetNamePersistentLocalId,
                        x.PersistentLocalId,
                        x.HouseNumber,
                        x.BoxNumber,
                        x.StreetNameId,
                        x.PostalCode,
                        x.PointPosition,
                        x.PositionMethod,
                        x.PositionSpecification,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.IsComplete,
                        x.IsOfficiallyAssigned,
                        x.Status,
                        x.Organisation,
                        x.Reason,
                        x.EventDataAsXml);

                if (_embedEvent)
                    return x => new AddressSyndicationQueryResult(
                        x.AddressId.Value,
                        x.FeedPosition,
                        x.StreetNamePersistentLocalId,
                        x.PersistentLocalId,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.IsComplete,
                        x.Organisation,
                        x.Reason,
                        x.EventDataAsXml);

                if (_embedObject)
                    return x => new AddressSyndicationQueryResult(
                        x.AddressId.Value,
                        x.FeedPosition,
                        x.StreetNamePersistentLocalId,
                        x.PersistentLocalId,
                        x.HouseNumber,
                        x.BoxNumber,
                        x.StreetNameId,
                        x.PostalCode,
                        x.PointPosition,
                        x.PositionMethod,
                        x.PositionSpecification,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.IsComplete,
                        x.IsOfficiallyAssigned,
                        x.Status,
                        x.Organisation,
                        x.Reason);

                return x => new AddressSyndicationQueryResult(
                    x.AddressId.Value,
                    x.FeedPosition,
                    x.StreetNamePersistentLocalId,
                    x.PersistentLocalId,
                    x.ChangeType,
                    x.RecordCreatedAt,
                    x.LastChangedOn,
                    x.IsComplete,
                    x.Organisation,
                    x.Reason);
            }
        }

        protected override IQueryable<AddressSyndicationItem> Filter(FilteringHeader<AddressSyndicationPersistentLocalIdFilter> filtering)
        {
            var addressSyndicationItems = _context
                .AddressSyndication
                .Where(x => x.PersistentLocalId == filtering.Filter.PersistentLocalId)
                .OrderBy(x => x.FeedPosition)
                .AsNoTracking();

            if (filtering.Filter.Position.HasValue)
                addressSyndicationItems = addressSyndicationItems.Where(m => m.FeedPosition >= filtering.Filter.Position);

            return addressSyndicationItems;
        }
    }

    internal class AddressSyndicationPersistentLocalIdSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } =
        [
            nameof(AddressSyndicationItem.FeedPosition)
        ];

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(AddressSyndicationItem.FeedPosition), SortOrder.Ascending);
    }

    public class AddressSyndicationPersistentLocalIdFilter
    {
        public int? PersistentLocalId { get; set; }
        public long? Position { get; set; }
        public SyncEmbedValue Embed { get; set; } = new SyncEmbedValue();
    }
}
