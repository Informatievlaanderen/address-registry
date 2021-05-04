namespace AddressRegistry.Api.Legacy.Address.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.AddressSyndication;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class AddressSyndicationQueryResult
    {
        public bool ContainsEvent { get; }
        public bool ContainsObject { get; }

        public Guid AddressId { get; }
        public long Position { get; }
        public int? PersistentLocalId { get; }
        public string HouseNumber { get; }
        public string BoxNumber { get; }
        public string ChangeType { get; }
        public Guid? StreetNameId { get; }
        public string PostalCode { get; set; }
        public byte[] PointPosition { get; set; }
        public GeometryMethod? GeometryMethod { get; set; }
        public GeometrySpecification? GeometrySpecification { get; set; }
        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public AddressStatus? Status { get; }
        public bool IsComplete { get; }
        public bool IsOfficiallyAssigned { get; }
        public Organisation? Organisation { get; }
        public string Reason { get; }
        public string EventDataAsXml { get; }

        public AddressSyndicationQueryResult(
            Guid addressId,
            long position,
            int? persistentLocalId,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason)
        {
            ContainsObject = false;
            ContainsEvent = false;

            AddressId = addressId;
            Position = position;
            PersistentLocalId = persistentLocalId;

            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
            IsComplete = isComplete;
            Organisation = organisation;
            Reason = reason;
        }

        public AddressSyndicationQueryResult(
            Guid addressId,
            long position,
            int? persistentLocalId,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            Organisation? organisation,
            string reason,
            string eventDataAsXml)
            : this(
                addressId,
                position,
                persistentLocalId,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                organisation,
                reason)
        {
            ContainsEvent = true;

            EventDataAsXml = eventDataAsXml;
        }

        public AddressSyndicationQueryResult(
            Guid addressId,
            long position,
            int? persistentLocalId,
            string houseNumber,
            string boxNumber,
            Guid? streetNameId,
            string postalCode,
            byte[] pointPosition,
            GeometryMethod? geometryMethod,
            GeometrySpecification? geometrySpecification,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            bool isOfficiallyAssigned,
            AddressStatus? status,
            Organisation? organisation,
            string reason)
            : this(
                addressId,
                position,
                persistentLocalId,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                organisation,
                reason)
        {
            ContainsObject = true;

            AddressId = addressId;
            Position = position;
            PersistentLocalId = persistentLocalId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            StreetNameId = streetNameId;
            PostalCode = postalCode;
            PointPosition = pointPosition;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
            IsComplete = isComplete;
            IsOfficiallyAssigned = isOfficiallyAssigned;
            Status = status;
            Organisation = organisation;
            Reason = reason;
        }

        public AddressSyndicationQueryResult(
            Guid addressId,
            long position,
            int? persistentLocalId,
            string houseNumber,
            string boxNumber,
            Guid? streetNameId,
            string postalCode,
            byte[] pointPosition,
            GeometryMethod? geometryMethod,
            GeometrySpecification? geometrySpecification,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            bool isOfficiallyAssigned,
            AddressStatus? status,
            Organisation? organisation,
            string reason,
            string eventDataAsXml)
            : this(
                addressId,
                position,
                persistentLocalId,
                houseNumber,
                boxNumber,
                streetNameId,
                postalCode,
                pointPosition,
                geometryMethod,
                geometrySpecification,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                isOfficiallyAssigned,
                status,
                organisation,
                reason)
        {
            ContainsEvent = true;

            EventDataAsXml = eventDataAsXml;
        }
    }

    public class AddressSyndicationQuery : Query<AddressSyndicationItem, AddressSyndicationFilter, AddressSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embedEvent;
        private readonly bool _embedObject;

        public AddressSyndicationQuery(LegacyContext context, SyncEmbedValue embed)
        {
            _context = context;
            _embedEvent = embed?.Event ?? false;
            _embedObject = embed?.Object ?? false;
        }

        protected override ISorting Sorting => new AddressSyndicationSorting();

        protected override Expression<Func<AddressSyndicationItem, AddressSyndicationQueryResult>> Transformation
        {
            get
            {
                if (_embedEvent && _embedObject)
                    return x => new AddressSyndicationQueryResult(
                        x.AddressId.Value,
                        x.Position,
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
                    x.PersistentLocalId,
                    x.ChangeType,
                    x.RecordCreatedAt,
                    x.LastChangedOn,
                    x.IsComplete,
                    x.Organisation,
                    x.Reason);
            }
        }

        protected override IQueryable<AddressSyndicationItem> Filter(FilteringHeader<AddressSyndicationFilter> filtering)
        {
            var addresses = _context
                .AddressSyndication
                .OrderBy(x => x.Position)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return addresses;

            if (filtering.Filter.Position.HasValue)
                addresses = addresses.Where(m => m.Position >= filtering.Filter.Position);

            return addresses;
        }
    }

    internal class AddressSyndicationSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(AddressSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(AddressSyndicationItem.Position), SortOrder.Ascending);
    }

    public class AddressSyndicationFilter
    {
        public long? Position { get; set; }
        public SyncEmbedValue Embed { get; set; }
    }
}
