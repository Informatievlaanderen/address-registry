namespace AddressRegistry.StreetName.Events
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("Het adres werd voorgesteld door heradressering.")]
    public class AddressWasProposedBecauseOfReaddress : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressWasProposedBecauseOfReaddress"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het bronAdres.")]
        public int SourceAddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het huisnummer adres waaraan het busnummer gelinkt is.")]
        public int? ParentPersistentLocalId { get; }

        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object dat deel uitmaakt van het adres.")]
        public string PostalCode { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Busnummer van het adres.")]
        public string? BoxNumber { get; }

        [EventPropertyDescription("Geometriemethode van de adrespositie. Mogelijkheden: DerivedFromObject of AppointedByAdministrator.")]
        public GeometryMethod GeometryMethod { get; }

        [EventPropertyDescription("Specificatie van het object dat voorgesteld wordt door de adrespositie. Mogelijkheden: Municipality, Parcel, Lot, Stand, Berth, Entry.")]
        public GeometrySpecification GeometrySpecification { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de adrespositie.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasProposedBecauseOfReaddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocal,
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            AddressPersistentLocalId? parentPersistentLocalId,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocal;
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            ParentPersistentLocalId = parentPersistentLocalId ?? (int?)null;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber ?? (string?)null;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            ExtendedWkbGeometry = extendedWkbGeometry.ToString();
        }

        [JsonConstructor]
        private AddressWasProposedBecauseOfReaddress(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            int sourceAddressPersistentLocalId,
            int? parentPersistentLocalId,
            string postalCode,
            string houseNumber,
            string? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(sourceAddressPersistentLocalId),
                parentPersistentLocalId != null ? new AddressPersistentLocalId(parentPersistentLocalId.Value) : null,
                new PostalCode(postalCode),
                new HouseNumber(houseNumber),
                boxNumber != null ? new BoxNumber(boxNumber) : null,
                geometryMethod,
                geometrySpecification,
                new ExtendedWkbGeometry(extendedWkbGeometry))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public System.Collections.Generic.IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            fields.Add(SourceAddressPersistentLocalId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            fields.Add(HouseNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
            fields.Add(GeometryMethod.ToString());
            fields.Add(GeometrySpecification.ToString());
            fields.Add(ExtendedWkbGeometry.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (BoxNumber is not null)
            {
                fields.Add(BoxNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
