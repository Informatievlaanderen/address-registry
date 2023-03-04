namespace AddressRegistry.Api.Legacy.Address.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Projections.Legacy.AddressList;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Convertors;
    using Microsoft.EntityFrameworkCore;

    public class AddressListQuery : Query<AddressListViewItem, AddressFilter>
    {
        private readonly AddressQueryContext _context;

        protected override ISorting Sorting => new AddressSorting();

        public AddressListQuery(AddressQueryContext context)
        {
            _context = context;
        }

        protected override IQueryable<AddressListViewItem> Filter(FilteringHeader<AddressFilter> filtering)
        {
            var addresses = _context
                .AddressListView
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .AsQueryable();

            if (!filtering.ShouldFilter)
            {
                return addresses;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.BoxNumber))
            {
                var unescapedBoxNumber = Uri.UnescapeDataString(filtering.Filter.BoxNumber);
                addresses = addresses.Where(a => a.BoxNumber == unescapedBoxNumber);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.HouseNumber))
            {
                var unescapedHouseNumber = Uri.UnescapeDataString(filtering.Filter.HouseNumber);
                addresses = addresses.Where(a => a.HouseNumber == unescapedHouseNumber);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.PostalCode))
            {
                addresses = addresses.Where(a => a.PostalCode == filtering.Filter.PostalCode);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(AdresStatus), filtering.Filter.Status, true, out var status))
                {
                    var addressStatus =  AddressStatusExtensions.ConvertFromAdresStatus((AdresStatus)status);
                    addresses = addresses.Where(a => a.Status.HasValue && a.Status.Value == addressStatus);
                }
                else
                {
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    addresses = addresses.Where(m => m.Status.HasValue && (int)m.Status.Value == -1);
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NisCode))
            {
                addresses = addresses.Where(x => x.NisCode == filtering.Filter.NisCode);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var searchName = filtering.Filter.MunicipalityName.RemoveDiacritics();

                addresses = addresses.Where(m =>
                    m.MunicipalityNameDutchSearch == searchName ||
                    m.MunicipalityNameFrenchSearch == searchName ||
                    m.MunicipalityNameGermanSearch == searchName ||
                    m.MunicipalityNameEnglishSearch == searchName);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.StreetName))
            {
                var searchName = filtering.Filter.StreetName.RemoveDiacritics();

                addresses = addresses.Where(s =>
                    s.StreetNameDutchSearch == searchName ||
                    s.StreetNameFrenchSearch == searchName ||
                    s.StreetNameGermanSearch == searchName ||
                    s.StreetNameEnglishSearch == searchName);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.HomonymAddition))
            {
                addresses = addresses.Where(s =>
                    s.HomonymAdditionDutch == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionFrench == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionGerman == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionEnglish == filtering.Filter.HomonymAddition);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.StreetNameId))
            {
                if (int.TryParse(filtering.Filter.StreetNameId, out _))
                {
                    addresses = addresses.Where(x => x.StreetNamePersistentLocalId == filtering.Filter.StreetNameId);
                }
                else
                {
                    // don't bother sending to sql, no results will be returned
                    return new List<AddressListViewItem>().AsQueryable();
                }
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
        public string Status { get; set; }
        public string? NisCode { get; set; }
        public string? StreetNameId { get; set; }
    }
}
