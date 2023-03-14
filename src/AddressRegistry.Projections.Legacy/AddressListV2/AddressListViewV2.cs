namespace AddressRegistry.Projections.Legacy.AddressListV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public class AddressListViewItemV2
    {
        public int AddressPersistentLocalId { get; set; }

        public string? PostalCode { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public AddressStatus Status { get; set; }
        public DateTimeOffset VersionTimestamp { get; set; }

        public Instant VersionTimestampAsInstant => Instant.FromDateTimeOffset(VersionTimestamp);

        public int StreetNamePersistentLocalId { get; set; }
        public string? StreetNameDutch { get; set; }
        public string? StreetNameDutchSearch { get; set; }
        public string? StreetNameEnglish { get; set; }
        public string? StreetNameEnglishSearch { get; set; }
        public string? StreetNameFrench { get; set; }
        public string? StreetNameFrenchSearch { get; set; }
        public string? StreetNameGerman { get; set; }
        public string? StreetNameGermanSearch { get; set; }

        public string? HomonymAdditionDutch { get; set; }
        public string? HomonymAdditionEnglish { get; set; }
        public string? HomonymAdditionFrench { get; set; }
        public string? HomonymAdditionGerman { get; set; }

        public string? NisCode { get; set; }

        public string OfficialLanguages { get; private set; }

        public List<string> OfficialLanguagesAsList
        {
            get => DeserializeOfficialLanguages();
            set => OfficialLanguages = JsonConvert.SerializeObject(value);
        }
        public string? MunicipalityNameDutch { get; set; }
        public string? MunicipalityNameDutchSearch { get; set; }
        public string? MunicipalityNameEnglish { get; set; }
        public string? MunicipalityNameEnglishSearch { get; set; }
        public string? MunicipalityNameFrench { get; set; }
        public string? MunicipalityNameFrenchSearch { get; set; }
        public string? MunicipalityNameGerman { get; set; }
        public string? MunicipalityNameGermanSearch { get; set; }

        private List<string> DeserializeOfficialLanguages()
        {
            return string.IsNullOrEmpty(OfficialLanguages)
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(OfficialLanguages) ?? new List<string>();
        }

        public MunicipalityLanguage PrimaryLanguage
        {
            get
            {
                var dictionaryLanguages = new Dictionary<string, MunicipalityLanguage>(new List<KeyValuePair<string, MunicipalityLanguage>>(), StringComparer.OrdinalIgnoreCase)
                {
                    { "Dutch", MunicipalityLanguage.Dutch },
                    { "French", MunicipalityLanguage.French },
                    { "English", MunicipalityLanguage.English },
                    { "German", MunicipalityLanguage.German }
                };

                return dictionaryLanguages[OfficialLanguagesAsList.First()];
            }
        }

        public KeyValuePair<Taal, string> DefaultMunicipalityName
            => PrimaryLanguage switch
            {
                MunicipalityLanguage.Dutch => new KeyValuePair<Taal, string>(Taal.NL, MunicipalityNameDutch),
                MunicipalityLanguage.French => new KeyValuePair<Taal, string>(Taal.FR, MunicipalityNameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string>(Taal.DE, MunicipalityNameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string>(Taal.EN, MunicipalityNameEnglish),
                _ => new KeyValuePair<Taal, string>(Taal.NL, MunicipalityNameDutch)
            };

        public KeyValuePair<Taal, string> DefaultStreetNameName
            => PrimaryLanguage switch
            {
                MunicipalityLanguage.Dutch => new KeyValuePair<Taal, string>(Taal.NL, StreetNameDutch),
                MunicipalityLanguage.French => new KeyValuePair<Taal, string>(Taal.FR, StreetNameFrench),
                MunicipalityLanguage.German => new KeyValuePair<Taal, string>(Taal.DE, StreetNameGerman),
                MunicipalityLanguage.English => new KeyValuePair<Taal, string>(Taal.EN, StreetNameEnglish),
                _ => new KeyValuePair<Taal, string>(Taal.NL, StreetNameDutch)
            };
    }

    public enum MunicipalityLanguage
    {
        Dutch = 0,
        French = 1,
        English = 2,
        German = 3
    }

    public class AddressListViewItemV2Configuration : IEntityTypeConfiguration<AddressListViewItemV2>
    {
        public const string ViewName = "vw_AddressListV2";

        public void Configure(EntityTypeBuilder<AddressListViewItemV2> builder)
        {
            builder
                .ToSqlQuery($"SELECT * FROM [{Schema.Legacy}].[{ViewName}] WITH (NOEXPAND)")
                .HasKey(x  => x.AddressPersistentLocalId);

            builder.Ignore(x => x.VersionTimestampAsInstant);
            builder.Ignore(x => x.OfficialLanguagesAsList);
        }
    }
}
