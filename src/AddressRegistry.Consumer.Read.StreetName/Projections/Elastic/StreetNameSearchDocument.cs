namespace AddressRegistry.Consumer.Read.StreetName.Projections.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Infrastructure.Elastic;

    public sealed class StreetNameSearchDocument
    {
        public int StreetNamePersistentLocalId { get; set; }
        public DateTimeOffset VersionTimestamp { get; set; }
        public StreetNameStatus Status { get; set; }

        public bool Active => Status is StreetNameStatus.Proposed or StreetNameStatus.Current;

        public Municipality Municipality { get; set; }

        public Name[] Names { get; set; }
        public Name[] HomonymAdditions { get; set; }

        public Name[] FullStreetName => BuildFullStreetName();


        public StreetNameSearchDocument(
            int streetNamePersistentLocalId,
            DateTimeOffset versionTimestamp,
            StreetNameStatus status,
            Municipality municipality,
            Name[] names,
            Name[]? homonymAdditions)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            VersionTimestamp = versionTimestamp;
            Status = status;
            Municipality = municipality;
            Names = names;
            HomonymAdditions = homonymAdditions ?? [];
        }

        private Name[] BuildFullStreetName()
        {
            var fullNames = new List<Name>();
            foreach (var name in Names)
            {
                fullNames.Add(
                    new Name(FormatFullAddress(
                            name.Spelling,
                            HomonymAdditions.SingleOrDefault(x => x.Language == name.Language)?.Spelling,
                            Municipality.Names.SingleOrDefault(x => x.Language == name.Language)?.Spelling ?? Municipality.Names.First().Spelling),
                        name.Language));
            }

            return fullNames.ToArray();
        }

        private static string FormatFullAddress(
            string streetName,
            string? homonym,
            string municipality)
        {
            return string.IsNullOrWhiteSpace(homonym)
                ? $"{streetName}, {municipality}".Replace("  ", " ")
                : $"{streetName}, {municipality} ({homonym})".Replace("  ", " ");
        }
    }

    public sealed class Municipality
    {
        public string NisCode { get; set; }
        public Name[] Names { get; set; }

        public Municipality()
        { }

        public Municipality(string nisCode, IEnumerable<Name> names)
        {
            NisCode = nisCode;
            Names = names.ToArray();
        }

        public static Municipality FromMunicipalityLatestItem(MunicipalityLatestItem municipalityLatestItem)
        {
            return new Municipality(
                municipalityLatestItem.NisCode,
                new[]
                    {
                        new Name(municipalityLatestItem.NameDutch, Language.nl),
                        new Name(municipalityLatestItem.NameFrench, Language.fr),
                        new Name(municipalityLatestItem.NameGerman, Language.de),
                        new Name(municipalityLatestItem.NameEnglish, Language.en)
                    }
                    .Where(x => !string.IsNullOrEmpty(x.Spelling)));
        }
    }
}
