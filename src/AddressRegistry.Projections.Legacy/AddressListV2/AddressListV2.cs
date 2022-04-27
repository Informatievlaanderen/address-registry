namespace AddressRegistry.Projections.Legacy.AddressListV2
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using StreetName;

    public class AddressListItemV2
    {
        protected AddressListItemV2()
        { }

        public AddressListItemV2(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            string postalCode,
            string houseNumber,
            AddressStatus status,
            bool removed,
            Instant versionTimestamp)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            Status = status;
            Removed = removed;
            VersionTimestamp = versionTimestamp;
        }

        public AddressListItemV2(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            string postalCode,
            string houseNumber,
            string? boxNumber,
            AddressStatus status,
            bool removed,
            Instant versionTimestamp)
            : this(addressPersistentLocalId, streetNamePersistentLocalId, postalCode, houseNumber, status, removed, versionTimestamp)
        {
            BoxNumber = boxNumber;
        }


        public int AddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public string PostalCode { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public AddressStatus Status { get; set; }

        public bool Removed { get; set; }

        public string LastEventHash { get; set; } = string.Empty;
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class AddressListItemV2Configuration : IEntityTypeConfiguration<AddressListItemV2>
    {
        internal const string TableName = "AddressListV2";

        public void Configure(EntityTypeBuilder<AddressListItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.AddressPersistentLocalId)
                .IsClustered();

            b.Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");

            b.Ignore(x => x.VersionTimestamp);

            b.Property(p => p.StreetNamePersistentLocalId);
            b.Property(p => p.PostalCode);
            b.Property(p => p.HouseNumber);
            b.Property(p => p.BoxNumber);
            b.Property(p => p.Status);
            b.Property(p => p.Removed);
            b.Property(x => x.LastEventHash);

            b.HasIndex(p => p.StreetNamePersistentLocalId);
            b.HasIndex(p => p.BoxNumber);
            b.HasIndex(p => p.HouseNumber);
            b.HasIndex(p => p.PostalCode);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.Removed);
        }
    }
}
