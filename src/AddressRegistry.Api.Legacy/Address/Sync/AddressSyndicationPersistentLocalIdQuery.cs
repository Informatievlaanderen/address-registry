namespace AddressRegistry.Api.Legacy.Address.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AddressRegistry.Address;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressSyndication;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using StreetName;

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
                        x.Position,
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
                        x.Position,
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
                        x.Position,
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
                    x.Position,
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
                .OrderBy(x => x.Position)
                .AsNoTracking();

            return addressSyndicationItems;
        }
    }

    internal class AddressSyndicationPersistentLocalIdSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(AddressSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(AddressSyndicationItem.Position), SortOrder.Ascending);
    }

    public class AddressSyndicationPersistentLocalIdFilter
    {
        public int? PersistentLocalId { get; set; }
        public SyncEmbedValue Embed { get; set; }
    }
}
