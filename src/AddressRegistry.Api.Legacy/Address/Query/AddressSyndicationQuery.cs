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
        public bool ContainsDetails { get; }

        public Guid AddressId { get; }
        public long Position { get; }
        public int? OsloId { get; }
        public string HouseNumber { get; }
        public string BoxNumber { get; }
        public string ChangeType { get; }
        public Guid? StreetNameId { get; }
        public string PostalCode { get; set; }
        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public AddressStatus? Status { get; }
        public bool IsComplete { get; }
        public Organisation? Organisation { get; }
        public Plan? Plan { get; }

        public AddressSyndicationQueryResult(
            Guid addressId,
            long position,
            int? osloId,
            string houseNumber,
            string boxNumber,
            Guid? streetNameId,
            string postalCode,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            AddressStatus? status,
            Organisation? organisation,
            Plan? plan)
        {
            ContainsDetails = false;

            AddressId = addressId;
            Position = position;
            OsloId = osloId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            StreetNameId = streetNameId;
            PostalCode = postalCode;
            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
            IsComplete = isComplete;
            Status = status;
            Organisation = organisation;
            Plan = plan;
        }
    }

    public class AddressSyndicationQuery : Query<AddressSyndicationItem, AddressSyndicationFilter, AddressSyndicationQueryResult>
    {
        private readonly LegacyContext _context;

        public AddressSyndicationQuery(LegacyContext context) => _context = context;

        protected override ISorting Sorting => new AddressSyndicationSorting();

        protected override Expression<Func<AddressSyndicationItem, AddressSyndicationQueryResult>> Transformation =>
            x => new AddressSyndicationQueryResult(
                x.AddressId.Value,
                x.Position,
                x.OsloId,
                x.HouseNumber,
                x.BoxNumber,
                x.StreetNameId,
                x.PostalCode,
                x.ChangeType,
                x.RecordCreatedAt,
                x.LastChangedOn,
                x.IsComplete,
                x.Status,
                x.Organisation,
                x.Plan);

        protected override IQueryable<AddressSyndicationItem> Filter(FilteringHeader<AddressSyndicationFilter> filtering)
        {
            var addresses = _context
                .AddressSyndication
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
    }
}
