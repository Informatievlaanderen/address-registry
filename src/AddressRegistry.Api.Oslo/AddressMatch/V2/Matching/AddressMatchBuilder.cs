namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using Convertors;

    public class AddressMatchBuilder : IEnumerable<AddressMatchBuilder.MunicipalityWrapper>, IProvidesRepresentationsForScoring
    {
        public class StreetNameWrapper : IEnumerable<AddressDetailItemV2>
        {
            private IList<AddressDetailItemV2> _addresses;

            public static readonly IEqualityComparer<StreetNameWrapper> Comparer = new PropertyEqualityComparer<StreetNameWrapper, int>(s => s.StreetName.PersistentLocalId);
            public static readonly IEqualityComparer<AddressDetailItemV2> AddressComparer = new PropertyEqualityComparer<AddressDetailItemV2, int>(a => a.AddressPersistentLocalId);

            public StreetNameWrapper()
                => _addresses = new List<AddressDetailItemV2>();

            public StreetNameLatestItem StreetName { get; set; }

            public void AddAddresses(IEnumerable<AddressDetailItemV2> addresses)
                => _addresses = _addresses
                    .Concat(addresses)
                    .Distinct(AddressComparer)
                    .ToList();

            public IEnumerator<AddressDetailItemV2> GetEnumerator()
                => _addresses.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        public class MunicipalityWrapper : IEnumerable<StreetNameWrapper>
        {
            private IList<StreetNameWrapper> _streetNames;

            public string? NisCode { get; set; }
            public string Name { get; set; }
            public string? PostalCode { get; set; }
            public MunicipalityLatestItem Municipality { get; set; }

            public MunicipalityWrapper()
                => _streetNames = new List<StreetNameWrapper>();

            public void AddStreetNames(IEnumerable<StreetNameLatestItem?> streetNames)
                => _streetNames = _streetNames
                    .Concat(
                        streetNames
                            .Where(streetName => streetName != null)
                            .Select(streetName => new StreetNameWrapper
                            {
                                StreetName = streetName!
                            }))
                    .Distinct(StreetNameWrapper.Comparer)
                    .ToList();

            public void AddStreetName(StreetNameLatestItem streetName)
                => AddStreetNames(new[] { streetName });

            public IEnumerator<StreetNameWrapper> GetEnumerator()
                => _streetNames.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            internal void ClearStreetNames()
                => _streetNames.Clear();
        }

        private readonly Dictionary<string, MunicipalityWrapper> _municipalities;
        private readonly List<AddressDetailItemV2> _rrAddresses;
        private readonly Sanitizer _sanitizer;

        public AddressMatchQueryComponents Query { get; }

        public AddressMatchBuilder(AddressMatchQueryComponents query)
        {
            Query = query;
            _municipalities = new Dictionary<string, MunicipalityWrapper>();
            _rrAddresses = new List<AddressDetailItemV2>();
            _sanitizer = new Sanitizer();
        }

        public IEnumerator<MunicipalityWrapper> GetEnumerator()
            => _municipalities.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public bool ContainsNisCode(string? nisCode)
            => !string.IsNullOrWhiteSpace(nisCode) && _municipalities.ContainsKey(nisCode);

        internal void ClearAllStreetNames()
        {
            foreach (var municipalityWrapper in _municipalities.Values)
            {
                municipalityWrapper.ClearStreetNames();
            }
        }

        public void AddMunicipalities(IEnumerable<MunicipalityLatestItem> municipalities)
            => AddMunicipalitiesByPostalCode(municipalities, null);

        public void AddMunicipalitiesByPostalCode(
            IEnumerable<MunicipalityLatestItem> municipalities,
            string? postalCodes)
        {
            foreach (var municipality in municipalities)
            {
                if (string.IsNullOrWhiteSpace(municipality.NisCode))
                {
                    continue;
                }

                _municipalities[municipality.NisCode] = new MunicipalityWrapper
                {
                    Name = municipality.DefaultName.Value,
                    NisCode = municipality.NisCode,
                    PostalCode = postalCodes,
                    Municipality = municipality
                };
            }
        }

        public IEnumerable<StreetNameWrapper> AllStreetNames()
            => this.SelectMany(municipalityWrapper => municipalityWrapper).Distinct(StreetNameWrapper.Comparer);

        public IEnumerable<AddressDetailItemV2> AllAddresses()
            => AllStreetNames()
                .SelectMany(s => s)
                .Union(_rrAddresses)
                .Distinct(StreetNameWrapper.AddressComparer);

        public IEnumerable<string> GetMatchRepresentationsForScoring()
        {
            var municipalityNames = _municipalities
                .Values
                .Select(g => g.Municipality.GetDefaultName())
                .Concat(new[] { Query.MunicipalityName })
                .Where(x => x != null)
                .Distinct(StringComparer.InvariantCultureIgnoreCase);

            var relevantStreetNames = AllStreetNames();

            // If addresses are found, forget about the streetnames without addresses
            if (AllAddresses().Any())
            {
                relevantStreetNames = relevantStreetNames.Where(s => s.Any());
            }

            string GetStreetName(StreetNameLatestItem streetName)
            {
                var nisCode = streetName.NisCode;
                if (string.IsNullOrWhiteSpace(nisCode))
                {
                    return string.Empty;
                }

                var municipality = _municipalities[nisCode];
                return streetName.GetDefaultName(municipality.Municipality.PrimaryLanguage.ToTaal());
            }

            var streetNames = relevantStreetNames
                .Select(sn => GetStreetName(sn.StreetName))
                .Concat(new[] { Query.StreetName })
                .Where(x => x != null)
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            static string CreateRepresentation(string? postalCode, string municipalityName, string streetName, string? houseNumber, string? boxNumber)
                => string.Format("{0}{1}{2},{3}{4}",
                    streetName,
                    !string.IsNullOrEmpty(houseNumber)
                        ? string.Concat(" ", houseNumber.PadLeft(10, '0'))
                        : string.Empty,
                    !string.IsNullOrEmpty(boxNumber)
                        ? string.Concat(" bus ", boxNumber.PadLeft(10, '0'))
                        : string.Empty,
                    !string.IsNullOrEmpty(postalCode)
                        ? string.Concat(" ", postalCode)
                        : string.Empty,
                    string.Concat(" ", municipalityName));

            static string CleanupStreetName(string street, string housenumber, string boxnumber)
            {
                // This method is not perfect, but it helps clean some user input to get to a better score
                if (!string.IsNullOrWhiteSpace(housenumber) && !string.IsNullOrWhiteSpace(boxnumber))
                {
                    // If one replace is a subset of the other, flip them.
                    if (housenumber.Contains(boxnumber))
                    {
                        street = street
                            .Replace(housenumber, string.Empty, StringComparison.OrdinalIgnoreCase)
                            .Replace(boxnumber, string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        street = street
                            .Replace(boxnumber, string.Empty, StringComparison.OrdinalIgnoreCase)
                            .Replace(housenumber, string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(housenumber))
                {
                    street = street.Replace(housenumber, string.Empty, StringComparison.OrdinalIgnoreCase);
                }
                else if (!string.IsNullOrWhiteSpace(boxnumber))
                {
                    street = street.Replace(boxnumber, string.Empty, StringComparison.OrdinalIgnoreCase);
                }

                return street.Trim();
            }

            foreach (var municipalityName in municipalityNames)
            {
                if (streetNames.Any())
                {
                    foreach (var streetName in streetNames.Distinct())
                    {
                        var houseNumbers = _sanitizer.Sanitize(
                            streetName,
                            Query.HouseNumber,
                            null,
                            true);

                        if (houseNumbers.Count > 1)
                        {
                            // create a representation per sanitized housenumber
                            foreach (var houseNumberWithSubaddress in houseNumbers)
                            {
                                yield return CreateRepresentation(
                                    Query.PostalCode,
                                    municipalityName,
                                    CleanupStreetName(
                                        streetName ?? string.Empty,
                                        houseNumberWithSubaddress.HouseNumber ?? Query.HouseNumber,
                                        houseNumberWithSubaddress.BoxNumber ?? Query.BoxNumber),
                                    houseNumberWithSubaddress.HouseNumber ?? Query.HouseNumber,
                                    houseNumberWithSubaddress.BoxNumber ?? Query.BoxNumber);
                            }
                        }
                        else if (houseNumbers.Count == 1)
                        {
                            var houseNumber = houseNumbers.First();
                            yield return CreateRepresentation(
                                Query.PostalCode,
                                municipalityName,
                                CleanupStreetName(
                                    streetName ?? string.Empty,
                                    houseNumber.HouseNumber ?? Query.HouseNumber,
                                    houseNumber.BoxNumber ?? Query.BoxNumber),
                                houseNumber.HouseNumber ?? Query.HouseNumber,
                                houseNumber.BoxNumber ?? Query.BoxNumber);
                        }
                        else
                        {
                            yield return CreateRepresentation(
                                Query.PostalCode,
                                municipalityName,
                                streetName ?? string.Empty,
                                Query.HouseNumber,
                                Query.BoxNumber);
                        }
                    }
                }
                else
                {
                    yield return municipalityName;
                }
            }
        }
    }
}
