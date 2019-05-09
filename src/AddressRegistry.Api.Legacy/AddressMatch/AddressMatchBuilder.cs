namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    public class AddressMatchBuilder : IEnumerable<AddressMatchBuilder.MunicipalityWrapper>, IProvidesRepresentationsForScoring
    {
        public class StreetNameWrapper : IEnumerable<AddressDetailItem>
        {
            private IList<AddressDetailItem> _addresses;

            public static readonly IEqualityComparer<StreetNameWrapper> Comparer = new PropertyEqualityComparer<StreetNameWrapper, string>(s => s.StreetName.OsloId);
            public static readonly IEqualityComparer<AddressDetailItem> AddressComparer = new PropertyEqualityComparer<AddressDetailItem, int>(a => a.OsloId.Value);

            public StreetNameWrapper()
            {
                _addresses = new List<AddressDetailItem>();
            }

            public StreetNameLatestItem StreetName { get; set; }

            public void AddAddresses(IEnumerable<AddressDetailItem> addresses)
            {
                _addresses = _addresses.Concat(addresses).Distinct(AddressComparer).ToList();
            }

            public IEnumerator<AddressDetailItem> GetEnumerator()
            {
                return _addresses.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class MunicipalityWrapper : IEnumerable<StreetNameWrapper>
        {
            private IList<StreetNameWrapper> _streetNames;

            public MunicipalityWrapper()
            {
                _streetNames = new List<StreetNameWrapper>();
            }

            public string NisCode { get; set; }
            public string Name { get; set; }
            public string PostalCode { get; set; }
            public MunicipalityLatestItem Municipality { get; set; }

            public void AddStreetNames(IEnumerable<StreetNameLatestItem> streetNames)
            {
                _streetNames = _streetNames.Concat(streetNames.Select(s => new StreetNameWrapper { StreetName = s })).Distinct(StreetNameWrapper.Comparer).ToList();
            }

            public void AddStreetName(StreetNameLatestItem streetName)
            {
                AddStreetNames(new[] { streetName });
            }

            public IEnumerator<StreetNameWrapper> GetEnumerator()
            {
                return _streetNames.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal void ClearStreetNames()
            {
                _streetNames.Clear();
            }
        }

        private readonly Dictionary<string, MunicipalityWrapper> _municipalities;
        private readonly List<AddressDetailItem> _rrAddresses;
        private readonly Sanitizer _sanitizer;

        public AddressMatchBuilder(AddressMatchQueryComponents query)
        {
            Query = query;
            _municipalities = new Dictionary<string, MunicipalityWrapper>();
            _rrAddresses = new List<AddressDetailItem>();
            _sanitizer = new Sanitizer();
        }

        public AddressMatchQueryComponents Query { get; }

        public IEnumerator<MunicipalityWrapper> GetEnumerator()
        {
            return _municipalities.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsNisCode(string nisCode)
        {
            return _municipalities.ContainsKey(nisCode);
        }

        internal void ClearAllStreetNames()
        {
            foreach (var municipalityWrapper in _municipalities.Values)
                municipalityWrapper.ClearStreetNames();
        }

        public IEnumerable<string> NisCodes
        {
            get { return _municipalities.Keys; }
        }

        public void AddMunicipalities(IEnumerable<MunicipalityLatestItem> municipalities)
        {
            AddMunicipalitiesByPostalCode(municipalities, null);
        }

        public void AddMunicipalitiesByPostalCode(IEnumerable<MunicipalityLatestItem> municipalities, string postalCodes)
        {
            foreach (var municipality in municipalities)
                _municipalities[municipality.NisCode] = new MunicipalityWrapper { Name = municipality.DefaultName.Value, NisCode = municipality.NisCode, PostalCode = postalCodes, Municipality = municipality };
        }

        public void AddRrAddresses(IEnumerable<AddressDetailItem> rrAddresses)
        {
            _rrAddresses.AddRange(rrAddresses);
        }

        public IEnumerable<StreetNameWrapper> AllStreetNames()
        {
            return this.SelectMany(municipalityWrapper => municipalityWrapper).Distinct(StreetNameWrapper.Comparer);
        }

        public IEnumerable<AddressDetailItem> AllAddresses()
        {
            return AllStreetNames().SelectMany(s => s).Union(_rrAddresses).Distinct(StreetNameWrapper.AddressComparer);
        }

        public IEnumerable<string> GetMatchRepresentationsForScoring()
        {
            IEnumerable<string> municipalityNames = _municipalities.Values.Select(g => g.Name).Concat(new[] { Query.MunicipalityName }).Where(x => x != null).Distinct(StringComparer.InvariantCultureIgnoreCase);


            IEnumerable<StreetNameWrapper> relevantStreetNames = AllStreetNames();
            //if addresses found, forget about the streetnames without addresses
            if (AllAddresses().Any())
                relevantStreetNames = relevantStreetNames.Where(s => s.Any());

            IEnumerable<string> streetNames = relevantStreetNames.Select(sn => sn.StreetName.NameDutch).Concat(new[] { Query.StreetName }).Where(x => x != null).Distinct(StringComparer.InvariantCultureIgnoreCase);

            Func<string, string, string, string> createRepresentation = (municipalityName, streetName, houseNumber) =>
                string.Format("{0}{1}{2}{3},{4}{5}", streetName,
                    !string.IsNullOrEmpty(houseNumber) ? string.Concat(" ", houseNumber) : string.Empty,
                    !string.IsNullOrEmpty(Query.Index) ? string.Concat(" bus ", Query.Index) : string.Empty,
                    !string.IsNullOrEmpty(Query.BoxNumber) ? string.Concat(" bus ", Query.BoxNumber) : string.Empty,
                    !string.IsNullOrEmpty(Query.PostalCode) ? string.Concat(" ", Query.PostalCode) : string.Empty,
                    string.Concat(" ", municipalityName));

            foreach (var municipalityName in municipalityNames)
                if (streetNames.Any())
                    foreach (var streetName in streetNames.Distinct())
                    {
                        var houseNumbers = _sanitizer.Sanitize(streetName, Query.HouseNumber, Query.Index);
                        if (houseNumbers.Count > 1)//create a representation per sanitized housenumber
                            foreach (var houseNumberWithSubaddress in houseNumbers)
                                yield return createRepresentation(municipalityName, streetName, houseNumberWithSubaddress.HouseNumber);
                        else
                            yield return createRepresentation(municipalityName, streetName, Query.HouseNumber);
                    }
                else
                    yield return municipalityName;
        }
    }
}
