namespace AddressRegistry.Api.Legacy.Address.Query
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Bosa;
    using Infrastructure.Options;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;
    using Requests;
    using Responses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AddressBosaQuery
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly ResponseOptions _responseOptions;

        public AddressBosaQuery(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            ResponseOptions responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressBosaResponse> Filter(BosaAddressRequest filter)
        {
            var addressesQuery = _legacyContext.AddressDetail.AsNoTracking().Where(x => x.Complete);
            var streetNamesQuery = _syndicationContext.StreetNameBosaItems.AsNoTracking().Where(x => x.IsComplete);
            var municipalitiesQuery = _syndicationContext.MunicipalityBosaItems.AsNoTracking();

            if (filter?.IsOnlyAdresIdRequested == true && int.TryParse(filter.AdresCode?.ObjectId, out var adresId))
            {
                addressesQuery = addressesQuery
                    .Where(a => a.PersistentLocalId == adresId)
                    .ToList()
                    .AsQueryable();

                var address = addressesQuery.FirstOrDefault();
                if (address == null)
                    return new AddressBosaResponse { Adressen = new List<AddressBosaResponseItem>(), TotaalAantal = 0 };

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

            var filteredMunicipalities = FilterMunicipalities(
                filter?.GemeenteCode?.ObjectId,
                filter?.GemeenteCode?.VersieId,
                filter?.Gemeentenaam?.Spelling,
                filter?.Gemeentenaam?.Taal,
                filter?.Gemeentenaam?.SearchType,
                municipalitiesQuery);

            var filteredStreetNames = FilterStreetNames(
                filter?.StraatnaamCode?.ObjectId,
                filter?.StraatnaamCode?.VersieId,
                filter?.Straatnaam?.Spelling,
                filter?.Straatnaam?.Taal,
                filter?.Straatnaam?.SearchType,
                streetNamesQuery,
                filteredMunicipalities).ToList();

            var filteredAddresses =
                FilterAddresses(
                    filter?.AdresCode?.ObjectId,
                    filter?.AdresCode?.VersieId,
                    filter?.Huisnummer,
                    filter?.Busnummer,
                    filter?.AdresStatus,
                    filter?.PostCode?.ObjectId,
                    addressesQuery,
                    filteredStreetNames.Select(x => x.StreetNameId))
                .OrderBy(x => x.PersistentLocalId);

            var municipalities = filteredMunicipalities.Select(x => new { x.NisCode, x.Version }).ToList();
            var streetNames = filteredStreetNames.Select(x => new { x.StreetNameId, PersistentLocalId = x.PersistentLocalId, x.Version, x.NisCode }).ToList();
            var count = filteredAddresses.Count();

            var addresses = filteredAddresses
                    .Take(1000)
                    .ToList()
                    .Select(x =>
                    {
                        var streetName = streetNames.First(y => y.StreetNameId == x.StreetNameId);
                        var municipality = municipalities.First(y => y.NisCode == streetName.NisCode);

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
                            streetName.Version.Value,
                            municipality.NisCode,
                            municipality.Version.Value,
                            x.PostalCode,
                            new DateTimeOffset(1830, 1, 1, 0, 0, 0, new TimeSpan()));
                    })
                    .ToList();

            return new AddressBosaResponse
            {
                TotaalAantal = count,
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
            IEnumerable<Guid> streetNameIds)
        {
            var filtered = addresses
                .Where(x => streetNameIds.Contains(x.StreetNameId));

            if (!string.IsNullOrEmpty(persistentLocalId))
            {
                if (int.TryParse(persistentLocalId, out var adresId))
                    filtered = filtered.Where(x => x.PersistentLocalId == adresId);
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
                filtered = filtered.Where(x => x.VersionTimestamp.ToBelgianDateTimeOffset() == version);

            return filtered;
        }

        // https://github.com/Informatievlaanderen/streetname-registry/blob/550a2398077140993d6e60029a1b831c193fb0ad/src/StreetNameRegistry.Api.Legacy/StreetName/Query/StreetNameBosaQuery.cs#L38
        private static IQueryable<StreetNameBosaItem> FilterStreetNames(
            string persistentLocalId,
            DateTimeOffset? version,
            string streetName,
            Taal? language,
            BosaSearchType? searchType,
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

            if (version.HasValue)
                filtered = filtered.Where(m => m.Version == version);

            if (!string.IsNullOrEmpty(streetName))
                filtered = CompareStreetNameByCompareType(
                    streetNames,
                    streetName.SanitizeForBosaSearch(),
                    language,
                    searchType == BosaSearchType.Bevat);
            else if (language.HasValue)
                filtered = ApplyStreetNameLanguageFilter(streetNames, language.Value);

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
            if (!language.HasValue)
            {
                return isContainsFilter
                    ? query.Where(i =>
                        i.NameDutchSearch.Contains(searchValue) ||
                        i.NameFrenchSearch.Contains(searchValue) ||
                        i.NameGermanSearch.Contains(searchValue) ||
                        i.NameEnglishSearch.Contains(searchValue))
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
                        ? query.Where(i => i.NameDutchSearch.Contains(searchValue))
                        : query.Where(i => i.NameDutch.Equals(searchValue));

                case Taal.FR:
                    return isContainsFilter
                        ? query.Where(i => i.NameFrenchSearch.Contains(searchValue))
                        : query.Where(i => i.NameFrench.Equals(searchValue));

                case Taal.DE:
                    return isContainsFilter
                        ? query.Where(i => i.NameGermanSearch.Contains(searchValue))
                        : query.Where(i => i.NameGerman.Equals(searchValue));

                case Taal.EN:
                    return isContainsFilter
                        ? query.Where(i => i.NameEnglishSearch.Contains(searchValue))
                        : query.Where(i => i.NameEnglish.Equals(searchValue));
            }
        }

        // https://github.com/Informatievlaanderen/municipality-registry/blob/054e52fffe13bb4a09f80bf36d221d34ab0aacaa/src/MunicipalityRegistry.Api.Legacy/Municipality/Query/MunicipalityBosaQuery.cs#L83
        private static IQueryable<MunicipalityBosaItem> FilterMunicipalities(
            string nisCode,
            DateTimeOffset? version,
            string municipalityName,
            Taal? language,
            BosaSearchType? searchType,
            IQueryable<MunicipalityBosaItem> municipalities)
        {
            var filtered = municipalities.Where(x => x.IsFlemishRegion);

            if (!string.IsNullOrEmpty(nisCode))
                filtered = filtered.Where(m => m.NisCode == nisCode);

            if (version.HasValue)
                filtered = filtered.Where(m => m.Version == version);

            if (string.IsNullOrEmpty(municipalityName))
            {
                if (language.HasValue)
                    filtered = ApplyMunicipalityLanguageFilter(filtered, language.Value);

                return filtered;
            }

            filtered = CompareMunicipalityByCompareType(filtered,
                municipalityName.SanitizeForBosaSearch(),
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
            if (!language.HasValue)
            {
                return isContainsFilter
                    ? query.Where(i =>
                        i.NameDutchSearch.Contains(searchValue) ||
                        i.NameFrenchSearch.Contains(searchValue) ||
                        i.NameGermanSearch.Contains(searchValue) ||
                        i.NameEnglishSearch.Contains(searchValue))
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
                        ? query.Where(i => i.NameDutchSearch.Contains(searchValue))
                        : query.Where(i => i.NameDutch.Equals(searchValue));

                case Taal.FR:
                    return isContainsFilter
                        ? query.Where(i => i.NameFrenchSearch.Contains(searchValue))
                        : query.Where(i => i.NameFrench.Equals(searchValue));

                case Taal.DE:
                    return isContainsFilter
                        ? query.Where(i => i.NameGermanSearch.Contains(searchValue))
                        : query.Where(i => i.NameGerman.Equals(searchValue));

                case Taal.EN:
                    return isContainsFilter
                        ? query.Where(i => i.NameEnglishSearch.Contains(searchValue))
                        : query.Where(i => i.NameEnglish.Equals(searchValue));
            }
        }
    }
}
