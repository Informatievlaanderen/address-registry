namespace AddressRegistry.Api.Oslo.Address.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Convertors;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.AddressListV2;

    public class AddressListOsloQueryV2 : Query<AddressListViewItemV2, AddressFilter>
    {
        private readonly AddressQueryContext _context;

        protected override ISorting Sorting => new AddressSortingV2();

        public AddressListOsloQueryV2(AddressQueryContext context)
        {
            _context = context;
        }

        protected override IQueryable<AddressListViewItemV2> Filter(FilteringHeader<AddressFilter> filtering)
        {
            var addressesV2 = _context
                .AddressListViewV2
                .AsNoTracking()
                .OrderBy(x => x.AddressPersistentLocalId)
                .AsQueryable();

            if (!filtering.ShouldFilter)
            {
                return addressesV2;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.BoxNumber))
            {
                var unescapedBoxNumber = Uri.UnescapeDataString(filtering.Filter.BoxNumber);
                addressesV2 = addressesV2.Where(a => a.BoxNumber == unescapedBoxNumber);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.HouseNumber))
            {
                var unescapedHouseNumber = Uri.UnescapeDataString(filtering.Filter.HouseNumber);
                addressesV2 = addressesV2.Where(a => a.HouseNumber == unescapedHouseNumber);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.PostalCode))
            {
                addressesV2 = addressesV2.Where(a => a.PostalCode == filtering.Filter.PostalCode);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(AdresStatus), filtering.Filter.Status, true, out var status))
                {
                    var addressStatus = StreetNameAddressStatusExtensions.ConvertFromAdresStatus((AdresStatus)status);
                    addressesV2 = addressesV2.Where(a => a.Status == addressStatus);
                }
                else
                {
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    addressesV2 = addressesV2.Where(m => (int)m.Status == -1);
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NisCode))
            {
                addressesV2 = addressesV2.Where(x => x.NisCode == filtering.Filter.NisCode);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var searchName = filtering.Filter.MunicipalityName.RemoveDiacritics();

                addressesV2 = addressesV2.Where(m =>
                    m.MunicipalityNameDutchSearch == searchName ||
                    m.MunicipalityNameFrenchSearch == searchName ||
                    m.MunicipalityNameGermanSearch == searchName ||
                    m.MunicipalityNameEnglishSearch == searchName);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.StreetName))
            {
                var searchName = filtering.Filter.StreetName.RemoveDiacritics();

                addressesV2 = addressesV2.Where(s =>
                    s.StreetNameDutchSearch == searchName ||
                    s.StreetNameFrenchSearch == searchName ||
                    s.StreetNameGermanSearch == searchName ||
                    s.StreetNameEnglishSearch == searchName);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.HomonymAddition))
            {
                addressesV2 = addressesV2.Where(s =>
                    s.HomonymAdditionDutch == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionFrench == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionGerman == filtering.Filter.HomonymAddition ||
                    s.HomonymAdditionEnglish == filtering.Filter.HomonymAddition);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.StreetNameId))
            {
                if (int.TryParse(filtering.Filter.StreetNameId, out var streetNameId))
                {
                    addressesV2 = addressesV2.Where(x => x.StreetNamePersistentLocalId == streetNameId);
                }
                else
                {
                    // don't bother sending to sql, no results will be returned
                    return new List<AddressListViewItemV2>().AsQueryable();
                }
            }

            return addressesV2;
        }
    }
    internal class AddressSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(AddressListItemV2.BoxNumber),
            nameof(AddressListItemV2.HouseNumber),
            nameof(AddressListItemV2.PostalCode),
            nameof(AddressListItemV2.AddressPersistentLocalId)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(AddressListItemV2.AddressPersistentLocalId), SortOrder.Ascending);
    }
}
