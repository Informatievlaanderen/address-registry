namespace AddressRegistry.Api.Legacy.Address.Query
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

    public class AddressListQueryV2 : Query<AddressListItemV2, AddressFilter>
    {
        private readonly AddressQueryContext _context;

        protected override ISorting Sorting => new AddressSortingV2();

        public AddressListQueryV2(AddressQueryContext context)
        {
            _context = context;
        }

        protected override IQueryable<AddressListItemV2> Filter(FilteringHeader<AddressFilter> filtering)
        {
            var addresses = _context
                .AddressListV2
                .AsNoTracking()
                .OrderBy(x => x.AddressPersistentLocalId)
                .Where(a => !a.Removed && a.AddressPersistentLocalId != 0);

            var municipalities = _context
                .MunicipalityLatestItems
                .AsNoTracking();

            var streetnames = _context
                .StreetNameLatestItems
                .AsNoTracking()
                .Where(x => x.IsComplete && !x.IsRemoved);

            var filterStreet = false;

            if (!filtering.ShouldFilter)
                return addresses;

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
                addresses = addresses.Where(a => a.PostalCode == filtering.Filter.PostalCode);

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(AdresStatus), filtering.Filter.Status, true, out var status))
                {
                    var addressStatus =  StreetNameAddressStatusExtensions.ConvertFromAdresStatus((AdresStatus)status);
                    addresses = addresses.Where(a => a.Status == addressStatus);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    addresses = addresses.Where(m => (int)m.Status == -1);
            }

            if (!string.IsNullOrEmpty(filtering.Filter.NisCode))
            {
                streetnames = streetnames.Where(x => x.NisCode == filtering.Filter.NisCode);
                municipalities = municipalities.Where(m => m.NisCode == filtering.Filter.NisCode);
                filterStreet = true;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.MunicipalityName))
            {
                var searchName = filtering.Filter.MunicipalityName.RemoveDiacritics();

                var municipalityNisCodes = municipalities.Where(m =>
                        m.NameDutchSearch == searchName ||
                        m.NameFrenchSearch == searchName ||
                        m.NameGermanSearch == searchName ||
                        m.NameEnglishSearch == searchName)
                    .Select(x => x.NisCode)
                    .ToList();

                if (!municipalityNisCodes.Any())
                    return new List<AddressListItemV2>().AsQueryable();

                streetnames = streetnames.Where(x => municipalityNisCodes.Contains(x.NisCode));

                filterStreet = true;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.StreetName))
            {
                var searchName = filtering.Filter.StreetName.RemoveDiacritics();

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
                addresses = addresses
                    .Where(x => streetnames
                        .Select(y => y.PersistentLocalId)
                        .Contains(x.StreetNamePersistentLocalId.ToString()));
            }

            return addresses;
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
