namespace AddressRegistry.Api.Legacy.Address.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressList;
    using Projections.Syndication;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;

    public class AddressListQuery : Query<AddressListItem, AddressFilter>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new AddressSorting();

        public AddressListQuery(
            LegacyContext context,
            SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<AddressListItem> Filter(FilteringHeader<AddressFilter> filtering)
        {
            var addresses = _context
                .AddressList
                .AsNoTracking()
                .Where(a => a.Complete && !a.Removed);

            var municipalities = _syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking();

            var streetnames = _syndicationContext
                .StreetNameLatestItems
                .AsNoTracking()
                .Where(x => x.IsComplete);

            var filterStreet = false;

            if (!filtering.ShouldFilter)
                return addresses;

            if (!string.IsNullOrEmpty(filtering.Filter.BoxNumber))
                addresses = addresses.Where(a => a.BoxNumber == filtering.Filter.BoxNumber);

            if (!string.IsNullOrEmpty(filtering.Filter.HouseNumber))
                addresses = addresses.Where(a => a.HouseNumber == filtering.Filter.HouseNumber);

            if (!string.IsNullOrEmpty(filtering.Filter.PostalCode))
                addresses = addresses.Where(a => a.PostalCode == filtering.Filter.PostalCode);

            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var searchName = filtering.Filter.MunicipalityName.RemoveDiacritics();
                var nisCodes = municipalities.Where(m =>
                    m.NameDutchSearch == searchName ||
                    m.NameFrenchSearch == searchName ||
                    m.NameGermanSearch == searchName ||
                    m.NameEnglishSearch == searchName)
                    .Select(m => m.NisCode);

                streetnames = streetnames.Where(s => nisCodes.Contains(s.NisCode));
                filterStreet = true;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.StreetName))
            {
                var searchName = filtering.Filter.MunicipalityName.RemoveDiacritics();
                streetnames = streetnames.Where(s =>
                    s.NameDutchSearch == searchName ||
                    s.NameFrenchSearch == searchName ||
                    s.NameGermanSearch == searchName ||
                    s.NameEnglishSearch == searchName);

                filterStreet = true;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.HomonymAddition))
            {
                streetnames = streetnames.Where(s =>
                    s.HomonymAdditionDutch == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionFrench == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionGerman == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionEnglish == filtering.Filter.HomonymAddition);

                filterStreet = true;
            }

            if (filterStreet)
            {
                var streetnamesList = streetnames.Select(x => x.StreetNameId).ToList();
                addresses = addresses.Where(a => streetnamesList.Contains(a.StreetNameId));
            }

            return addresses;
        }
    }

    internal class AddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(AddressListItem.BoxNumber),
            nameof(AddressListItem.HouseNumber),
            nameof(AddressListItem.PostalCode),
            nameof(AddressListItem.PersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(AddressListItem.PersistentLocalId), SortOrder.Ascending);
    }

    public class AddressFilter
    {
        public string BoxNumber { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public string MunicipalityName { get; set; }
        public string StreetName { get; set; }
        public string HomonymAddition { get; set; }
    }
}
