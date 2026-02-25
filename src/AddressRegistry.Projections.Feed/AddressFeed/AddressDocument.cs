namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class AddressDocument
    {
        public int PersistentLocalId { get; set; }
        public bool IsRemoved { get; set; }
        public AddressDocumentContent Document { get; set; }

        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }
        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToBelgianDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set => LastChangedOnAsDateTimeOffset = value.ToBelgianDateTimeOffset();
        }

        private AddressDocument()
        {
            Document = new AddressDocumentContent();
            IsRemoved = false;
        }

        public AddressDocument(
            int persistentLocalId,
            int streetNamePersistentLocalId,
            string houseNumber,
            string? boxNumber,
            string postalCode,
            Instant createdTimestamp)
        {
            PersistentLocalId = persistentLocalId;
            LastChangedOn = createdTimestamp;
            RecordCreatedAt = createdTimestamp;

            Document = new AddressDocumentContent
            {
                PersistentLocalId = persistentLocalId,
                StreetNamePersistentLocalId = streetNamePersistentLocalId,
                HouseNumber = houseNumber,
                BoxNumber = boxNumber,
                PostalCode = postalCode,
                Status = AdresStatus.Voorgesteld,
                OfficiallyAssigned = true,
                VersionId = createdTimestamp.ToBelgianDateTimeOffset()
            };
        }
    }

    public sealed class AddressDocumentContent
    {
        public int PersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }
        public string PostalCode { get; set; }
        public AdresStatus? Status { get; set; }
        public bool OfficiallyAssigned { get; set; }
        public string? PositionAsGml { get; set; }
        public string? PositionGeometryMethod { get; set; }
        public string? PositionSpecification { get; set; }

        public DateTimeOffset VersionId { get; set; }

        public AddressDocumentContent()
        {
            HouseNumber = string.Empty;
            PostalCode = string.Empty;
        }
    }

    public sealed class AddressDocumentConfiguration : IEntityTypeConfiguration<AddressDocument>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public AddressDocumentConfiguration(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Configure(EntityTypeBuilder<AddressDocument> b)
        {
            b.ToTable("AddressDocuments", Schema.Feed)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            b.Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            b.Property(x => x.LastChangedOnAsDateTimeOffset)
                .HasColumnName("LastChangedOn");

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset)
                .HasColumnName("RecordCreatedAt");

            b.Property(x => x.Document)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _serializerSettings),
                    v => JsonConvert.DeserializeObject<AddressDocumentContent>(v, _serializerSettings)!);

            b.Ignore(x => x.LastChangedOn);
            b.Ignore(x => x.RecordCreatedAt);
        }
    }
}
