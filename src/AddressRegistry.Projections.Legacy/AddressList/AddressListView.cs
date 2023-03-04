namespace AddressRegistry.Projections.Legacy.AddressList
{
    using System;
    using System.Collections.Generic;
    using Address;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class AddressListViewItem
    {
        public int PersistentLocalId { get; set; }

        public Guid StreetNameId { get; set; }
        public string? PostalCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public AddressStatus? Status { get; set; }
        public DateTimeOffset VersionTimestamp { get; set; }

        public Instant VersionTimestampAsInstant => Instant.FromDateTimeOffset(VersionTimestamp);

        public string? StreetNamePersistentLocalId { get; set; }
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
        public Taal? PrimaryLanguage { get; set; }
        public string? MunicipalityNameDutch { get; set; }
        public string? MunicipalityNameDutchSearch { get; set; }
        public string? MunicipalityNameEnglish { get; set; }
        public string? MunicipalityNameEnglishSearch { get; set; }
        public string? MunicipalityNameFrench { get; set; }
        public string? MunicipalityNameFrenchSearch { get; set; }
        public string? MunicipalityNameGerman { get; set; }
        public string? MunicipalityNameGermanSearch { get; set; }

        public KeyValuePair<Taal, string> DefaultMunicipalityName
            => PrimaryLanguage switch
            {
                Taal.NL => new KeyValuePair<Taal, string>(Taal.NL, MunicipalityNameDutch),
                Taal.FR => new KeyValuePair<Taal, string>(Taal.FR, MunicipalityNameFrench),
                Taal.DE => new KeyValuePair<Taal, string>(Taal.DE, MunicipalityNameGerman),
                Taal.EN => new KeyValuePair<Taal, string>(Taal.EN, MunicipalityNameEnglish),
                _ => new KeyValuePair<Taal, string>(Taal.NL, MunicipalityNameDutch)
            };

        public KeyValuePair<Taal, string> DefaultStreetNameName
            => PrimaryLanguage switch
            {
                Taal.NL => new KeyValuePair<Taal, string>(Taal.NL, StreetNameDutch),
                Taal.FR => new KeyValuePair<Taal, string>(Taal.FR, StreetNameFrench),
                Taal.DE => new KeyValuePair<Taal, string>(Taal.DE, StreetNameGerman),
                Taal.EN => new KeyValuePair<Taal, string>(Taal.EN, StreetNameEnglish),
                _ => new KeyValuePair<Taal, string>(Taal.NL, StreetNameDutch)
            };
    }

    public class AddressListViewItemConfiguration : IEntityTypeConfiguration<AddressListViewItem>
    {
        public const string ViewName = "vw_AddressList";

        public void Configure(EntityTypeBuilder<AddressListViewItem> builder)
        {
            builder
                .ToSqlQuery($"SELECT * FROM [{Schema.Legacy}].[{ViewName}] WITH (NOEXPAND)")
                .HasKey(x  => x.PersistentLocalId);

            builder.Ignore(x => x.VersionTimestampAsInstant);
        }
    }
}
