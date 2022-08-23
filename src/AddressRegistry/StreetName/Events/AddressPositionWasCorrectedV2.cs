namespace AddressRegistry.StreetName.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Edit, EventTag.For.Sync)]
    [EventName(EventName)]
    [EventDescription("De adrespositie werd gecorrigeerd.")]
    public class AddressPositionWasCorrectedV2 : IStreetNameEvent, IHasAddressPersistentLocalId
    {
        public const string EventName = "AddressPositionWasCorrectedV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van de straatnaam aan dewelke het adres is toegewezen.")]
        public int StreetNamePersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Geometriemethode van de adrespositie. Mogelijkheden: DerivedFromObject of AppointedByAdministrator.")]
        public GeometryMethod GeometryMethod { get; }

        [EventPropertyDescription("Specificatie van het object dat voorgesteld wordt door de adrespositie. Mogelijkheden: Municipality, Street, Parcel, Lot, Stand, Berth, Entry.")]
        public GeometrySpecification GeometrySpecification { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de adrespositie.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public AddressPositionWasCorrectedV2(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            ExtendedWkbGeometry = extendedWkbGeometry.ToString();
        }

        [JsonConstructor]
        private AddressPositionWasCorrectedV2(
            int streetNamePersistentLocalId,
            int addressPersistentLocalId,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                geometryMethod,
                geometrySpecification,
                new ExtendedWkbGeometry(extendedWkbGeometry))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(StreetNamePersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(AddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(GeometryMethod.ToString());
            fields.Add(GeometrySpecification.ToString());
            fields.Add(ExtendedWkbGeometry.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
