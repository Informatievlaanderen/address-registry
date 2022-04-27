namespace AddressRegistry.StreetName.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;

    [EventTags(Tag.StreetName)]
    [EventName(EventName)]
    [EventDescription("Het adres werd gemigreerd naar straatnaam.")]
    public class AddressWasMigratedToStreetName : IStreetNameEvent
    {
        public const string EventName = "AddressWasMigratedToStreetName"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Interne GUID van het gemigreerde adres.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Interne GUID van de straatnaam.")]
        public Guid StreetNameId { get; } //TODO: Need?

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("De status van het adres. Mogelijkheden: Current, Proposed en Retired en Rejected.")]
        public AddressStatus Status { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("Busnummer van het adres.")]
        public string? BoxNumber { get; }

        [EventPropertyDescription("Geometriemethode van de adrespositie. Mogelijkheden: DerivedFromObject, AppointedByAdministrator of Interpolated.")]
        public GeometryMethod GeometryMethod { get; }

        [EventPropertyDescription("Specificatie van het object dat voorgesteld wordt door de adrespositie. Mogelijkheden: BuildingUnit, Stand, Parcel, Lot, Entry, RoadSegment, Municipality of Berth.")]
        public GeometrySpecification GeometrySpecification { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de adrespositie.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("True wanneer het adres aanduiding kreeg 'officieel toegekend'.")]
        public bool OfficiallyAssigned { get; }

        [EventPropertyDescription("Postcode (= objectidentificator) van het PostInfo-object dat deel uitmaakt van het adres.")]
        public string PostalCode { get; }

        [EventPropertyDescription("De inhoud is altijd true en is wanneer het adres voldoet aan het informatiemodel.")]
        public bool IsCompleted { get; }

        [EventPropertyDescription("False wanneer het adres niet werd verwijderd. True wanneer het adres werd verwijderd.")]
        public bool IsRemoved { get; }

        [EventPropertyDescription("Objectidentificator van het huisnummer adres waaraan het busnummer gelinkt is.")]
        public int? ParentPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressWasMigratedToStreetName(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressId addressId,
            AddressStreetNameId streetNameId,
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus status,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            AddressGeometry geometry,
            bool officiallyAssigned,
            PostalCode postalCode,
            bool isCompleted,
            bool isRemoved,
            AddressPersistentLocalId? parentPersistentLocalId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressId = addressId;
            StreetNameId = streetNameId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Status = status;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber ?? (string?)null;
            GeometryMethod = geometry.GeometryMethod;
            GeometrySpecification = geometry.GeometrySpecification;
            ExtendedWkbGeometry = geometry.Geometry.ToString();
            OfficiallyAssigned = officiallyAssigned;
            PostalCode = postalCode;
            IsCompleted = isCompleted;
            IsRemoved = isRemoved;
            ParentPersistentLocalId = parentPersistentLocalId ?? (int?)null;
        }

        [JsonConstructor]
        private AddressWasMigratedToStreetName(
            int streetNamePersistentLocalId,
            Guid addressId,
            Guid streetNameId,
            int addressPersistentLocalId,
            AddressStatus status,
            string houseNumber,
            string? boxNumber,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            string extendedWkbGeometry,
            bool officiallyAssigned,
            string postalCode,
            bool isCompleted,
            bool isRemoved,
            int? parentPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressId(addressId),
                new AddressStreetNameId(streetNameId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                status,
                new HouseNumber(houseNumber),
                new BoxNumber(boxNumber),
                new AddressGeometry(
                    geometryMethod,
                    geometrySpecification,
                    new ExtendedWkbGeometry(extendedWkbGeometry.ToByteArray())),
                officiallyAssigned,
                new PostalCode(postalCode),
                isCompleted,
                isRemoved,
                parentPersistentLocalId.HasValue ? new AddressPersistentLocalId(parentPersistentLocalId.Value) : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressId.ToString("D"));
            fields.Add(StreetNameId.ToString("D"));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(Status.ToString());
            fields.Add(HouseNumber);
            fields.Add(!string.IsNullOrEmpty(BoxNumber) ? BoxNumber : string.Empty);
            fields.Add(GeometryMethod.ToString());
            fields.Add(GeometrySpecification.ToString());
            fields.Add(ExtendedWkbGeometry);
            fields.Add(OfficiallyAssigned.ToString());
            fields.Add(PostalCode);
            fields.Add(IsCompleted.ToString());
            fields.Add(IsRemoved.ToString());
            fields.Add(ParentPersistentLocalId.HasValue ? ParentPersistentLocalId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
