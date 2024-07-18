namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("De verwijdering van het adres werd gecorrigeerd.")]
    public class AddressRemovalWasCorrected : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressRemovalWasCorrected"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("De status van het adres. Mogelijkheden: Current, Proposed, Retired en Rejected.")]
        public AddressStatus Status { get; }

        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object dat deel uitmaakt van het adres.")]
        public string? PostalCode { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Busnummer van het adres.")]
        public string? BoxNumber { get; }

        [EventPropertyDescription("Geometriemethode van de adrespositie. Mogelijkheden: DerivedFromObject, AppointedByAdministrator of Interpolated.")]
        public GeometryMethod GeometryMethod { get; }

        [EventPropertyDescription("Specificatie van het object dat voorgesteld wordt door de adrespositie. Mogelijkheden: BuildingUnit, Stand, Parcel, Lot, Entry, RoadSegment, Municipality of Berth.")]
        public GeometrySpecification GeometrySpecification { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de adrespositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("True wanneer het adres aanduiding kreeg 'officieel toegekend'.")]
        public bool OfficiallyAssigned { get; }

        [EventPropertyDescription("Objectidentificator van het huisnummer adres waaraan het busnummer gelinkt is.")]
        public int? ParentPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressRemovalWasCorrected(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus status,
            PostalCode? postalCode,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry extendedWkbGeometry,
            bool officiallyAssigned,
            AddressPersistentLocalId? parentPersistentLocalId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Status = status;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            ExtendedWkbGeometry = extendedWkbGeometry;
            OfficiallyAssigned = officiallyAssigned;
            ParentPersistentLocalId = parentPersistentLocalId;
        }

        [JsonConstructor]
        private AddressRemovalWasCorrected(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            AddressStatus status,
            string? postalCode,
            string houseNumber,
            string? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            string extendedWkbGeometry,
            bool officiallyAssigned,
            int? parentPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                status,
                !string.IsNullOrEmpty(postalCode) ? new PostalCode(postalCode) : null,
                new HouseNumber(houseNumber),
                !string.IsNullOrEmpty(boxNumber) ? new BoxNumber(boxNumber) : null,
                geometryMethod,
                geometrySpecification,
                new ExtendedWkbGeometry(extendedWkbGeometry.ToByteArray()),
                officiallyAssigned,
                parentPersistentLocalId.HasValue ? new AddressPersistentLocalId(parentPersistentLocalId.Value) : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(Status.ToString());
            fields.Add(!string.IsNullOrEmpty(PostalCode) ? PostalCode : string.Empty);
            fields.Add(HouseNumber);
            fields.Add(!string.IsNullOrEmpty(BoxNumber) ? BoxNumber : string.Empty);
            fields.Add(GeometryMethod.ToString());
            fields.Add(GeometrySpecification.ToString());
            fields.Add(ExtendedWkbGeometry);
            fields.Add(OfficiallyAssigned.ToString());
            fields.Add(ParentPersistentLocalId.HasValue ? ParentPersistentLocalId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
