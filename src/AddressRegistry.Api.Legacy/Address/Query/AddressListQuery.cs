namespace AddressRegistry.Api.Legacy.Address.Query
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy.AddressList;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Convertors;
    using Microsoft.EntityFrameworkCore;

    public class AddressListQuery : Query<AddressListItem, AddressFilter>
    {
        private readonly AddressQueryContext _context;

        protected override ISorting Sorting => new AddressSorting();

        public AddressListQuery(AddressQueryContext context)
        {
            _context = context;
        }

        protected override IQueryable<AddressListItem> Filter(FilteringHeader<AddressFilter> filtering)
        {
            var addresses = _context
                .AddressList
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .Where(a => a.Complete && !a.Removed && a.PersistentLocalId != 0);

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
                    var addressStatus = ((AdresStatus)status).ConvertFromAdresStatus();
                    addresses = addresses.Where(a => a.Status.HasValue && a.Status.Value == addressStatus);
                }
                else
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    addresses = addresses.Where(m => m.Status.HasValue && (int)m.Status.Value == -1);
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
                    return new List<AddressListItem>().AsQueryable();

                streetnames = streetnames.Where(x => municipalityNisCodes.Contains(x.NisCode));

                //streetnames =
                //    from s in streetnames
                //    join m in municipalities.Where(m =>
                //            m.NameDutchSearch == searchName ||
                //            m.NameFrenchSearch == searchName ||
                //            m.NameGermanSearch == searchName ||
                //            m.NameEnglishSearch == searchName)
                //        on s.NisCode equals m.NisCode
                //    select s;

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
                        .Select(y => y.StreetNameId).Contains(x.StreetNameId));

                //addresses =
                //    from a in addresses
                //    join s in streetnames
                //        on a.StreetNameId equals s.StreetNameId
                //    select a;
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
    }
}
