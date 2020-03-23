namespace AddressRegistry.Api.Legacy.Address.Query
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using Infrastructure.Options;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;
    using Requests;
    using Responses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class AddressBosaQuery
    {
        private readonly AddressBosaContext _context;
        private readonly ResponseOptions _responseOptions;

        public AddressBosaQuery(
            AddressBosaContext context,
            ResponseOptions responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<AddressBosaResponse> Filter(BosaAddressRequest filter)
        {
            var addressesQuery = _context.AddressDetail.AsNoTracking().OrderBy(x => x.PersistentLocalId).Where(x => x.Complete && !x.Removed);
            var streetNamesQuery = _context.StreetNameBosaItems.AsNoTracking().Where(x => x.IsComplete);
            var municipalitiesQuery = _context.MunicipalityBosaItems.AsNoTracking();

            if (filter?.IsOnlyAdresIdRequested == true && int.TryParse(filter.AdresCode?.ObjectId, out var adresId))
            {
                addressesQuery = addressesQuery
                    .Where(a => a.PersistentLocalId == adresId)
                    .ToList()
                    .AsQueryable();

                var address = addressesQuery.FirstOrDefault();
                if (address == null)
                    return new AddressBosaResponse { Adressen = new List<AddressBosaResponseItem>() };

                streetNamesQuery = (await streetNamesQuery
                    .Where(x => x.StreetNameId == address.StreetNameId)
                    .ToListAsync())
                    .AsQueryable();

                var streetName = streetNamesQuery.FirstOrDefault();

                municipalitiesQuery = (await municipalitiesQuery
                    .Where(x => x.NisCode == streetName.NisCode)
                    .ToListAsync())
                    .AsQueryable();
            }

            var gemeenteCodeVersieId = filter?.GemeenteCode?.VersieId == null ? null : new Rfc3339SerializableDateTimeOffset(filter.GemeenteCode.VersieId.Value).ToString();

            var filteredMunicipalities = FilterMunicipalities(
                filter?.GemeenteCode?.ObjectId,
                gemeenteCodeVersieId,
                filter?.Gemeentenaam?.Spelling,
                filter?.Gemeentenaam?.Taal,
                filter?.Gemeentenaam?.SearchType ?? BosaSearchType.Bevat,
                municipalitiesQuery);

            var straatnaamCodeVersieId = filter?.StraatnaamCode?.VersieId == null ? null : new Rfc3339SerializableDateTimeOffset(filter.StraatnaamCode.VersieId.Value).ToString();
            var filteredStreetNames = FilterStreetNames(
                filter?.StraatnaamCode?.ObjectId,
                straatnaamCodeVersieId,
                filter?.Straatnaam?.Spelling,
                filter?.Straatnaam?.Taal,
                filter?.Straatnaam?.SearchType ?? BosaSearchType.Bevat,
                streetNamesQuery,
                filteredMunicipalities);

            var filteredAddresses =
                FilterAddresses(
                    filter?.AdresCode?.ObjectId,
                    filter?.AdresCode?.VersieId,
                    filter?.Huisnummer,
                    filter?.Busnummer,
                    filter?.AdresStatus,
                    filter?.PostCode?.ObjectId,
                    addressesQuery,
                    filteredStreetNames)
                .OrderBy(x => x.PersistentLocalId);

            var municipalities = filteredMunicipalities.Select(x => new { x.NisCode, x.Version }).ToList();
            var streetNames = filteredStreetNames.Select(x => new { x.StreetNameId, x.PersistentLocalId, x.Version, x.NisCode }).ToList();

            var addresses = filteredAddresses
                    .Take(1001)
                    .ToList()
                    .Select(x =>
                    {
                        var streetName = streetNames.First(y => y.StreetNameId == x.StreetNameId);
                        var municipality = municipalities.First(y => y.NisCode == streetName.NisCode);
                        var postalCode = _context
                            .PostalInfoLatestItems
                            .AsNoTracking()
                            .First(y => y.PostalCode == x.PostalCode);

                        return new AddressBosaResponseItem(
                            _responseOptions.PostInfoNaamruimte,
                            _responseOptions.GemeenteNaamruimte,
                            _responseOptions.StraatNaamNaamruimte,
                            _responseOptions.Naamruimte,
                            x.PersistentLocalId.Value,
                            AddressMapper.ConvertFromAddressStatus(x.Status),
                            x.HouseNumber,
                            x.BoxNumber,
                            x.OfficiallyAssigned,
                            AddressMapper.GetAddressPoint(x.Position),
                            AddressMapper.ConvertFromGeometryMethod(x.PositionMethod),
                            AddressMapper.ConvertFromGeometrySpecification(x.PositionSpecification),
                            x.VersionTimestamp.ToBelgianDateTimeOffset(),
                            streetName.PersistentLocalId,
                            streetName.Version,
                            municipality.NisCode,
                            municipality.Version,
                            x.PostalCode,
                            postalCode.Version);
                    })
                    .ToList();

            return new AddressBosaResponse
            {
                Adressen = addresses
            };
        }

        private static IQueryable<AddressDetailItem> FilterAddresses(
            string persistentLocalId,
            DateTimeOffset? version,
            string houseNumber,
            string boxNumber,
            AdresStatus? status,
            string postalCode,
            IQueryable<AddressDetailItem> addresses,
            IQueryable<StreetNameBosaItem> streetNames)
        {
            var filtered = addresses.Join(streetNames,
                address => address.StreetNameId,
                streetName => streetName.StreetNameId,
                (address, street) => address);

            if (!string.IsNullOrEmpty(persistentLocalId))
            {
                if (int.TryParse(persistentLocalId, out var addressId))
                    filtered = filtered.Where(x => x.PersistentLocalId == addressId);
                else
                    return Enumerable.Empty<AddressDetailItem>().AsQueryable();
            }

            if (!string.IsNullOrEmpty(houseNumber))
                filtered = filtered.Where(x => x.HouseNumber.StartsWith(houseNumber));

            if (!string.IsNullOrEmpty(boxNumber))
                filtered = filtered.Where(x => x.BoxNumber.StartsWith(boxNumber));

            if (status.HasValue)
            {
                var mappedStatus = AddressMapper.ConvertFromAdresStatus(status);
                filtered = filtered.Where(x => x.Status == mappedStatus);
            }

            if (!string.IsNullOrEmpty(postalCode))
                filtered = filtered.Where(x => x.PostalCode == postalCode);

            if (version.HasValue)
                filtered = filtered.Where(x => x.VersionTimestampAsDateTimeOffset == version);

            return filtered;
        }

        // https://github.com/Informatievlaanderen/streetname-registry/blob/550a2398077140993d6e60029a1b831c193fb0ad/src/StreetNameRegistry.Api.Legacy/StreetName/Query/StreetNameBosaQuery.cs#L38
        private static IQueryable<StreetNameBosaItem> FilterStreetNames(
            string persistentLocalId,
            string version,
            string streetName,
            Taal? language,
            BosaSearchType searchType,
            IQueryable<StreetNameBosaItem> streetNames,
            IQueryable<MunicipalityBosaItem> filteredMunicipalities)
        {
            var filtered = streetNames.Join(
                filteredMunicipalities,
                street => street.NisCode,
                municipality => municipality.NisCode,
                (street, municipality) => street);

            if (!string.IsNullOrEmpty(persistentLocalId))
                filtered = filtered.Where(m => m.PersistentLocalId == persistentLocalId);

            if (!string.IsNullOrEmpty(version))
                filtered = filtered.Where(m => m.Version == version);

            if (!string.IsNullOrEmpty(streetName))
                filtered = CompareStreetNameByCompareType(
                    filtered,
                    streetName,
                    language,
                    searchType == BosaSearchType.Bevat);
            else if (language.HasValue)
                filtered = ApplyStreetNameLanguageFilter(filtered, language.Value);

            return filtered;
        }

        private static IQueryable<StreetNameBosaItem> ApplyStreetNameLanguageFilter(IQueryable<StreetNameBosaItem> query, Taal language)
        {
            switch (language)
            {
                default:
                case Taal.NL:
                    return query.Where(m => m.NameDutchSearch != null);

                case Taal.FR:
                    return query.Where(m => m.NameFrenchSearch != null);

                case Taal.DE:
                    return query.Where(m => m.NameGermanSearch != null);

                case Taal.EN:
                    return query.Where(m => m.NameEnglishSearch != null);
            }
        }

        private static IQueryable<StreetNameBosaItem> CompareStreetNameByCompareType(
            IQueryable<StreetNameBosaItem> query,
            string searchValue,
            Taal? language,
            bool isContainsFilter)
        {
            var containsValue = searchValue.SanitizeForBosaSearch();
            if (!language.HasValue)
            {
                return isContainsFilter
                    ? query.Where(i =>
                        EF.Functions.Like(i.NameDutchSearch, $"%{containsValue}%") ||
                        EF.Functions.Like(i.NameFrenchSearch, $"%{containsValue}%") ||
                        EF.Functions.Like(i.NameEnglishSearch, $"%{containsValue}%") ||
                        EF.Functions.Like(i.NameGermanSearch, $"%{containsValue}%"))
                    : query.Where(i =>
                        i.NameDutch.Equals(searchValue) ||
                        i.NameFrench.Equals(searchValue) ||
                        i.NameGerman.Equals(searchValue) ||
                        i.NameEnglish.Equals(searchValue));
            }

            switch (language.Value)
            {
                default:
                case Taal.NL:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameDutchSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameDutch.Equals(searchValue));

                case Taal.FR:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameFrenchSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameFrench.Equals(searchValue));

                case Taal.DE:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameGermanSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameGerman.Equals(searchValue));

                case Taal.EN:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameEnglishSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameEnglish.Equals(searchValue));
            }
        }

        // https://github.com/Informatievlaanderen/municipality-registry/blob/054e52fffe13bb4a09f80bf36d221d34ab0aacaa/src/MunicipalityRegistry.Api.Legacy/Municipality/Query/MunicipalityBosaQuery.cs#L83
        private static IQueryable<MunicipalityBosaItem> FilterMunicipalities(
            string nisCode,
            string version,
            string municipalityName,
            Taal? language,
            BosaSearchType searchType,
            IQueryable<MunicipalityBosaItem> municipalities)
        {
            var filtered = municipalities.Where(x => x.IsFlemishRegion);

            if (!string.IsNullOrEmpty(nisCode))
                filtered = filtered.Where(m => m.NisCode == nisCode);

            if (!string.IsNullOrEmpty(version))
                filtered = filtered.Where(m => m.Version == version);

            if (string.IsNullOrEmpty(municipalityName))
            {
                if (language.HasValue)
                    filtered = ApplyMunicipalityLanguageFilter(filtered, language.Value);

                return filtered;
            }

            filtered = CompareMunicipalityByCompareType(filtered,
                municipalityName,
                language,
                searchType == BosaSearchType.Bevat);

            return filtered;
        }

        private static IQueryable<MunicipalityBosaItem> ApplyMunicipalityLanguageFilter(
            IQueryable<MunicipalityBosaItem> query,
            Taal language)
        {
            switch (language)
            {
                default:
                case Taal.NL:
                    return query.Where(m => m.NameDutchSearch != null);

                case Taal.FR:
                    return query.Where(m => m.NameFrenchSearch != null);

                case Taal.DE:
                    return query.Where(m => m.NameGermanSearch != null);

                case Taal.EN:
                    return query.Where(m => m.NameEnglishSearch != null);
            }
        }

        private static IQueryable<MunicipalityBosaItem> CompareMunicipalityByCompareType(
            IQueryable<MunicipalityBosaItem> query,
            string searchValue,
            Taal? language,
            bool isContainsFilter)
        {
            var containsValue = searchValue.SanitizeForBosaSearch();
            if (!language.HasValue)
            {
                return isContainsFilter
                    ? query.Where(i =>
                        EF.Functions.Like(i.NameDutchSearch, $"%{containsValue}%") ||
                        EF.Functions.Like(i.NameFrenchSearch, $"%{containsValue}%") ||
                        EF.Functions.Like(i.NameGermanSearch, $"%{containsValue}%") ||
                        EF.Functions.Like(i.NameEnglishSearch, $"%{containsValue}%"))
                    : query.Where(i =>
                        i.NameDutch.Equals(searchValue) ||
                        i.NameFrench.Equals(searchValue) ||
                        i.NameGerman.Equals(searchValue) ||
                        i.NameEnglish.Equals(searchValue));
            }

            switch (language.Value)
            {
                default:
                case Taal.NL:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameDutchSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameDutch.Equals(searchValue));

                case Taal.FR:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameFrenchSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameFrench.Equals(searchValue));

                case Taal.DE:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameGermanSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameGerman.Equals(searchValue));

                case Taal.EN:
                    return isContainsFilter
                        ? query.Where(i => EF.Functions.Like(i.NameEnglishSearch, $"%{containsValue}%"))
                        : query.Where(i => i.NameEnglish.Equals(searchValue));
            }
        }
    }
}
