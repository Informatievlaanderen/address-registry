namespace AddressRegistry.StreetName.Events
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, Tag.Edit)]
    [EventName(EventName)]
    [EventDescription("De adres werd voorgesteld.")]
    public class AddressWasProposedV2 : IStreetNameEvent
    {
        public const string EventName = "AddressWasApprovedV2"; // BE CAREFUL CHANGING THIS!!

        public int StreetNamePersistentLocalId { get; }
        public int AddressPersistentLocalId { get; }
        public int? ParentPersistentLocalId { get; }
        public string PostalCode { get; }
        public string HouseNumber { get; }
        public string? BoxNumber { get; }

        public ProvenanceData Provenance { get; private set; }

        public AddressWasProposedV2(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocal,
            AddressPersistentLocalId? parentPersistentLocalId,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocal;
            ParentPersistentLocalId = parentPersistentLocalId ?? (int?)null;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber ?? (string?)null;
        }

        [JsonConstructor]
        private AddressWasProposedV2(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            int? parentPersistentLocalId,
            string postalCode,
            string houseNumber,
            string? boxNumber,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                parentPersistentLocalId != null ? new AddressPersistentLocalId(parentPersistentLocalId.Value) : null,
                new PostalCode(postalCode),
                new HouseNumber(houseNumber),
                boxNumber != null ? new BoxNumber(boxNumber) : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public System.Collections.Generic.IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            fields.Add(HouseNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (BoxNumber is not null)
            {
                fields.Add(BoxNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
